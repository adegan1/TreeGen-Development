using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "TreeGen/Tree Preset", fileName = "TreePreset")]
public class TreePreset : ScriptableObject
{
    [Header("Tree Structure")]
    public string lSystemSeed = "FB";
    [Range(1, 10)]
    public int complexity = 3;
    [Range(0.1f, 5f)]
    public float segmentLength = 1f;

    [Header("Thickness")]
    [Range(0.1f, 2f)]
    public float baseThickness = 0.5f;
    [Range(0.5f, 1.0f)]
    public float branchThinningRate = 0.9f;
    [Range(0.3f, 0.95f)]
    public float childBranchThickness = 0.7f;

    [Header("Leaves")]
    public TreeGenerator.LeafGenerationMode leafMode = TreeGenerator.LeafGenerationMode.Clusters;
    [Range(0.05f, 10f)]
    public float leafWidth = 0.25f;
    [Range(0.05f, 10f)]
    public float leafLength = 0.4f;
    [Range(0f, 20f)]
    public float leafDensity = 1.2f;
    [Range(0f, 1f)]
    public float leafStartHeight = 0.5f;
    [Range(0f, 1f)]
    public float leafSizeVariation = 0.2f;
    public bool doubleSidedLeaves = false;
    [Range(-0.5f, 1f)]
    public float leafDistanceFromBranch = 0.1f;
    [Range(0f, 1f)]
    public float leafClumpiness = 0.5f;
    [Range(0.01f, 1f)]
    public float leafClumpSpread = 0.25f;
    [Range(0f, 1f)]
    public float leafTipBias = 0.6f;
    [Range(0f, 1f)]
    public float leafUpAlignment = 0.7f;
    [Range(0f, 1f)]
    public float leafSizeByHeight = 0.3f;
    [Range(0f, 0.2f)]
    public float leafRadialJitter = 0.04f;

    [Header("Leaf Appearance")]
    [Range(0f, 1f)]
    public float leafTransparency = 1f;
    [Range(0f, 1f)]
    public float leafUVRandomness = 0.2f;
    [Range(0f, 0.5f)]
    public float leafUVNoiseStrength = 0.05f;
    [Range(0.1f, 5f)]
    public float leafUVNoiseScale = 2f;

    [Header("Leaf Clusters (Cluster Mode)")]
    [Range(0.2f, 10f)]
    public float clusterRadius = 0.8f;
    [Range(0.5f, 1f)]
    public float clusterSizeMin = 0.8f;
    [Range(1f, 2f)]
    public float clusterSizeMax = 1.2f;
    [Range(0.5f, 2f)]
    public float clusterShapeX = 1.2f;
    [Range(0.5f, 2f)]
    public float clusterShapeY = 0.8f;
    [Range(0.5f, 2f)]
    public float clusterShapeZ = 1.1f;
    [Range(0f, 0.5f)]
    public float clusterNoiseStrength = 0.15f;
    [Range(0.5f, 5f)]
    public float clusterNoiseScale = 2f;
    public bool enableOuterShell = true;
    [Range(1.05f, 1.5f)]
    public float outerShellThickness = 1.2f;
    [Range(0f, 1f)]
    public float outerShellTransparency = 0.3f;
    [Range(4, 32)]
    public int clusterSegments = 12;
    [Range(0.5f, 10f)]
    public float clusterTextureTiling = 1f;
    public bool randomizeClusterRotation = true;
    [Range(-1f, 2f)]
    public float clusterOffset = 0.3f;

    [Header("Leaf Domes (Dome Mode)")]
    [Range(0.2f, 10f)]
    public float domeRadius = 1.2f;
    [Range(0.5f, 2f)]
    public float domeShapeX = 1.15f;
    [Range(0.5f, 2f)]
    public float domeShapeY = 0.75f;
    [Range(0.5f, 2f)]
    public float domeShapeZ = 1.05f;
    [Range(-1f, 2f)]
    public float domeOffset = 0.5f;
    [Range(4, 32)]
    public int domeSegments = 12;
    [Range(0.5f, 5f)]
    public float domeNoiseScale = 2f;
    [Range(0f, 0.5f)]
    public float domeNoiseStrength = 0.12f;
    public bool randomizeDomeRotation = true;
    [Range(0.5f, 10f)]
    public float domeTextureTiling = 1f;

    [Header("Leaf Performance")]
    [Range(0, 10000)]
    public int maxLeafCount = 2000;
    public bool optimizeLeafDistribution = true;
    [Range(0f, 0.5f)]
    public float minBranchRadiusForLeaves = 0.05f;

    [Header("Branch Connection")]
    [Range(0f, 1f)]
    public float branchBlendDistance = 0.2f;

    [Header("Advanced Growth Parameters")]
    [Range(0f, 100f)]
    public float segmentGrowthChance = 50f;
    [Range(0f, 100f)]
    public float branchPatternVariation = 50f;
    [Range(0f, 90f)]
    public float minBranchAngle = 15f;
    [Range(0f, 90f)]
    public float maxBranchAngle = 45f;
    [Range(-180f, 0f)]
    public float minVerticalAngle = -30f;
    [Range(0f, 180f)]
    public float maxVerticalAngle = 30f;

    [Header("Branch Curvature")]
    [Range(0f, 1f)]
    public float branchBendChance = 0.35f;
    [Range(0f, 10f)]
    public float branchBendStrength = 3f;
}

#if UNITY_EDITOR
public static class TreePresetDefaults
{
    private const string PresetFolder = "Assets/TreePresets";

    public static void CreateDefaultPresets()
    {
        if (!AssetDatabase.IsValidFolder(PresetFolder))
        {
            AssetDatabase.CreateFolder("Assets", "TreePresets");
        }

        CreateOrUpdatePreset(PresetFolder + "/Oak.asset", preset =>
        {
            preset.lSystemSeed = "FFFB";
            preset.complexity = 3;
            preset.segmentLength = 0.7f;
            preset.baseThickness = 0.6f;
            preset.branchThinningRate = 0.9f;
            preset.childBranchThickness = 0.75f;
            preset.leafMode = TreeGenerator.LeafGenerationMode.Clusters;
            preset.leafDensity = 2.2f;
            preset.leafStartHeight = 0.25f;
            preset.leafSizeVariation = 0.25f;
            preset.leafTransparency = 0.95f;
            preset.clusterRadius = 1.1f;
            preset.clusterSizeMin = 0.85f;
            preset.clusterSizeMax = 1.35f;
            preset.clusterShapeX = 1.3f;
            preset.clusterShapeY = 0.95f;
            preset.clusterShapeZ = 1.1f;
            preset.clusterNoiseStrength = 0.18f;
            preset.clusterNoiseScale = 2f;
            preset.enableOuterShell = true;
            preset.outerShellThickness = 1.2f;
            preset.outerShellTransparency = 0.35f;
            preset.clusterSegments = 14;
            preset.clusterTextureTiling = 1.2f;
            preset.clusterOffset = 0.2f;
            preset.segmentGrowthChance = 40f;
            preset.branchPatternVariation = 55f;
            preset.minBranchAngle = 25f;
            preset.maxBranchAngle = 50f;
            preset.minVerticalAngle = -10f;
            preset.maxVerticalAngle = 40f;
            preset.branchBlendDistance = 0.25f;
            preset.branchBendChance = 0.35f;
            preset.branchBendStrength = 3.5f;
            preset.minBranchRadiusForLeaves = 0.02f;
            preset.maxLeafCount = 3500;
        });

        CreateOrUpdatePreset(PresetFolder + "/Pine.asset", preset =>
        {
            preset.lSystemSeed = "FFFFB";
            preset.complexity = 4;
            preset.segmentLength = 0.9f;
            preset.baseThickness = 0.4f;
            preset.branchThinningRate = 0.92f;
            preset.childBranchThickness = 0.65f;
            preset.leafMode = TreeGenerator.LeafGenerationMode.Planes;
            preset.leafWidth = 0.05f;
            preset.leafLength = 0.35f;
            preset.leafDensity = 9f;
            preset.leafStartHeight = 0.25f;
            preset.leafSizeVariation = 0.15f;
            preset.doubleSidedLeaves = true;
            preset.leafDistanceFromBranch = 0.05f;
            preset.leafClumpiness = 0.7f;
            preset.leafClumpSpread = 0.2f;
            preset.leafTipBias = 0.8f;
            preset.leafUpAlignment = 0.2f;
            preset.leafSizeByHeight = 0.4f;
            preset.leafRadialJitter = 0.05f;
            preset.segmentGrowthChance = 35f;
            preset.branchPatternVariation = 35f;
            preset.minBranchAngle = 15f;
            preset.maxBranchAngle = 35f;
            preset.minVerticalAngle = -55f;
            preset.maxVerticalAngle = 5f;
            preset.branchBlendDistance = 0.15f;
            preset.branchBendChance = 0.2f;
            preset.branchBendStrength = 2f;
            preset.minBranchRadiusForLeaves = 0.015f;
            preset.maxLeafCount = 4500;
        });

        CreateOrUpdatePreset(PresetFolder + "/Palm.asset", preset =>
        {
            preset.lSystemSeed = "FFFFFB";
            preset.complexity = 2;
            preset.segmentLength = 1.1f;
            preset.baseThickness = 0.5f;
            preset.branchThinningRate = 0.95f;
            preset.childBranchThickness = 0.85f;
            preset.leafMode = TreeGenerator.LeafGenerationMode.Domes;
            preset.leafDensity = 1.2f;
            preset.leafStartHeight = 0.6f;
            preset.leafSizeVariation = 0.15f;
            preset.domeRadius = 2.6f;
            preset.domeShapeX = 1.1f;
            preset.domeShapeY = 0.55f;
            preset.domeShapeZ = 1.1f;
            preset.domeOffset = 0.8f;
            preset.domeSegments = 14;
            preset.domeNoiseStrength = 0.08f;
            preset.domeNoiseScale = 2f;
            preset.domeTextureTiling = 1.2f;
            preset.randomizeDomeRotation = true;
            preset.segmentGrowthChance = 45f;
            preset.branchPatternVariation = 25f;
            preset.minBranchAngle = 10f;
            preset.maxBranchAngle = 25f;
            preset.minVerticalAngle = -5f;
            preset.maxVerticalAngle = 45f;
            preset.branchBlendDistance = 0.1f;
            preset.branchBendChance = 0.1f;
            preset.branchBendStrength = 1.5f;
            preset.minBranchRadiusForLeaves = 0.02f;
            preset.maxLeafCount = 2500;
        });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateOrUpdatePreset(string assetPath, System.Action<TreePreset> applyValues)
    {
        TreePreset preset = AssetDatabase.LoadAssetAtPath<TreePreset>(assetPath);
        if (preset == null)
        {
            preset = ScriptableObject.CreateInstance<TreePreset>();
            AssetDatabase.CreateAsset(preset, assetPath);
        }

        applyValues(preset);
        EditorUtility.SetDirty(preset);
    }
}
#endif
