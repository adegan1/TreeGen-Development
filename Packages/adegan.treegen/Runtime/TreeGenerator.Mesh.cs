using System.Collections.Generic;
using UnityEngine;

public partial class TreeGenerator
{
    private List<List<BranchPoint>> BuildBranchMesh(Mesh mesh)
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        List<List<BranchPoint>> branches = GenerateGuidedGrowthBranches(vertices, triangles, uvs);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return branches;
    }

    private void BuildLeafMesh(GameObject treeObject, List<List<BranchPoint>> branches)
    {
        if (leafMaterial == null || leafDensity <= 0f)
        {
            return;
        }

        var leafVertices = new List<Vector3>();
        var leafTriangles = new List<int>();
        var leafUvs = new List<Vector2>();
        var leafColors = new List<Color>();

        if (leafMode == LeafGenerationMode.Clusters)
        {
            CreateLeafClusters(leafVertices, leafTriangles, leafUvs, leafColors, branches);
        }
        else if (leafMode == LeafGenerationMode.Domes)
        {
            CreateLeafDomes(leafVertices, leafTriangles, leafUvs, leafColors, branches);
        }
        else
        {
            CreatePlaneLeaves(leafVertices, leafTriangles, leafUvs, leafColors, branches);
        }

        if (leafVertices.Count == 0 || leafTriangles.Count == 0)
        {
            return;
        }

        GameObject leafObject = CreateMeshObject("Leaves", leafMaterial, treeObject.transform, out Mesh leafMesh);
        leafMesh.vertices = leafVertices.ToArray();
        leafMesh.triangles = leafTriangles.ToArray();
        leafMesh.uv = leafUvs.ToArray();
        leafMesh.colors = leafColors.ToArray();
        leafMesh.RecalculateNormals();
        leafMesh.RecalculateBounds();

        ConfigureLeafMaterialTransparency(leafObject);
    }

    private void AddTube(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<BranchPoint> points, int segments, Vector3 parentConnectionDir, int branchSeed)
    {
        if (points.Count < 2) return;

        int startVertexIndex = vertices.Count;
        int pointCount = points.Count;

        // Calculate cumulative distances along the branch for proper UV mapping
        float[] cumulativeDistances = new float[pointCount];
        cumulativeDistances[0] = 0f;
        for (int i = 1; i < pointCount; i++)
        {
            cumulativeDistances[i] = cumulativeDistances[i - 1] + Vector3.Distance(points[i - 1].pos, points[i].pos);
        }
        float totalLength = cumulativeDistances[pointCount - 1];

        // If this is a child branch, add an extra ring extruded back along parent direction
        int ringOffset = 0;
        float connectionDistance = 0f;
        if (parentConnectionDir != Vector3.zero && branchBlendDistance > 0f)
        {
            ringOffset = 1;
            connectionDistance = branchBlendDistance;
            Vector3 connectionPos = points[0].pos - parentConnectionDir * branchBlendDistance;
            float radius = points[0].radius;

            // Generate the connection ring at the extruded position
            Vector3 perpendicular = GetPerpendicular(parentConnectionDir);

            for (int j = 0; j < segments; j++)
            {
                float angle = j * Mathf.PI * 2 / segments;
                Vector3 offset = (Mathf.Cos(angle) * perpendicular + Mathf.Sin(angle) * Vector3.Cross(parentConnectionDir, perpendicular)) * radius;
                Vector3 vertexWorldPos = connectionPos + offset;
                vertices.Add(vertexWorldPos);

                float u = (j / (float)segments) * barkTilingHorizontal;
                float v = (-connectionDistance / totalLength) * totalLength * barkTilingVertical;
                Vector2 baseUV = new Vector2(u, v);

                // Apply texture variation
                Vector2 finalUV = BarkTextureUtility.ApplyAllUVVariation(
                    baseUV,
                    vertexWorldPos,
                    branchSeed,
                    barkUVRandomness,
                    barkUVNoiseScale,
                    barkUVNoiseStrength
                );
                uvs.Add(finalUV);
            }
        }

        bool stabilizeTwist = branchSeed == 0;
        Vector3 previousPerpendicular = Vector3.zero;

        // Add all ring vertices for the main branch
        for (int i = 0; i < pointCount; i++)
        {
            BranchPoint point = points[i];
            Vector3 pos = point.pos;
            float radius = point.radius;
            Vector3 direction;
            if (i < pointCount - 1)
            {
                direction = (points[i + 1].pos - pos).normalized;
            }
            else
            {
                direction = (pos - points[i - 1].pos).normalized;
            }

            // Use branch's own direction for all rings
            Vector3 perpendicular;
            if (stabilizeTwist && previousPerpendicular != Vector3.zero)
            {
                Vector3 projected = previousPerpendicular - direction * Vector3.Dot(previousPerpendicular, direction);
                if (projected.sqrMagnitude < 0.0001f)
                {
                    perpendicular = GetPerpendicular(direction);
                }
                else
                {
                    perpendicular = projected.normalized;
                }
            }
            else
            {
                perpendicular = GetPerpendicular(direction);
            }
            previousPerpendicular = perpendicular;

            for (int j = 0; j < segments; j++)
            {
                float angle = j * Mathf.PI * 2 / segments;
                Vector3 offset = (Mathf.Cos(angle) * perpendicular + Mathf.Sin(angle) * Vector3.Cross(direction, perpendicular)) * radius;
                Vector3 vertexWorldPos = pos + offset;
                vertices.Add(vertexWorldPos);

                // Calculate UVs based on actual distance and circumference
                float u = (j / (float)segments) * barkTilingHorizontal;
                float v = (cumulativeDistances[i] / totalLength) * totalLength * barkTilingVertical;
                Vector2 baseUV = new Vector2(u, v);

                // Apply texture variation
                Vector2 finalUV = BarkTextureUtility.ApplyAllUVVariation(
                    baseUV,
                    vertexWorldPos,
                    branchSeed,
                    barkUVRandomness,
                    barkUVNoiseScale,
                    barkUVNoiseStrength
                );
                uvs.Add(finalUV);
            }
        }

        // Add triangles, including connection segment if present
        int totalRings = pointCount + ringOffset;
        for (int i = 0; i < totalRings - 1; i++)
        {
            int ringStart = startVertexIndex + i * segments;
            int nextRingStart = startVertexIndex + (i + 1) * segments;
            for (int j = 0; j < segments; j++)
            {
                int nextJ = (j + 1) % segments;
                // First triangle (reversed for outward normals)
                triangles.Add(ringStart + j);
                triangles.Add(nextRingStart + nextJ);
                triangles.Add(nextRingStart + j);
                // Second triangle
                triangles.Add(ringStart + j);
                triangles.Add(ringStart + nextJ);
                triangles.Add(nextRingStart + nextJ);
            }
        }

        // Add end cap to close off the branch
        BranchPoint lastPoint = points[pointCount - 1];
        int centerVertexIndex = vertices.Count;
        vertices.Add(lastPoint.pos);
        uvs.Add(new Vector2(0.5f, 0.5f));

        int lastRingStart = startVertexIndex + (totalRings - 1) * segments;
        for (int j = 0; j < segments; j++)
        {
            int nextJ = (j + 1) % segments;
            triangles.Add(centerVertexIndex);
            triangles.Add(lastRingStart + j);
            triangles.Add(lastRingStart + nextJ);
        }
    }

    private static Vector3 GetPerpendicular(Vector3 direction)
    {
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
        if (perpendicular == Vector3.zero)
        {
            perpendicular = Vector3.Cross(direction, Vector3.right).normalized;
        }
        return perpendicular;
    }

    private static GameObject CreateMeshObject(string name, Material material, Transform parent, out Mesh mesh)
    {
        GameObject obj = new GameObject(name);
        if (parent != null)
        {
            obj.transform.SetParent(parent, false);
        }

        var meshFilter = obj.AddComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        var meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.material = material;

        return obj;
    }
}
