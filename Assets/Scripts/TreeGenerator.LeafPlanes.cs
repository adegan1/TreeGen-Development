using System.Collections.Generic;
using UnityEngine;

public partial class TreeGenerator
{
    private void CreatePlaneLeaves(List<Vector3> leafVertices, List<int> leafTriangles, List<Vector2> leafUvs, List<Color> leafColors, List<List<BranchPoint>> branches)
    {
        if (branches.Count == 0) return;

        (float minY, float maxY, float heightRange) = CalculateHeightRange(branches);

        // Track total leaf count for performance limiting
        int totalLeavesGenerated = 0;
        bool reachedMaxLeaves = false;
        int leafSeed = 0; // For UV randomization

        float tipBiasPow = Mathf.Lerp(1f, 0.35f, leafTipBias);

        foreach (var branch in branches)
        {
            if (branch.Count < 2) continue;
            if (reachedMaxLeaves) break;

            int segmentCount = branch.Count - 1;
            for (int i = 0; i < segmentCount; i++)
            {
                if (reachedMaxLeaves) break;

                float segmentHeightNormalized = (branch[i].pos.y - minY) / heightRange;
                if (segmentHeightNormalized < leafStartHeight) continue;

                float radius = branch[i].radius;

                // Skip leaves on very thin branches for performance
                if (radius < minBranchRadiusForLeaves) continue;

                // Calculate leaf density based on branch thickness if optimization is enabled
                float densityMultiplier = 1f;
                if (optimizeLeafDistribution)
                {
                    // Reduce leaves on thinner branches (radius-based scaling)
                    densityMultiplier = Mathf.Clamp01(radius / baseThickness);
                }

                float leavesForSegment = leafDensity * densityMultiplier;
                int leafCount = Mathf.FloorToInt(leavesForSegment);
                if (Random.value < (leavesForSegment - leafCount)) leafCount++;

                // Check max leaf limit
                if (maxLeafCount > 0 && totalLeavesGenerated + leafCount > maxLeafCount)
                {
                    leafCount = maxLeafCount - totalLeavesGenerated;
                    reachedMaxLeaves = true;
                }

                Vector3 start = branch[i].pos;
                Vector3 end = branch[i + 1].pos;
                Vector3 direction = (end - start).normalized;
                float branchRadius = branch[i].radius;

                int clumpCount = Mathf.Clamp(
                    Mathf.RoundToInt(Mathf.Lerp(leafCount, Mathf.Max(1, Mathf.CeilToInt(leafCount / 3f)), leafClumpiness)),
                    1,
                    Mathf.Max(1, leafCount)
                );
                List<float> clumpAnchors = new List<float>(clumpCount);
                for (int c = 0; c < clumpCount; c++)
                {
                    float anchorT = 1f - Mathf.Pow(Random.value, tipBiasPow);
                    clumpAnchors.Add(anchorT);
                }

                for (int l = 0; l < leafCount; l++)
                {
                    float anchorT = clumpAnchors[Random.Range(0, clumpAnchors.Count)];
                    float along = Mathf.Clamp01(anchorT + Random.Range(-leafClumpSpread, leafClumpSpread));
                    Vector3 pos = Vector3.Lerp(start, end, along);

                    Vector3 perpendicular = GetPerpendicular(direction);
                    Vector3 binormal = Vector3.Cross(direction, perpendicular).normalized;
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    Vector3 radial = (Mathf.Cos(angle) * perpendicular + Mathf.Sin(angle) * binormal).normalized;
                    float radialOffset = branchRadius + leafDistanceFromBranch + Random.Range(-leafRadialJitter, leafRadialJitter);
                    radialOffset = Mathf.Max(branchRadius, radialOffset);
                    pos += radial * radialOffset;

                    // Add rotation diversity
                    Vector3 upDir = Vector3.Slerp(direction, Vector3.up, leafUpAlignment).normalized;
                    if (upDir.sqrMagnitude < 0.001f)
                    {
                        upDir = direction;
                    }
                    Quaternion rotation = Quaternion.LookRotation(radial, upDir) * Quaternion.Euler(Random.Range(-45f, 45f), Random.Range(0f, 360f), Random.Range(-45f, 45f));

                    // Add size variation
                    float heightSizeMultiplier = Mathf.Lerp(1f, 1f - leafSizeByHeight, segmentHeightNormalized);
                    float sizeVariationMultiplier = (1f + Random.Range(-leafSizeVariation, leafSizeVariation)) * heightSizeMultiplier;
                    float variedWidth = leafWidth * sizeVariationMultiplier;
                    float variedLength = leafLength * sizeVariationMultiplier;

                    AddLeafQuad(leafVertices, leafTriangles, leafUvs, leafColors, pos, rotation, variedWidth, variedLength, doubleSidedLeaves, leafSeed++);
                    totalLeavesGenerated++;
                }
            }
        }
    }

    private void AddLeafQuad(List<Vector3> leafVertices, List<int> leafTriangles, List<Vector2> leafUvs, List<Color> leafColors, Vector3 center, Quaternion rotation, float width, float length, bool doubleSided, int leafSeed)
    {
        int startIndex = leafVertices.Count;
        float halfWidth = width * 0.5f;
        float halfLength = length * 0.5f;

        Vector3 right = rotation * Vector3.right * halfWidth;
        Vector3 up = rotation * Vector3.up * halfLength;

        Color leafColor = new Color(1f, 1f, 1f, leafTransparency);

        leafVertices.Add(center - right - up);
        leafVertices.Add(center + right - up);
        leafVertices.Add(center + right + up);
        leafVertices.Add(center - right + up);

        // Add colors for all vertices
        leafColors.Add(leafColor);
        leafColors.Add(leafColor);
        leafColors.Add(leafColor);
        leafColors.Add(leafColor);

        // Apply UV variation to each corner
        Vector2[] baseUVs = new Vector2[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f)
        };

        for (int i = 0; i < 4; i++)
        {
            Vector3 vertexPos = leafVertices[startIndex + i];
            Vector2 finalUV = BarkTextureUtility.ApplyAllUVVariation(
                baseUVs[i],
                vertexPos,
                leafSeed,
                leafUVRandomness,
                leafUVNoiseScale,
                leafUVNoiseStrength
            );
            leafUvs.Add(finalUV);
        }

        leafTriangles.Add(startIndex + 0);
        leafTriangles.Add(startIndex + 1);
        leafTriangles.Add(startIndex + 2);
        leafTriangles.Add(startIndex + 0);
        leafTriangles.Add(startIndex + 2);
        leafTriangles.Add(startIndex + 3);

        if (doubleSided)
        {
            leafTriangles.Add(startIndex + 2);
            leafTriangles.Add(startIndex + 1);
            leafTriangles.Add(startIndex + 0);
            leafTriangles.Add(startIndex + 3);
            leafTriangles.Add(startIndex + 2);
            leafTriangles.Add(startIndex + 0);
        }
    }
}
