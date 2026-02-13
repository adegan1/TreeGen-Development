using System.Collections.Generic;
using UnityEngine;

public partial class TreeGenerator
{
    private List<List<BranchPoint>> GenerateGuidedGrowthBranches(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        var allBranches = new List<List<BranchPoint>>();
        Random.State previousState = Random.state;
        if (randomSeed != 0)
        {
            Random.InitState(randomSeed);
        }

        Vector3 basePos = transform.position;
        Quaternion baseRotation = transform.rotation;
        Vector3 canopyCenter = basePos + baseRotation * canopyCenterOffset;

        float height = trunkHeight * Random.Range(1f - trunkHeightVariation, 1f + trunkHeightVariation);
        height = Mathf.Max(segmentLength * 2f, height);
        int trunkSegments = Mathf.Max(2, Mathf.RoundToInt(height / segmentLength));
        float trunkStep = height / trunkSegments;

        Vector3 leanDir = baseRotation * (Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * Vector3.forward);
        Vector3 trunkUp = baseRotation * Vector3.up;

        var trunk = new List<BranchPoint>(trunkSegments + 1);
        Vector3 pos = basePos;
        Vector3 dir = trunkUp;
        float radius = baseThickness;
        trunk.Add(new BranchPoint(pos, radius));

        for (int i = 0; i < trunkSegments; i++)
        {
            float t = (i + 1) / (float)trunkSegments;
            float trunkNoiseScaleFactor = Mathf.Lerp(1f, 0.4f, t);
            Vector3 noise = GetNoiseVector(pos, trunkNoiseScale, randomSeed + 13) * trunkNoiseStrength * trunkNoiseScaleFactor;
            Vector3 lean = leanDir * (trunkLeanStrength * t);
            Vector3 targetDir = (trunkUp + lean + noise).normalized;
            dir = Vector3.Slerp(dir, targetDir, 0.6f).normalized;

            pos += dir * trunkStep;
            radius *= branchThinningRate;
            trunk.Add(new BranchPoint(pos, radius));
        }

        AddTube(vertices, triangles, uvs, trunk, DefaultSegments, Vector3.zero, 0);
        allBranches.Add(trunk);

        List<List<BranchPoint>> parents = new List<List<BranchPoint>> { trunk };
        int totalBranches = 1;
        int branchSeed = 1;
        float goldenAngle = 137.507764f;

        for (int depth = 1; depth <= branchLevels; depth++)
        {
            if (parents.Count == 0)
            {
                break;
            }

            var nextParents = new List<List<BranchPoint>>();
            float depthFactor = depth / Mathf.Max(1f, branchLevels);

            foreach (var parent in parents)
            {
                float densityScale = Mathf.Lerp(1f, branchLevelDensityFalloff, depthFactor);
                int desired = Mathf.Max(1, Mathf.RoundToInt(branchesPerLevel * densityScale));

                for (int i = 0; i < desired; i++)
                {
                    if (maxGeneratedBranches > 0 && totalBranches >= maxGeneratedBranches)
                    {
                        break;
                    }

                    float t = Mathf.Lerp(0.3f, 0.9f, Random.value);
                    BranchPoint startPoint = SampleBranchPoint(parent, t, out Vector3 parentDir);

                    Vector3 axis = parentDir.sqrMagnitude > 0.0001f ? parentDir.normalized : trunkUp;
                    Vector3 perpendicular = GetPerpendicular(axis);
                    float azimuth = i * goldenAngle + Random.Range(-branchTwistJitter, branchTwistJitter);
                    Vector3 outward = RotateAroundAxis(perpendicular, axis, azimuth).normalized;

                    float angle = Random.Range(branchAngleMin, branchAngleMax);
                    Vector3 baseDir = Vector3.Slerp(axis, outward, Mathf.Clamp01(angle / 90f)).normalized;
                    baseDir = Vector3.Slerp(baseDir, trunkUp, Mathf.Clamp01(branchUpwardBias * (1f - depthFactor))).normalized;

                    float length = height * branchLengthFactor * Mathf.Pow(branchLengthFalloff, depth - 1) * Random.Range(0.85f, 1.15f);
                    float startRadius = Mathf.Max(0.001f, startPoint.radius * childBranchThickness);

                    List<BranchPoint> branch = GrowBranch(startPoint.pos, baseDir, length, startRadius, depth, branchSeed, basePos, height, canopyCenter);
                    AddTube(vertices, triangles, uvs, branch, DefaultSegments, axis, branchSeed);
                    allBranches.Add(branch);
                    nextParents.Add(branch);

                    branchSeed++;
                    totalBranches++;
                }
            }

            parents = nextParents;
        }

        Random.state = previousState;
        return allBranches;
    }

    private List<BranchPoint> GrowBranch(Vector3 startPos, Vector3 startDir, float length, float startRadius, int depth, int branchSeed, Vector3 basePos, float trunkHeightValue, Vector3 canopyCenter)
    {
        int segments = Mathf.Max(2, Mathf.RoundToInt(length / segmentLength));
        float step = length / segments;
        var points = new List<BranchPoint>(segments + 1);
        Vector3 pos = startPos;
        Vector3 dir = startDir.normalized;
        float radius = startRadius;

        points.Add(new BranchPoint(pos, radius));

        for (int i = 0; i < segments; i++)
        {
            float t = (i + 1) / (float)segments;
            float depthFactor = depth / Mathf.Max(1f, branchLevels);
            float noiseScaleFactor = Mathf.Lerp(0.6f, 0.2f, t) * Mathf.Lerp(1f, 0.7f, depthFactor);
            Vector3 noise = GetNoiseVector(pos, branchNoiseScale, branchSeed + depth * 31) * branchNoiseStrength * noiseScaleFactor;
            Vector3 droop = Vector3.down * (branchDroop * t * 0.6f);
            Vector3 targetDir = (dir + noise + droop).normalized;

            if (canopyTargetEnabled)
            {
                Vector3 canopyDir = GetCanopyTargetDirection(pos, canopyCenter, canopyRadii, canopySurfaceTarget);
                if (canopyDir.sqrMagnitude > 0.0001f)
                {
                    float attraction = GetCanopyAttraction(pos, basePos, trunkHeightValue);
                    if (attraction > 0f)
                    {
                        targetDir = Vector3.Slerp(targetDir, canopyDir, attraction).normalized;
                    }
                }
            }

            if (targetDir.y < minBranchUpward)
            {
                targetDir.y = minBranchUpward;
                targetDir = targetDir.normalized;
            }
            dir = Vector3.Slerp(dir, targetDir, 0.55f).normalized;

            pos += dir * step;
            if (clampBranchesAboveBase)
            {
                float minY = basePos.y + branchGroundClearance;
                if (pos.y < minY)
                {
                    pos.y = minY;
                    if (dir.y < minBranchUpward)
                    {
                        dir.y = minBranchUpward;
                        dir = dir.normalized;
                    }
                }
            }
            radius *= branchThinningRate;
            points.Add(new BranchPoint(pos, radius));
        }

        return points;
    }

    private float GetCanopyAttraction(Vector3 position, Vector3 basePos, float trunkHeightValue)
    {
        float heightT = Mathf.InverseLerp(basePos.y + trunkHeightValue * canopyHeightStart, basePos.y + trunkHeightValue * canopyHeightEnd, position.y);
        return Mathf.Clamp01(heightT) * canopyAttraction;
    }

    private Vector3 GetCanopyTargetDirection(Vector3 position, Vector3 canopyCenter, Vector3 radii, bool surfaceTarget)
    {
        Vector3 toCenter = canopyCenter - position;
        if (!surfaceTarget || radii.sqrMagnitude < 0.0001f)
        {
            return toCenter.normalized;
        }

        Vector3 fromCenter = position - canopyCenter;
        if (fromCenter.sqrMagnitude < 0.0001f)
        {
            return toCenter.normalized;
        }

        Vector3 dir = fromCenter.normalized;
        Vector3 surfacePos = canopyCenter + new Vector3(dir.x * radii.x, dir.y * radii.y, dir.z * radii.z);
        return (surfacePos - position).normalized;
    }
}
