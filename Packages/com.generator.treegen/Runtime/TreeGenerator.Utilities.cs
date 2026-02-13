using System.Collections.Generic;
using UnityEngine;

public partial class TreeGenerator
{
    private (float minY, float maxY, float heightRange) CalculateHeightRange(List<List<BranchPoint>> branches)
    {
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        foreach (var branch in branches)
        {
            foreach (BranchPoint point in branch)
            {
                minY = Mathf.Min(minY, point.pos.y);
                maxY = Mathf.Max(maxY, point.pos.y);
            }
        }

        float heightRange = Mathf.Max(0.0001f, maxY - minY);
        return (minY, maxY, heightRange);
    }

    private void CollectLeafEndpoints(List<List<BranchPoint>> branches, float minY, float heightRange, float tipInset, float tipOffset, List<Vector3> endpoints)
    {
        endpoints.Clear();

        for (int i = 0; i < branches.Count; i++)
        {
            List<BranchPoint> branch = branches[i];
            if (branch.Count < 2)
            {
                continue;
            }

            BranchPoint endpoint = branch[branch.Count - 1];
            float endpointHeight = (endpoint.pos.y - minY) / heightRange;
            if (endpointHeight < leafStartHeight || endpoint.radius < minBranchRadiusForLeaves)
            {
                continue;
            }

            Vector3 branchDir = (branch[branch.Count - 1].pos - branch[branch.Count - 2].pos).normalized;
            Vector3 endpointPos = endpoint.pos - branchDir * tipInset + branchDir * tipOffset;
            endpoints.Add(endpointPos);
        }
    }

    private void BuildLeafTargets(List<Vector3> endpoints, float proximityRadius, int targetCount, List<Vector3> positions, List<int> nearbyCounts)
    {
        positions.Clear();
        nearbyCounts.Clear();

        if (endpoints.Count == 0 || targetCount <= 0)
        {
            return;
        }

        float proximityRadiusSqr = proximityRadius * proximityRadius;
        float cellSize = Mathf.Max(0.0001f, proximityRadius);
        var spatialHash = BuildSpatialHash(endpoints, cellSize);

        var data = new List<(Vector3 pos, int count)>(endpoints.Count);
        for (int i = 0; i < endpoints.Count; i++)
        {
            Vector3 pos = endpoints[i];
            int nearbyCount = CountNearbyBranchesSpatial(pos, endpoints, spatialHash, cellSize, proximityRadiusSqr);
            data.Add((pos, nearbyCount));
        }

        data.Sort((a, b) => a.count.CompareTo(b.count));

        int count = Mathf.Min(targetCount, data.Count);
        for (int i = 0; i < count; i++)
        {
            positions.Add(data[i].pos);
            nearbyCounts.Add(data[i].count);
        }
    }

    private static Dictionary<Vector3Int, List<int>> BuildSpatialHash(List<Vector3> positions, float cellSize)
    {
        var hash = new Dictionary<Vector3Int, List<int>>();
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3Int cell = GetSpatialCell(positions[i], cellSize);
            if (!hash.TryGetValue(cell, out List<int> bucket))
            {
                bucket = new List<int>();
                hash[cell] = bucket;
            }
            bucket.Add(i);
        }
        return hash;
    }

    private static int CountNearbyBranchesSpatial(Vector3 position, List<Vector3> positions, Dictionary<Vector3Int, List<int>> spatialHash, float cellSize, float radiusSqr)
    {
        int count = 0;
        Vector3Int baseCell = GetSpatialCell(position, cellSize);
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3Int cell = new Vector3Int(baseCell.x + x, baseCell.y + y, baseCell.z + z);
                    if (!spatialHash.TryGetValue(cell, out List<int> bucket))
                    {
                        continue;
                    }
                    for (int i = 0; i < bucket.Count; i++)
                    {
                        Vector3 other = positions[bucket[i]];
                        if ((position - other).sqrMagnitude < radiusSqr)
                        {
                            count++;
                        }
                    }
                }
            }
        }
        return count;
    }

    private static Vector3Int GetSpatialCell(Vector3 position, float cellSize)
    {
        return new Vector3Int(
            Mathf.FloorToInt(position.x / cellSize),
            Mathf.FloorToInt(position.y / cellSize),
            Mathf.FloorToInt(position.z / cellSize)
        );
    }

    private float CalculateMaxDistanceFromCenter(List<Vector3> positions, Vector3 center)
    {
        float maxDistance = 0f;
        foreach (Vector3 pos in positions)
        {
            float dist = Vector3.Distance(center, pos);
            if (dist > maxDistance) maxDistance = dist;
        }
        return maxDistance;
    }

    private float CalculateClusterSize(Vector3 clusterPosition, int nearbyCount, Vector3 treeCenter, float maxDistance)
    {
        // Ensure isolated branches (low nearby count) get reasonable size to cover tips
        // More branches = larger cluster for main canopy
        float minIsolatedSize = (clusterSizeMin + clusterSizeMax) * 0.5f; // Mid-range for isolated branches
        float proximitySize = nearbyCount <= 2
            ? minIsolatedSize
            : Mathf.Lerp(minIsolatedSize, clusterSizeMax, Mathf.Clamp01((nearbyCount - 2) / (float)(MaxProximityBranchCount - 2)));

        // Size based on distance from center (center = large, edges = small)
        float distanceFromCenter = maxDistance > 0 ? Vector3.Distance(treeCenter, clusterPosition) / maxDistance : 0f;
        float centerDistanceSize = Mathf.Lerp(clusterSizeMax, clusterSizeMin, distanceFromCenter);

        // Combine both factors, but give more weight to proximity for isolated branches
        float sizeMultiplier = Mathf.Lerp(centerDistanceSize, proximitySize, ProximitySizeWeight);

        // Add random variation
        sizeMultiplier *= Random.Range(1f - RandomSizeVariation, 1f + RandomSizeVariation);

        return sizeMultiplier;
    }

    private Vector3 CalculateTreeCenter(List<Vector3> positions)
    {
        if (positions.Count == 0)
            return Vector3.zero;

        Vector3 center = Vector3.zero;
        foreach (Vector3 pos in positions)
        {
            center += pos;
        }
        return center / positions.Count;
    }

    private static Vector3 RotateAroundAxis(Vector3 vector, Vector3 axis, float angleDegrees)
    {
        return Quaternion.AngleAxis(angleDegrees, axis) * vector;
    }

    private static Vector3 GetNoiseVector(Vector3 position, float scale, int seedOffset)
    {
        float x = Mathf.PerlinNoise(position.y * scale + seedOffset, position.z * scale + seedOffset * 2) * 2f - 1f;
        float y = Mathf.PerlinNoise(position.z * scale + seedOffset * 3, position.x * scale + seedOffset * 4) * 2f - 1f;
        float z = Mathf.PerlinNoise(position.x * scale + seedOffset * 5, position.y * scale + seedOffset * 6) * 2f - 1f;
        return new Vector3(x, y, z);
    }

    private static BranchPoint SampleBranchPoint(List<BranchPoint> branch, float t, out Vector3 direction)
    {
        int count = branch.Count;
        int index = Mathf.Clamp(Mathf.RoundToInt(t * (count - 1)), 0, count - 1);
        int nextIndex = Mathf.Min(count - 1, index + 1);
        int prevIndex = Mathf.Max(0, index - 1);

        if (nextIndex != index)
        {
            direction = (branch[nextIndex].pos - branch[index].pos).normalized;
        }
        else
        {
            direction = (branch[index].pos - branch[prevIndex].pos).normalized;
        }

        return branch[index];
    }
}
