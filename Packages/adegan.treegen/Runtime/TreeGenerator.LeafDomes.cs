using System.Collections.Generic;
using UnityEngine;

public partial class TreeGenerator
{
    private void CreateLeafDomes(List<Vector3> leafVertices, List<int> leafTriangles, List<Vector2> leafUvs, List<Color> leafColors, List<List<BranchPoint>> branches)
    {
        if (branches.Count == 0) return;

        (float minY, _, float heightRange) = CalculateHeightRange(branches);

        List<Vector3> domePositions = new List<Vector3>();
        List<int> nearbyBranchCounts = new List<int>();
        List<Vector3> allEndpoints = new List<Vector3>();
        int domeSeed = 0;

        float tipInset = domeRadius * 0.15f;
        CollectLeafEndpoints(branches, minY, heightRange, tipInset, domeOffset, allEndpoints);

        Vector3 treeCenter = CalculateTreeCenter(allEndpoints);
        float proximityRadius = domeRadius * ClusterProximityRadiusMultiplier;
        int targetDomeCount = Mathf.RoundToInt(leafDensity);
        BuildLeafTargets(allEndpoints, proximityRadius, targetDomeCount, domePositions, nearbyBranchCounts);

        int domesGenerated = 0;
        int maxDomes = maxLeafCount > 0 ? Mathf.Min(maxLeafCount, domePositions.Count) : domePositions.Count;
        float maxDistance = CalculateMaxDistanceFromCenter(domePositions, treeCenter);

        for (int i = 0; i < domePositions.Count && domesGenerated < maxDomes; i++)
        {
            Vector3 domeCenter = domePositions[i];
            int nearbyCount = nearbyBranchCounts[i];
            float sizeMultiplier = CalculateClusterSize(domeCenter, nearbyCount, treeCenter, maxDistance);
            float radius = domeRadius * sizeMultiplier;
            Quaternion rotation = randomizeDomeRotation
                ? Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f))
                : Quaternion.identity;

            AddLeafDome(leafVertices, leafTriangles, leafUvs, leafColors, domeCenter, radius, domeSegments, rotation, domeTextureTiling, domeSeed++);
            domesGenerated++;
        }
    }

    private void AddLeafDome(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Color> colors, Vector3 center, float radius, int segments, Quaternion rotation, float uvTiling, int domeSeed)
    {
        int rings = segments / 2;
        int segmentsPerRing = segments;
        int startIndex = vertices.Count;
        Color leafColor = new Color(1f, 1f, 1f, leafTransparency);

        Random.State previousState = Random.state;
        Random.InitState(domeSeed * ClusterSeedMultiplier);

        for (int ring = 0; ring <= rings; ring++)
        {
            float phi = (Mathf.PI * 0.5f) * ring / rings;
            float y = Mathf.Cos(phi);
            float ringRadius = Mathf.Sin(phi);

            for (int seg = 0; seg <= segmentsPerRing; seg++)
            {
                float theta = 2f * Mathf.PI * seg / segmentsPerRing;
                float x = ringRadius * Mathf.Cos(theta);
                float z = ringRadius * Mathf.Sin(theta);

                Vector3 domePos = new Vector3(x * domeShapeX, y * domeShapeY, z * domeShapeZ);
                if (domeNoiseStrength > 0f)
                {
                    Vector3 noisePos = domePos * domeNoiseScale;
                    float noise = Mathf.PerlinNoise(noisePos.x + domeSeed, noisePos.y + domeSeed) * 2f - 1f;
                    noise += Mathf.PerlinNoise(noisePos.z + domeSeed * 0.5f, noisePos.x + domeSeed * 0.5f) * 2f - 1f;
                    domePos *= (1f + noise * domeNoiseStrength);
                }

                Vector3 localPos = domePos * radius;
                Vector3 rotatedPos = rotation * localPos;
                Vector3 pos = center + rotatedPos;
                vertices.Add(pos);
                colors.Add(leafColor);

                float u = (float)seg / segmentsPerRing * uvTiling;
                float v = (float)ring / rings * uvTiling;
                Vector2 baseUV = new Vector2(u, v);
                Vector2 finalUV = BarkTextureUtility.ApplyAllUVVariation(
                    baseUV,
                    pos,
                    domeSeed,
                    leafUVRandomness,
                    leafUVNoiseScale,
                    leafUVNoiseStrength
                );
                uvs.Add(finalUV);
            }
        }

        for (int ring = 0; ring < rings; ring++)
        {
            for (int seg = 0; seg < segmentsPerRing; seg++)
            {
                int current = startIndex + ring * (segmentsPerRing + 1) + seg;
                int next = current + segmentsPerRing + 1;

                triangles.Add(current);
                triangles.Add(current + 1);
                triangles.Add(next);

                triangles.Add(current + 1);
                triangles.Add(next + 1);
                triangles.Add(next);
            }
        }

        Random.state = previousState;
    }
}
