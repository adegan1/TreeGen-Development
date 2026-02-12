using System.Collections.Generic;
using UnityEngine;

public partial class TreeGenerator
{
    private void CreateLeafClusters(List<Vector3> leafVertices, List<int> leafTriangles, List<Vector2> leafUvs, List<Color> leafColors, List<List<BranchPoint>> branches)
    {
        if (branches.Count == 0) return;

        (float minY, _, float heightRange) = CalculateHeightRange(branches);

        List<Vector3> clusterPositions = new List<Vector3>();
        List<int> nearbyBranchCounts = new List<int>();
        List<Vector3> allEndpoints = new List<Vector3>();
        int clusterSeed = 0; // For UV randomization

        float tipInset = clusterRadius * 0.2f;
        CollectLeafEndpoints(branches, minY, heightRange, tipInset, clusterOffset, allEndpoints);

        Vector3 treeCenter = CalculateTreeCenter(allEndpoints);
        float clusterProximityRadius = clusterRadius * ClusterProximityRadiusMultiplier;
        int targetClusterCount = Mathf.RoundToInt(leafDensity);
        BuildLeafTargets(allEndpoints, clusterProximityRadius, targetClusterCount, clusterPositions, nearbyBranchCounts);

        // Generate clusters with size based on branch proximity and distance from center
        int clustersGenerated = 0;
        int maxClusters = maxLeafCount > 0 ? Mathf.Min(maxLeafCount, clusterPositions.Count) : clusterPositions.Count;
        float maxDistance = CalculateMaxDistanceFromCenter(clusterPositions, treeCenter);

        for (int i = 0; i < clusterPositions.Count && clustersGenerated < maxClusters; i++)
        {
            Vector3 clusterCenter = clusterPositions[i];
            int nearbyCount = nearbyBranchCounts[i];

            // Calculate cluster size based on proximity and distance from center
            float sizeMultiplier = CalculateClusterSize(clusterCenter, nearbyCount, treeCenter, maxDistance);

            float radius = clusterRadius * sizeMultiplier;

            // Generate random rotation if enabled
            Quaternion rotation = randomizeClusterRotation
                ? Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f))
                : Quaternion.identity;

            AddLeafCluster(leafVertices, leafTriangles, leafUvs, leafColors, clusterCenter, radius, clusterSegments, rotation, clusterTextureTiling, clusterSeed++);
            clustersGenerated++;
        }
    }

    private void AddLeafCluster(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Color> colors, Vector3 center, float radius, int segments, Quaternion rotation, float uvTiling, int clusterSeed)
    {
        // Create an ellipsoidal cluster with organic noise variation
        // This creates horizontal and vertical rings of quads

        int rings = segments / 2;
        int segmentsPerRing = segments;

        int startIndex = vertices.Count;
        Color leafColor = new Color(1f, 1f, 1f, leafTransparency);

        // Seed for consistent noise per cluster without affecting global Random state
        Random.State previousState = Random.state;
        Random.InitState(clusterSeed * ClusterSeedMultiplier);

        // Generate inner cluster vertices
        for (int ring = 0; ring <= rings; ring++)
        {
            float phi = Mathf.PI * ring / rings; // 0 to PI
            float y = Mathf.Cos(phi);
            float ringRadius = Mathf.Sin(phi);

            for (int seg = 0; seg <= segmentsPerRing; seg++)
            {
                float theta = 2f * Mathf.PI * seg / segmentsPerRing; // 0 to 2PI
                float x = ringRadius * Mathf.Cos(theta);
                float z = ringRadius * Mathf.Sin(theta);

                // Create ellipsoid shape by scaling each axis differently
                Vector3 ellipsoidPos = new Vector3(x * clusterShapeX, y * clusterShapeY, z * clusterShapeZ);

                // Add organic noise variation
                if (clusterNoiseStrength > 0f)
                {
                    Vector3 noisePos = ellipsoidPos * clusterNoiseScale;
                    float noise = Mathf.PerlinNoise(noisePos.x + clusterSeed, noisePos.y + clusterSeed) * 2f - 1f;
                    noise += Mathf.PerlinNoise(noisePos.z + clusterSeed * 0.5f, noisePos.x + clusterSeed * 0.5f) * 2f - 1f;
                    ellipsoidPos *= (1f + noise * clusterNoiseStrength);
                }

                // Apply rotation to the cluster vertex
                Vector3 localPos = ellipsoidPos * radius;
                Vector3 rotatedPos = rotation * localPos;
                Vector3 pos = center + rotatedPos;
                vertices.Add(pos);
                colors.Add(leafColor);

                // UV coordinates with tiling and variation
                float u = (float)seg / segmentsPerRing * uvTiling;
                float v = (float)ring / rings * uvTiling;
                Vector2 baseUV = new Vector2(u, v);

                // Apply UV variation using BarkTextureUtility
                Vector2 finalUV = BarkTextureUtility.ApplyAllUVVariation(
                    baseUV,
                    pos,
                    clusterSeed,
                    leafUVRandomness,
                    leafUVNoiseScale,
                    leafUVNoiseStrength
                );
                uvs.Add(finalUV);
            }
        }

        // Generate triangles
        for (int ring = 0; ring < rings; ring++)
        {
            for (int seg = 0; seg < segmentsPerRing; seg++)
            {
                int current = startIndex + ring * (segmentsPerRing + 1) + seg;
                int next = current + segmentsPerRing + 1;

                // First triangle (counter-clockwise for outward normals)
                triangles.Add(current);
                triangles.Add(current + 1);
                triangles.Add(next);

                // Second triangle (counter-clockwise for outward normals)
                triangles.Add(current + 1);
                triangles.Add(next + 1);
                triangles.Add(next);
            }
        }

        // Generate outer transparent shell for depth and texture
        if (enableOuterShell && outerShellThickness > 1f)
        {
            int outerStartIndex = vertices.Count;
            Color outerColor = new Color(1f, 1f, 1f, outerShellTransparency);
            float outerRadius = radius * outerShellThickness;

            // Generate outer shell vertices
            for (int ring = 0; ring <= rings; ring++)
            {
                float phi = Mathf.PI * ring / rings;
                float y = Mathf.Cos(phi);
                float ringRadius = Mathf.Sin(phi);

                for (int seg = 0; seg <= segmentsPerRing; seg++)
                {
                    float theta = 2f * Mathf.PI * seg / segmentsPerRing;
                    float x = ringRadius * Mathf.Cos(theta);
                    float z = ringRadius * Mathf.Sin(theta);

                    // Create ellipsoid shape with slightly more variation on outer shell
                    Vector3 ellipsoidPos = new Vector3(x * clusterShapeX, y * clusterShapeY, z * clusterShapeZ);

                    // Add more pronounced noise to outer shell for wispy effect
                    if (clusterNoiseStrength > 0f)
                    {
                        Vector3 noisePos = ellipsoidPos * clusterNoiseScale * OuterShellNoiseScale;
                        float noise = Mathf.PerlinNoise(noisePos.x + clusterSeed + OuterShellSeedOffset, noisePos.y + clusterSeed + OuterShellSeedOffset) * 2f - 1f;
                        noise += Mathf.PerlinNoise(noisePos.z + clusterSeed * 0.5f + OuterShellSeedOffset, noisePos.x + clusterSeed * 0.5f + OuterShellSeedOffset) * 2f - 1f;
                        ellipsoidPos *= (1f + noise * clusterNoiseStrength * OuterShellNoiseMultiplier);
                    }

                    Vector3 localPos = ellipsoidPos * outerRadius;
                    Vector3 rotatedPos = rotation * localPos;
                    Vector3 pos = center + rotatedPos;
                    vertices.Add(pos);
                    colors.Add(outerColor);

                    // UV coordinates for outer shell
                    float u = (float)seg / segmentsPerRing * uvTiling;
                    float v = (float)ring / rings * uvTiling;
                    Vector2 baseUV = new Vector2(u, v);
                    Vector2 finalUV = BarkTextureUtility.ApplyAllUVVariation(
                        baseUV,
                        pos,
                        clusterSeed + OuterShellSeedOffset,
                        leafUVRandomness,
                        leafUVNoiseScale,
                        leafUVNoiseStrength
                    );
                    uvs.Add(finalUV);
                }
            }

            // Generate triangles for outer shell
            for (int ring = 0; ring < rings; ring++)
            {
                for (int seg = 0; seg < segmentsPerRing; seg++)
                {
                    int current = outerStartIndex + ring * (segmentsPerRing + 1) + seg;
                    int next = current + segmentsPerRing + 1;

                    triangles.Add(current);
                    triangles.Add(current + 1);
                    triangles.Add(next);

                    triangles.Add(current + 1);
                    triangles.Add(next + 1);
                    triangles.Add(next);
                }
            }
        }

        Random.state = previousState;
    }
}
