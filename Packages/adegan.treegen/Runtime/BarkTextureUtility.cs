using UnityEngine;

/// <summary>
/// Utility class for adding variation to bark textures to reduce repetitiveness
/// </summary>
public static class BarkTextureUtility
{
    /// <summary>
    /// Adds random UV offset to break up texture repetition
    /// </summary>
    public static Vector2 GetRandomUVOffset(int branchSeed)
    {
        Random.InitState(branchSeed);
        return new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    /// <summary>
    /// Applies noise-based variation to UVs
    /// </summary>
    public static Vector2 ApplyUVNoise(Vector2 baseUV, Vector3 worldPosition, float noiseScale, float noiseStrength)
    {
        if (noiseStrength <= 0f) return baseUV;

        // Use Perlin noise based on world position for consistent variation
        float noiseU = Mathf.PerlinNoise(worldPosition.x * noiseScale, worldPosition.z * noiseScale);
        float noiseV = Mathf.PerlinNoise(worldPosition.y * noiseScale, worldPosition.x * noiseScale);

        // Apply noise as offset
        Vector2 noiseOffset = new Vector2(noiseU - 0.5f, noiseV - 0.5f) * noiseStrength;
        return baseUV + noiseOffset;
    }

    /// <summary>
    /// Combines multiple variation techniques
    /// </summary>
    public static Vector2 ApplyAllUVVariation(
        Vector2 baseUV,
        Vector3 worldPosition,
        int branchSeed,
        float uvOffsetStrength,
        float noiseScale,
        float noiseStrength)
    {
        Vector2 uv = baseUV;

        // Apply random offset per branch
        if (uvOffsetStrength > 0f)
        {
            Vector2 offset = GetRandomUVOffset(branchSeed);
            uv += offset * uvOffsetStrength;
        }

        // Apply noise variation
        if (noiseStrength > 0f)
        {
            uv = ApplyUVNoise(uv, worldPosition, noiseScale, noiseStrength);
        }

        return uv;
    }
}
