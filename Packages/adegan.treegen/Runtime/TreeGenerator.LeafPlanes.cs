using System.Collections.Generic;
using UnityEngine;

public partial class TreeGenerator
{
    private void CreatePlaneLeaves(List<Vector3> leafVertices, List<int> leafTriangles, List<Vector2> leafUvs, List<Color> leafColors, List<List<BranchPoint>> branches)
    {
        if (branches.Count == 0) return;

        (float minY, _, float heightRange) = CalculateHeightRange(branches);

        // Track total leaf count for performance limiting
        int totalLeavesGenerated = 0;
        bool reachedMaxLeaves = false;
        int leafSeed = 0; // For UV randomization


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

                for (int l = 0; l < leafCount; l++)
                {
                    float along = Random.value;
                    Vector3 pos = Vector3.Lerp(start, end, along);

                    Vector3 perpendicular = GetPerpendicular(direction);
                    Vector3 binormal = Vector3.Cross(direction, perpendicular).normalized;
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    Vector3 radial = (Mathf.Cos(angle) * perpendicular + Mathf.Sin(angle) * binormal).normalized;
                    float radialOffset = branchRadius + leafDistanceFromBranch + Random.Range(-leafRadialJitter, leafRadialJitter);
                    radialOffset = Mathf.Max(branchRadius, radialOffset);
                    pos += radial * radialOffset;

                    // Align leaf length outward from the branch
                    Quaternion rotation = Quaternion.LookRotation(direction, radial);
                    rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), radial) * rotation;

                    // Add size variation and optional vertical taper (larger at bottom, smaller at top)
                    float sizeVariationMultiplier = 1f + Random.Range(-leafSizeVariation, leafSizeVariation);
                    float heightT = Mathf.Clamp01(segmentHeightNormalized);
                    float heightSizeMultiplier = 1f;
                    if (enablePlaneLeafSizeByHeight)
                    {
                        heightSizeMultiplier = Mathf.Lerp(planeLeafSizeBottom, planeLeafSizeTop, heightT);
                    }
                    float variedWidth = leafWidth * sizeVariationMultiplier * heightSizeMultiplier;
                    float variedLength = leafLength * sizeVariationMultiplier * heightSizeMultiplier;

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
        float uvTiling = Mathf.Max(0.01f, planeLeafTextureTiling);
        Vector2[] baseUVs = new Vector2[]
        {
            new Vector2(0f, 0f) * uvTiling,
            new Vector2(1f, 0f) * uvTiling,
            new Vector2(1f, 1f) * uvTiling,
            new Vector2(0f, 1f) * uvTiling
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
