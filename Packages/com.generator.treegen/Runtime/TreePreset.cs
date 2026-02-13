using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "TreeGen/Tree Preset", fileName = "TreePreset")]
public class TreePreset : ScriptableObject
{
    [Header("Materials")]
    public Material barkMaterial;
    public Material leafMaterial;

    [HideInInspector]
    public TreeGenerator.TreeStructureMode structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
    [HideInInspector]
    public string lSystemSeed = "FB";
    [HideInInspector]
    [Range(1, 10)]
    public int complexity = 3;
    [Header("Structure")]
    [Range(0.1f, 5f)]
    public float segmentLength = 1f;
    public int randomSeed = 0;
    [Range(1f, 20f)]
    public float trunkHeight = 6f;
    [Range(0f, 0.5f)]
    public float trunkHeightVariation = 0.15f;
    [Range(0f, 1f)]
    public float trunkLeanStrength = 0.2f;
    [Range(0.05f, 3f)]
    public float trunkNoiseScale = 0.35f;
    [Range(0f, 0.8f)]
    public float trunkNoiseStrength = 0.2f;

    [Header("Branches")]
    public bool noBranches = false;
    [Range(0, 5)]
    public int branchLevels = 3;
    [Range(1, 12)]
    public int branchesPerLevel = 3;
    [HideInInspector]
    [Range(1, 20)]
    public int minBranchesPerLevel = 4;
    [HideInInspector]
    [Range(1, 30)]
    public int maxBranchesPerLevel = 7;
    [Range(0.2f, 1f)]
    public float branchLevelDensityFalloff = 0.6f;
    [Range(0.2f, 1.2f)]
    public float branchLengthFactor = 0.7f;
    [Range(0.3f, 0.9f)]
    public float branchLengthFalloff = 0.65f;
    [HideInInspector]
    [Range(0f, 1f)]
    public float branchSpawnStart = 0.2f;
    [HideInInspector]
    [Range(0f, 1f)]
    public float branchSpawnEnd = 0.9f;
    [HideInInspector]
    [Range(0.5f, 3f)]
    public float branchDistributionPower = 1.4f;
    [Range(0f, 90f)]
    public float branchAngleMin = 20f;
    [Range(0f, 90f)]
    public float branchAngleMax = 55f;
    [Range(0f, 1f)]
    public float branchUpwardBias = 0.2f;
    [Range(0f, 1f)]
    public float branchDroop = 0.25f;
    [Range(0.05f, 3f)]
    public float branchNoiseScale = 0.6f;
    [Range(0f, 0.8f)]
    public float branchNoiseStrength = 0.25f;
    [Range(0f, 60f)]
    public float branchTwistJitter = 18f;
    [Range(0, 500)]
    public int maxGeneratedBranches = 200;
    [Range(-0.2f, 0.5f)]
    public float minBranchUpward = 0.02f;
    public bool clampBranchesAboveBase = false;
    [Range(0f, 2f)]
    public float branchGroundClearance = 0.05f;

    [Header("Canopy Targeting")]
    public bool canopyTargetEnabled = false;
    public Vector3 canopyCenterOffset = new Vector3(0f, 5f, 0f);
    public Vector3 canopyRadii = new Vector3(3f, 2.5f, 3f);
    [Range(0f, 1f)]
    public float canopyAttraction = 0.35f;
    public bool canopySurfaceTarget = true;
    [Range(0f, 1f)]
    public float canopyHeightStart = 0.35f;
    [Range(0f, 1f)]
    public float canopyHeightEnd = 1f;
    [HideInInspector]
    public List<TreeGenerator.CanopyVolumeSettings> canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

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
    [Range(0.1f, 10f)]
    public float planeLeafTextureTiling = 1f;
    public bool enablePlaneLeafSizeByHeight = false;
    [Range(0.1f, 10f)]
    public float planeLeafSizeBottom = 1f;
    [Range(0.1f, 10f)]
    public float planeLeafSizeTop = 1f;
    [HideInInspector]
    [Range(0f, 1f)]
    public float leafClumpiness = 0.5f;
    [HideInInspector]
    [Range(0.01f, 1f)]
    public float leafClumpSpread = 0.25f;
    [HideInInspector]
    [Range(0f, 1f)]
    public float leafTipBias = 0.6f;
    [HideInInspector]
    [Range(0f, 1f)]
    public float leafUpAlignment = 0.7f;
    [HideInInspector]
    [Range(0f, 1f)]
    public float leafSizeByHeight = 0.3f;
    [HideInInspector]
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
    public float clusterTextureTiling = 4.5f;
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
    private const string PresetDefaultsFolder = "Assets/TreePresets/Defaults";
    private const string PackageRoot = "Packages/com.generator.treegen";
    private const string MaterialFolder = PackageRoot + "/Runtime/Materials";
    private const string PresetMaterialFolder = PackageRoot + "/Runtime/Materials/Presets";

    private static Material LoadMaterial(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/" + name + ".mat");
    }

    private static Material LoadPresetMaterial(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(PresetMaterialFolder + "/" + name + ".mat");
    }

    public static void CreateDefaultPresets()
    {
        EnsurePresetDefaultsFolder();

        Material barkMat = LoadMaterial("WoodMaterial");
        Material leafClusterMat = LoadMaterial("LeafCluster");
        Material leafClusterYellowMat = LoadMaterial("LeafCluster_Yellow");
        Material leafPlaneMat = LoadMaterial("LeafPlane");
        Material pineLeafMat = LoadMaterial("PineLeaf");
        Material leafDomeMat = LoadMaterial("LeafDome");
        Material solidLeafMat = LoadMaterial("SolidLeaf");

        Material barkBrownMat = LoadPresetMaterial("Bark_Brown");
        Material barkWhiteMat = LoadPresetMaterial("Bark_White");
        Material barkAspenMat = LoadPresetMaterial("Bark_Aspen");
        Material barkRedwoodMat = LoadPresetMaterial("Bark_Redwood");
        Material barkGrayMat = LoadPresetMaterial("Bark_Gray");

        Material leafClusterLushMat = LoadPresetMaterial("LeafCluster_LushGreen");
        Material leafClusterLightMat = LoadPresetMaterial("LeafCluster_LightGreen");
        Material leafClusterRedMat = LoadPresetMaterial("LeafCluster_Red");
        Material leafClusterOliveMat = LoadPresetMaterial("LeafCluster_Olive");

        Material pineLeafDarkMat = LoadPresetMaterial("PineLeaf_Dark");
        Material solidLeafBushMat = LoadPresetMaterial("SolidLeaf_Bush");

        CreateOrUpdatePreset(PresetFolder + "/Oak.asset", preset =>
        {
            preset.barkMaterial = barkBrownMat;
            preset.leafMaterial = leafClusterLushMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.65f;
            preset.baseThickness = 0.75f;
            preset.branchThinningRate = 0.92f;
            preset.childBranchThickness = 0.7f;

            preset.trunkHeight = 7.5f;
            preset.trunkHeightVariation = 0.18f;
            preset.trunkLeanStrength = 0.15f;
            preset.trunkNoiseScale = 0.35f;
            preset.trunkNoiseStrength = 0.18f;
            preset.branchLevels = 2;
            preset.minBranchesPerLevel = 3;
            preset.maxBranchesPerLevel = 4;
            preset.branchLevelDensityFalloff = 0.8f;
            preset.branchLengthFactor = 0.9f;
            preset.branchLengthFalloff = 0.7f;
            preset.branchSpawnStart = 0.12f;
            preset.branchSpawnEnd = 0.85f;
            preset.branchDistributionPower = 1.25f;
            preset.branchAngleMin = 30f;
            preset.branchAngleMax = 70f;
            preset.branchUpwardBias = 0.05f;
            preset.branchDroop = 0.25f;
            preset.branchNoiseScale = 0.35f;
            preset.branchNoiseStrength = 0.1f;
            preset.branchTwistJitter = 10f;
            preset.maxGeneratedBranches = 90;
            preset.minBranchUpward = 0.03f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.05f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 4.8f, 0f);
            preset.canopyRadii = new Vector3(4.6f, 2.6f, 4.6f);
            preset.canopyAttraction = 0.5f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.2f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>
            {
                new TreeGenerator.CanopyVolumeSettings
                {
                    centerOffset = new Vector3(0f, 4.4f, 0f),
                    radii = new Vector3(4.8f, 2.4f, 4.8f),
                    attraction = 0.5f,
                    surfaceTarget = true,
                    heightStart = 0.2f,
                    heightEnd = 0.75f
                },
                new TreeGenerator.CanopyVolumeSettings
                {
                    centerOffset = new Vector3(0f, 6.2f, 0f),
                    radii = new Vector3(3.2f, 2.2f, 3.2f),
                    attraction = 0.35f,
                    surfaceTarget = true,
                    heightStart = 0.6f,
                    heightEnd = 1f
                }
            };

            preset.leafMode = TreeGenerator.LeafGenerationMode.Clusters;
            preset.leafDensity = 9f;
            preset.leafStartHeight = 0.22f;
            preset.leafSizeVariation = 0.25f;
            preset.leafTransparency = 0.95f;
            preset.clusterRadius = 1.7f;
            preset.clusterSizeMin = 0.85f;
            preset.clusterSizeMax = 1.5f;
            preset.clusterShapeX = 1.35f;
            preset.clusterShapeY = 0.9f;
            preset.clusterShapeZ = 1.25f;
            preset.clusterNoiseStrength = 0.22f;
            preset.clusterNoiseScale = 2.2f;
            preset.enableOuterShell = true;
            preset.outerShellThickness = 1.25f;
            preset.outerShellTransparency = 0.32f;
            preset.clusterSegments = 16;
            preset.clusterTextureTiling = 4.5f;
            preset.clusterOffset = 0.2f;
            preset.minBranchRadiusForLeaves = 0.015f;
            preset.maxLeafCount = 11000;
            preset.branchBlendDistance = 0.25f;
        });

        CreateOrUpdatePreset(PresetFolder + "/Pine.asset", preset =>
        {
            preset.barkMaterial = barkBrownMat;
            preset.leafMaterial = pineLeafDarkMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.9f;
            preset.baseThickness = 0.55f;
            preset.branchThinningRate = 0.94f;
            preset.childBranchThickness = 0.62f;

            preset.trunkHeight = 12f;
            preset.trunkHeightVariation = 0.12f;
            preset.trunkLeanStrength = 0.08f;
            preset.trunkNoiseScale = 0.25f;
            preset.trunkNoiseStrength = 0.12f;
            preset.branchLevels = 3;
            preset.minBranchesPerLevel = 3;
            preset.maxBranchesPerLevel = 5;
            preset.branchLevelDensityFalloff = 0.8f;
            preset.branchLengthFactor = 0.95f;
            preset.branchLengthFalloff = 0.6f;
            preset.branchSpawnStart = 0.15f;
            preset.branchSpawnEnd = 0.98f;
            preset.branchDistributionPower = 1.1f;
            preset.branchAngleMin = 10f;
            preset.branchAngleMax = 35f;
            preset.branchUpwardBias = 0.02f;
            preset.branchDroop = 0.3f;
            preset.branchNoiseScale = 0.3f;
            preset.branchNoiseStrength = 0.08f;
            preset.branchTwistJitter = 8f;
            preset.maxGeneratedBranches = 120;
            preset.minBranchUpward = 0.02f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.05f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 8.5f, 0f);
            preset.canopyRadii = new Vector3(2.8f, 5.2f, 2.8f);
            preset.canopyAttraction = 0.4f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.2f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>
            {
                new TreeGenerator.CanopyVolumeSettings
                {
                    centerOffset = new Vector3(0f, 6f, 0f),
                    radii = new Vector3(3.6f, 3.2f, 3.6f),
                    attraction = 0.3f,
                    surfaceTarget = true,
                    heightStart = 0.2f,
                    heightEnd = 0.7f
                },
                new TreeGenerator.CanopyVolumeSettings
                {
                    centerOffset = new Vector3(0f, 10.5f, 0f),
                    radii = new Vector3(1.9f, 3.6f, 1.9f),
                    attraction = 0.45f,
                    surfaceTarget = true,
                    heightStart = 0.5f,
                    heightEnd = 1f
                }
            };

            preset.leafMode = TreeGenerator.LeafGenerationMode.Planes;
            preset.leafWidth = 1.25f;
            preset.leafLength = 4.5f;
            preset.leafDensity = 14f;
            preset.leafStartHeight = 0.1f;
            preset.leafSizeVariation = 0.12f;
            preset.doubleSidedLeaves = true;
            preset.leafDistanceFromBranch = 0.035f;
            preset.enablePlaneLeafSizeByHeight = true;
            preset.planeLeafSizeBottom = 2f;
            preset.planeLeafSizeTop = 0.5f;
            preset.leafClumpiness = 0.7f;
            preset.leafClumpSpread = 0.2f;
            preset.leafTipBias = 0.8f;
            preset.leafUpAlignment = 0.1f;
            preset.leafSizeByHeight = 0.55f;
            preset.leafRadialJitter = 0.04f;
            preset.minBranchRadiusForLeaves = 0.015f;
            preset.maxLeafCount = 9500;
            preset.branchBlendDistance = 0.16f;
        });


        CreateOrUpdatePreset(PresetFolder + "/Ash.asset", preset =>
        {
            preset.barkMaterial = barkBrownMat;
            preset.leafMaterial = leafClusterOliveMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.7f;
            preset.baseThickness = 0.55f;
            preset.branchThinningRate = 0.92f;
            preset.childBranchThickness = 0.7f;

            preset.trunkHeight = 9.5f;
            preset.trunkHeightVariation = 0.18f;
            preset.trunkLeanStrength = 0.18f;
            preset.trunkNoiseScale = 0.35f;
            preset.trunkNoiseStrength = 0.18f;
            preset.branchLevels = 2;
            preset.minBranchesPerLevel = 3;
            preset.maxBranchesPerLevel = 4;
            preset.branchLevelDensityFalloff = 0.8f;
            preset.branchLengthFactor = 0.75f;
            preset.branchLengthFalloff = 0.72f;
            preset.branchSpawnStart = 0.35f;
            preset.branchSpawnEnd = 0.95f;
            preset.branchDistributionPower = 1.3f;
            preset.branchAngleMin = 20f;
            preset.branchAngleMax = 45f;
            preset.branchUpwardBias = 0.35f;
            preset.branchDroop = 0.15f;
            preset.branchNoiseScale = 0.35f;
            preset.branchNoiseStrength = 0.1f;
            preset.branchTwistJitter = 10f;
            preset.maxGeneratedBranches = 85;
            preset.minBranchUpward = 0.03f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.05f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 7.2f, 0f);
            preset.canopyRadii = new Vector3(2.8f, 3.4f, 2.8f);
            preset.canopyAttraction = 0.42f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.45f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

            preset.leafMode = TreeGenerator.LeafGenerationMode.Clusters;
            preset.leafDensity = 9f;
            preset.leafStartHeight = 0.45f;
            preset.leafSizeVariation = 0.2f;
            preset.clusterRadius = 1.6f;
            preset.clusterSizeMin = 0.85f;
            preset.clusterSizeMax = 1.25f;
            preset.clusterShapeX = 1.2f;
            preset.clusterShapeY = 0.95f;
            preset.clusterShapeZ = 1.05f;
            preset.clusterNoiseStrength = 0.16f;
            preset.clusterNoiseScale = 1.9f;
            preset.clusterSegments = 16;
            preset.clusterTextureTiling = 4.5f;
            preset.clusterOffset = 0.2f;
            preset.minBranchRadiusForLeaves = 0.015f;
            preset.maxLeafCount = 9000;
            preset.branchBlendDistance = 0.22f;
        });

        CreateOrUpdatePreset(PresetFolder + "/Aspen.asset", preset =>
        {
            preset.barkMaterial = barkAspenMat;
            preset.leafMaterial = leafClusterYellowMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.7f;
            preset.baseThickness = 0.42f;
            preset.branchThinningRate = 0.94f;
            preset.childBranchThickness = 0.68f;

            preset.trunkHeight = 10f;
            preset.trunkHeightVariation = 0.15f;
            preset.trunkLeanStrength = 0.15f;
            preset.trunkNoiseScale = 0.3f;
            preset.trunkNoiseStrength = 0.15f;
            preset.branchLevels = 2;
            preset.minBranchesPerLevel = 3;
            preset.maxBranchesPerLevel = 4;
            preset.branchLevelDensityFalloff = 0.8f;
            preset.branchLengthFactor = 0.7f;
            preset.branchLengthFalloff = 0.72f;
            preset.branchSpawnStart = 0.45f;
            preset.branchSpawnEnd = 0.95f;
            preset.branchDistributionPower = 1.15f;
            preset.branchAngleMin = 12f;
            preset.branchAngleMax = 35f;
            preset.branchUpwardBias = 0.5f;
            preset.branchDroop = 0.1f;
            preset.branchNoiseScale = 0.3f;
            preset.branchNoiseStrength = 0.08f;
            preset.branchTwistJitter = 8f;
            preset.maxGeneratedBranches = 75;
            preset.minBranchUpward = 0.03f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.05f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 7.5f, 0f);
            preset.canopyRadii = new Vector3(1.9f, 3.6f, 1.9f);
            preset.canopyAttraction = 0.45f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.55f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

            preset.leafMode = TreeGenerator.LeafGenerationMode.Clusters;
            preset.leafDensity = 8f;
            preset.leafStartHeight = 0.55f;
            preset.leafSizeVariation = 0.18f;
            preset.clusterRadius = 1.6f;
            preset.clusterSizeMin = 0.85f;
            preset.clusterSizeMax = 1.2f;
            preset.clusterShapeX = 1.15f;
            preset.clusterShapeY = 0.95f;
            preset.clusterShapeZ = 1.05f;
            preset.clusterNoiseStrength = 0.14f;
            preset.clusterNoiseScale = 1.8f;
            preset.clusterSegments = 16;
            preset.clusterTextureTiling = 4.5f;
            preset.clusterOffset = 0.15f;
            preset.minBranchRadiusForLeaves = 0.015f;
            preset.maxLeafCount = 8200;
            preset.branchBlendDistance = 0.22f;
        });

        CreateOrUpdatePreset(PresetFolder + "/Maple.asset", preset =>
        {
            preset.barkMaterial = barkBrownMat;
            preset.leafMaterial = leafClusterRedMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.6f;
            preset.baseThickness = 0.6f;
            preset.branchThinningRate = 0.92f;
            preset.childBranchThickness = 0.72f;

            preset.trunkHeight = 7.5f;
            preset.trunkHeightVariation = 0.2f;
            preset.trunkLeanStrength = 0.2f;
            preset.trunkNoiseScale = 0.45f;
            preset.trunkNoiseStrength = 0.22f;
            preset.branchLevels = 2;
            preset.minBranchesPerLevel = 3;
            preset.maxBranchesPerLevel = 4;
            preset.branchLevelDensityFalloff = 0.8f;
            preset.branchLengthFactor = 0.75f;
            preset.branchLengthFalloff = 0.65f;
            preset.branchSpawnStart = 0.2f;
            preset.branchSpawnEnd = 0.9f;
            preset.branchDistributionPower = 1.5f;
            preset.branchAngleMin = 25f;
            preset.branchAngleMax = 55f;
            preset.branchUpwardBias = 0.2f;
            preset.branchDroop = 0.2f;
            preset.branchNoiseScale = 0.35f;
            preset.branchNoiseStrength = 0.1f;
            preset.branchTwistJitter = 10f;
            preset.maxGeneratedBranches = 95;
            preset.minBranchUpward = 0.03f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.05f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 5.8f, 0f);
            preset.canopyRadii = new Vector3(3.6f, 3.2f, 3.6f);
            preset.canopyAttraction = 0.45f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.3f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

            preset.leafMode = TreeGenerator.LeafGenerationMode.Clusters;
            preset.leafDensity = 9f;
            preset.leafStartHeight = 0.3f;
            preset.leafSizeVariation = 0.25f;
            preset.clusterRadius = 1.6f;
            preset.clusterSizeMin = 0.85f;
            preset.clusterSizeMax = 1.35f;
            preset.clusterShapeX = 1.25f;
            preset.clusterShapeY = 1.05f;
            preset.clusterShapeZ = 1.1f;
            preset.clusterNoiseStrength = 0.2f;
            preset.clusterNoiseScale = 2.1f;
            preset.clusterSegments = 16;
            preset.clusterTextureTiling = 4.5f;
            preset.clusterOffset = 0.25f;
            preset.minBranchRadiusForLeaves = 0.015f;
            preset.maxLeafCount = 10500;
            preset.branchBlendDistance = 0.24f;
        });

        CreateOrUpdatePreset(PresetFolder + "/Birch.asset", preset =>
        {
            preset.barkMaterial = barkAspenMat;
            preset.leafMaterial = leafClusterLightMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.7f;
            preset.baseThickness = 0.4f;
            preset.branchThinningRate = 0.94f;
            preset.childBranchThickness = 0.65f;

            preset.trunkHeight = 9.5f;
            preset.trunkHeightVariation = 0.15f;
            preset.trunkLeanStrength = 0.2f;
            preset.trunkNoiseScale = 0.35f;
            preset.trunkNoiseStrength = 0.18f;
            preset.branchLevels = 2;
            preset.minBranchesPerLevel = 3;
            preset.maxBranchesPerLevel = 4;
            preset.branchLevelDensityFalloff = 0.8f;
            preset.branchLengthFactor = 0.65f;
            preset.branchLengthFalloff = 0.7f;
            preset.branchSpawnStart = 0.45f;
            preset.branchSpawnEnd = 0.95f;
            preset.branchDistributionPower = 1.35f;
            preset.branchAngleMin = 15f;
            preset.branchAngleMax = 40f;
            preset.branchUpwardBias = 0.45f;
            preset.branchDroop = 0.12f;
            preset.branchNoiseScale = 0.3f;
            preset.branchNoiseStrength = 0.08f;
            preset.branchTwistJitter = 8f;
            preset.maxGeneratedBranches = 70;
            preset.minBranchUpward = 0.03f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.05f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 7.2f, 0f);
            preset.canopyRadii = new Vector3(2.1f, 3.2f, 2.1f);
            preset.canopyAttraction = 0.42f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.55f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

            preset.leafMode = TreeGenerator.LeafGenerationMode.Clusters;
            preset.leafDensity = 8f;
            preset.leafStartHeight = 0.55f;
            preset.leafSizeVariation = 0.18f;
            preset.clusterRadius = 1.5f;
            preset.clusterSizeMin = 0.85f;
            preset.clusterSizeMax = 1.2f;
            preset.clusterShapeX = 1.15f;
            preset.clusterShapeY = 0.95f;
            preset.clusterShapeZ = 1.05f;
            preset.clusterNoiseStrength = 0.14f;
            preset.clusterNoiseScale = 1.8f;
            preset.clusterSegments = 16;
            preset.clusterTextureTiling = 4.5f;
            preset.clusterOffset = 0.15f;
            preset.minBranchRadiusForLeaves = 0.015f;
            preset.maxLeafCount = 7800;
            preset.branchBlendDistance = 0.22f;
        });


        CreateOrUpdatePreset(PresetFolder + "/Bush.asset", preset =>
        {
            preset.barkMaterial = barkBrownMat;
            preset.leafMaterial = solidLeafBushMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.45f;
            preset.baseThickness = 0.3f;
            preset.branchThinningRate = 0.9f;
            preset.childBranchThickness = 0.75f;

            preset.trunkHeight = 2.5f;
            preset.trunkHeightVariation = 0.25f;
            preset.trunkLeanStrength = 0.3f;
            preset.trunkNoiseScale = 0.6f;
            preset.trunkNoiseStrength = 0.3f;
            preset.branchLevels = 2;
            preset.minBranchesPerLevel = 4;
            preset.maxBranchesPerLevel = 6;
            preset.branchLevelDensityFalloff = 0.85f;
            preset.branchLengthFactor = 0.9f;
            preset.branchLengthFalloff = 0.8f;
            preset.branchSpawnStart = 0.1f;
            preset.branchSpawnEnd = 0.95f;
            preset.branchDistributionPower = 1.2f;
            preset.branchAngleMin = 30f;
            preset.branchAngleMax = 60f;
            preset.branchUpwardBias = 0.2f;
            preset.branchDroop = 0.25f;
            preset.branchNoiseScale = 0.45f;
            preset.branchNoiseStrength = 0.12f;
            preset.branchTwistJitter = 12f;
            preset.maxGeneratedBranches = 90;
            preset.minBranchUpward = 0.02f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.03f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 1.5f, 0f);
            preset.canopyRadii = new Vector3(2.2f, 1.8f, 2.2f);
            preset.canopyAttraction = 0.45f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.2f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

            preset.leafMode = TreeGenerator.LeafGenerationMode.Clusters;
            preset.leafDensity = 9f;
            preset.leafStartHeight = 0.1f;
            preset.leafSizeVariation = 0.25f;
            preset.clusterRadius = 1.5f;
            preset.clusterSizeMin = 0.9f;
            preset.clusterSizeMax = 1.2f;
            preset.clusterShapeX = 1.2f;
            preset.clusterShapeY = 1f;
            preset.clusterShapeZ = 1.2f;
            preset.clusterNoiseStrength = 0.2f;
            preset.clusterNoiseScale = 2f;
            preset.clusterSegments = 14;
            preset.clusterTextureTiling = 4.5f;
            preset.clusterOffset = 0.15f;
            preset.minBranchRadiusForLeaves = 0.012f;
            preset.maxLeafCount = 8000;
            preset.branchBlendDistance = 0.18f;
        });

        CreateOrUpdatePreset(PresetFolder + "/Redwood.asset", preset =>
        {
            preset.barkMaterial = barkRedwoodMat;
            preset.leafMaterial = pineLeafDarkMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 1.05f;
            preset.baseThickness = 0.9f;
            preset.branchThinningRate = 0.95f;
            preset.childBranchThickness = 0.6f;

            preset.trunkHeight = 15f;
            preset.trunkHeightVariation = 0.1f;
            preset.trunkLeanStrength = 0.06f;
            preset.trunkNoiseScale = 0.2f;
            preset.trunkNoiseStrength = 0.1f;
            preset.branchLevels = 3;
            preset.minBranchesPerLevel = 3;
            preset.maxBranchesPerLevel = 5;
            preset.branchLevelDensityFalloff = 0.8f;
            preset.branchLengthFactor = 0.85f;
            preset.branchLengthFalloff = 0.55f;
            preset.branchSpawnStart = 0.25f;
            preset.branchSpawnEnd = 0.98f;
            preset.branchDistributionPower = 1.4f;
            preset.branchAngleMin = 12f;
            preset.branchAngleMax = 40f;
            preset.branchUpwardBias = 0.15f;
            preset.branchDroop = 0.3f;
            preset.branchNoiseScale = 0.25f;
            preset.branchNoiseStrength = 0.08f;
            preset.branchTwistJitter = 8f;
            preset.maxGeneratedBranches = 120;
            preset.minBranchUpward = 0.02f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.05f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 12f, 0f);
            preset.canopyRadii = new Vector3(2.2f, 4.2f, 2.2f);
            preset.canopyAttraction = 0.35f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.6f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

            preset.leafMode = TreeGenerator.LeafGenerationMode.Planes;
            preset.leafWidth = 1.25f;
            preset.leafLength = 4.5f;
            preset.leafDensity = 12.5f;
            preset.leafStartHeight = 0.45f;
            preset.leafSizeVariation = 0.1f;
            preset.doubleSidedLeaves = true;
            preset.leafDistanceFromBranch = 0.04f;
            preset.enablePlaneLeafSizeByHeight = true;
            preset.planeLeafSizeBottom = 2f;
            preset.planeLeafSizeTop = 0.5f;
            preset.leafClumpiness = 0.6f;
            preset.leafClumpSpread = 0.2f;
            preset.leafTipBias = 0.7f;
            preset.leafUpAlignment = 0.15f;
            preset.leafSizeByHeight = 0.6f;
            preset.leafRadialJitter = 0.04f;
            preset.minBranchRadiusForLeaves = 0.015f;
            preset.maxLeafCount = 9800;
            preset.branchBlendDistance = 0.2f;
        });

        CreateOrUpdatePreset(PresetFolder + "/Oak_Young.asset", preset =>
        {
            preset.barkMaterial = barkBrownMat;
            preset.leafMaterial = leafClusterLushMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.55f;
            preset.baseThickness = 0.45f;
            preset.branchThinningRate = 0.92f;
            preset.childBranchThickness = 0.74f;

            preset.trunkHeight = 4.5f;
            preset.trunkHeightVariation = 0.2f;
            preset.trunkLeanStrength = 0.2f;
            preset.trunkNoiseScale = 0.45f;
            preset.trunkNoiseStrength = 0.22f;
            preset.branchLevels = 2;
            preset.minBranchesPerLevel = 2;
            preset.maxBranchesPerLevel = 4;
            preset.branchLevelDensityFalloff = 0.8f;
            preset.branchLengthFactor = 0.7f;
            preset.branchLengthFalloff = 0.7f;
            preset.branchSpawnStart = 0.2f;
            preset.branchSpawnEnd = 0.85f;
            preset.branchDistributionPower = 1.5f;
            preset.branchAngleMin = 25f;
            preset.branchAngleMax = 55f;
            preset.branchUpwardBias = 0.2f;
            preset.branchDroop = 0.35f;
            preset.branchNoiseScale = 0.4f;
            preset.branchNoiseStrength = 0.12f;
            preset.branchTwistJitter = 12f;
            preset.maxGeneratedBranches = 60;
            preset.minBranchUpward = 0.03f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.03f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 3.2f, 0f);
            preset.canopyRadii = new Vector3(2.2f, 1.8f, 2.2f);
            preset.canopyAttraction = 0.45f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.3f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

            preset.leafMode = TreeGenerator.LeafGenerationMode.Clusters;
            preset.leafDensity = 7f;
            preset.leafStartHeight = 0.25f;
            preset.leafSizeVariation = 0.25f;
            preset.clusterRadius = 1.2f;
            preset.clusterSizeMin = 0.85f;
            preset.clusterSizeMax = 1.25f;
            preset.clusterShapeX = 1.25f;
            preset.clusterShapeY = 0.95f;
            preset.clusterShapeZ = 1.1f;
            preset.clusterNoiseStrength = 0.18f;
            preset.clusterNoiseScale = 2f;
            preset.clusterSegments = 14;
            preset.clusterTextureTiling = 4.5f;
            preset.clusterOffset = 0.2f;
            preset.minBranchRadiusForLeaves = 0.02f;
            preset.maxLeafCount = 6000;
            preset.branchBlendDistance = 0.22f;
        });

        CreateOrUpdatePreset(PresetFolder + "/Maple_Young.asset", preset =>
        {
            preset.barkMaterial = barkBrownMat;
            preset.leafMaterial = leafClusterRedMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.55f;
            preset.baseThickness = 0.45f;
            preset.branchThinningRate = 0.92f;
            preset.childBranchThickness = 0.74f;

            preset.trunkHeight = 4.8f;
            preset.trunkHeightVariation = 0.2f;
            preset.trunkLeanStrength = 0.22f;
            preset.trunkNoiseScale = 0.45f;
            preset.trunkNoiseStrength = 0.22f;
            preset.branchLevels = 2;
            preset.minBranchesPerLevel = 2;
            preset.maxBranchesPerLevel = 4;
            preset.branchLevelDensityFalloff = 0.8f;
            preset.branchLengthFactor = 0.7f;
            preset.branchLengthFalloff = 0.7f;
            preset.branchSpawnStart = 0.2f;
            preset.branchSpawnEnd = 0.85f;
            preset.branchDistributionPower = 1.5f;
            preset.branchAngleMin = 25f;
            preset.branchAngleMax = 55f;
            preset.branchUpwardBias = 0.25f;
            preset.branchDroop = 0.3f;
            preset.branchNoiseScale = 0.4f;
            preset.branchNoiseStrength = 0.12f;
            preset.branchTwistJitter = 12f;
            preset.maxGeneratedBranches = 65;
            preset.minBranchUpward = 0.03f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.03f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 3.5f, 0f);
            preset.canopyRadii = new Vector3(2.2f, 1.9f, 2.2f);
            preset.canopyAttraction = 0.45f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.3f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

            preset.leafMode = TreeGenerator.LeafGenerationMode.Clusters;
            preset.leafDensity = 7f;
            preset.leafStartHeight = 0.25f;
            preset.leafSizeVariation = 0.25f;
            preset.clusterRadius = 1.2f;
            preset.clusterSizeMin = 0.85f;
            preset.clusterSizeMax = 1.25f;
            preset.clusterShapeX = 1.25f;
            preset.clusterShapeY = 0.95f;
            preset.clusterShapeZ = 1.1f;
            preset.clusterNoiseStrength = 0.18f;
            preset.clusterNoiseScale = 2f;
            preset.clusterSegments = 14;
            preset.clusterTextureTiling = 4.5f;
            preset.clusterOffset = 0.2f;
            preset.minBranchRadiusForLeaves = 0.02f;
            preset.maxLeafCount = 6200;
            preset.branchBlendDistance = 0.22f;
        });

        CreateOrUpdatePreset(PresetFolder + "/Birch_Young.asset", preset =>
        {
            preset.barkMaterial = barkWhiteMat;
            preset.leafMaterial = leafClusterLightMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.55f;
            preset.baseThickness = 0.38f;
            preset.branchThinningRate = 0.93f;
            preset.childBranchThickness = 0.7f;

            preset.trunkHeight = 5f;
            preset.trunkHeightVariation = 0.2f;
            preset.trunkLeanStrength = 0.28f;
            preset.trunkNoiseScale = 0.4f;
            preset.trunkNoiseStrength = 0.22f;
            preset.branchLevels = 2;
            preset.minBranchesPerLevel = 2;
            preset.maxBranchesPerLevel = 4;
            preset.branchLevelDensityFalloff = 0.8f;
            preset.branchLengthFactor = 0.65f;
            preset.branchLengthFalloff = 0.7f;
            preset.branchSpawnStart = 0.3f;
            preset.branchSpawnEnd = 0.9f;
            preset.branchDistributionPower = 1.6f;
            preset.branchAngleMin = 20f;
            preset.branchAngleMax = 50f;
            preset.branchUpwardBias = 0.35f;
            preset.branchDroop = 0.15f;
            preset.branchNoiseScale = 0.35f;
            preset.branchNoiseStrength = 0.1f;
            preset.branchTwistJitter = 10f;
            preset.maxGeneratedBranches = 55;
            preset.minBranchUpward = 0.03f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.03f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 3.6f, 0f);
            preset.canopyRadii = new Vector3(1.9f, 1.6f, 1.9f);
            preset.canopyAttraction = 0.42f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.4f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

            preset.leafMode = TreeGenerator.LeafGenerationMode.Clusters;
            preset.leafDensity = 7f;
            preset.leafStartHeight = 0.4f;
            preset.leafSizeVariation = 0.2f;
            preset.clusterRadius = 1.2f;
            preset.clusterSizeMin = 0.85f;
            preset.clusterSizeMax = 1.2f;
            preset.clusterShapeX = 1.2f;
            preset.clusterShapeY = 0.95f;
            preset.clusterShapeZ = 1.1f;
            preset.clusterNoiseStrength = 0.16f;
            preset.clusterNoiseScale = 2f;
            preset.clusterSegments = 14;
            preset.clusterTextureTiling = 4.5f;
            preset.clusterOffset = 0.18f;
            preset.minBranchRadiusForLeaves = 0.02f;
            preset.maxLeafCount = 5600;
            preset.branchBlendDistance = 0.2f;
        });

        CreateOrUpdatePreset(PresetFolder + "/Aspen_Young.asset", preset =>
        {
            preset.barkMaterial = barkWhiteMat;
            preset.leafMaterial = leafClusterYellowMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.55f;
            preset.baseThickness = 0.38f;
            preset.branchThinningRate = 0.93f;
            preset.childBranchThickness = 0.7f;

            preset.trunkHeight = 4.8f;
            preset.trunkHeightVariation = 0.2f;
            preset.trunkLeanStrength = 0.25f;
            preset.trunkNoiseScale = 0.4f;
            preset.trunkNoiseStrength = 0.2f;
            preset.branchLevels = 2;
            preset.minBranchesPerLevel = 2;
            preset.maxBranchesPerLevel = 4;
            preset.branchLevelDensityFalloff = 0.8f;
            preset.branchLengthFactor = 0.65f;
            preset.branchLengthFalloff = 0.7f;
            preset.branchSpawnStart = 0.3f;
            preset.branchSpawnEnd = 0.9f;
            preset.branchDistributionPower = 1.5f;
            preset.branchAngleMin = 22f;
            preset.branchAngleMax = 50f;
            preset.branchUpwardBias = 0.35f;
            preset.branchDroop = 0.18f;
            preset.branchNoiseScale = 0.35f;
            preset.branchNoiseStrength = 0.1f;
            preset.branchTwistJitter = 10f;
            preset.maxGeneratedBranches = 55;
            preset.minBranchUpward = 0.03f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.03f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 3.4f, 0f);
            preset.canopyRadii = new Vector3(1.9f, 1.6f, 1.9f);
            preset.canopyAttraction = 0.4f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.4f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

            preset.leafMode = TreeGenerator.LeafGenerationMode.Clusters;
            preset.leafDensity = 7f;
            preset.leafStartHeight = 0.4f;
            preset.leafSizeVariation = 0.2f;
            preset.clusterRadius = 1.2f;
            preset.clusterSizeMin = 0.85f;
            preset.clusterSizeMax = 1.2f;
            preset.clusterShapeX = 1.2f;
            preset.clusterShapeY = 0.95f;
            preset.clusterShapeZ = 1.1f;
            preset.clusterNoiseStrength = 0.16f;
            preset.clusterNoiseScale = 2f;
            preset.clusterSegments = 14;
            preset.clusterTextureTiling = 4.5f;
            preset.clusterOffset = 0.18f;
            preset.minBranchRadiusForLeaves = 0.02f;
            preset.maxLeafCount = 5600;
            preset.branchBlendDistance = 0.2f;
        });

        CreateOrUpdatePreset(PresetFolder + "/Ash_Young.asset", preset =>
        {
            preset.barkMaterial = barkBrownMat;
            preset.leafMaterial = leafClusterOliveMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.6f;
            preset.baseThickness = 0.42f;
            preset.branchThinningRate = 0.92f;
            preset.childBranchThickness = 0.72f;

            preset.trunkHeight = 5.2f;
            preset.trunkHeightVariation = 0.2f;
            preset.trunkLeanStrength = 0.22f;
            preset.trunkNoiseScale = 0.45f;
            preset.trunkNoiseStrength = 0.22f;
            preset.branchLevels = 2;
            preset.minBranchesPerLevel = 2;
            preset.maxBranchesPerLevel = 4;
            preset.branchLevelDensityFalloff = 0.8f;
            preset.branchLengthFactor = 0.7f;
            preset.branchLengthFalloff = 0.7f;
            preset.branchSpawnStart = 0.25f;
            preset.branchSpawnEnd = 0.9f;
            preset.branchDistributionPower = 1.4f;
            preset.branchAngleMin = 25f;
            preset.branchAngleMax = 55f;
            preset.branchUpwardBias = 0.25f;
            preset.branchDroop = 0.2f;
            preset.branchNoiseScale = 0.4f;
            preset.branchNoiseStrength = 0.12f;
            preset.branchTwistJitter = 12f;
            preset.maxGeneratedBranches = 60;
            preset.minBranchUpward = 0.03f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.03f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 3.8f, 0f);
            preset.canopyRadii = new Vector3(2.1f, 1.8f, 2.1f);
            preset.canopyAttraction = 0.42f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.35f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

            preset.leafMode = TreeGenerator.LeafGenerationMode.Clusters;
            preset.leafDensity = 7f;
            preset.leafStartHeight = 0.35f;
            preset.leafSizeVariation = 0.22f;
            preset.clusterRadius = 1.2f;
            preset.clusterSizeMin = 0.85f;
            preset.clusterSizeMax = 1.25f;
            preset.clusterShapeX = 1.25f;
            preset.clusterShapeY = 0.95f;
            preset.clusterShapeZ = 1.1f;
            preset.clusterNoiseStrength = 0.18f;
            preset.clusterNoiseScale = 2f;
            preset.clusterSegments = 14;
            preset.clusterTextureTiling = 4.5f;
            preset.clusterOffset = 0.2f;
            preset.minBranchRadiusForLeaves = 0.02f;
            preset.maxLeafCount = 5800;
            preset.branchBlendDistance = 0.22f;
        });


        CreateOrUpdatePreset(PresetFolder + "/Pine_Young.asset", preset =>
        {
            preset.barkMaterial = barkBrownMat;
            preset.leafMaterial = pineLeafDarkMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.8f;
            preset.baseThickness = 0.38f;
            preset.branchThinningRate = 0.93f;
            preset.childBranchThickness = 0.65f;

            preset.trunkHeight = 20f;
            preset.trunkHeightVariation = 0.18f;
            preset.trunkLeanStrength = 0.12f;
            preset.trunkNoiseScale = 0.3f;
            preset.trunkNoiseStrength = 0.15f;
            preset.branchLevels = 3;
            preset.minBranchesPerLevel = 3;
            preset.maxBranchesPerLevel = 4;
            preset.branchLevelDensityFalloff = 0.8f;
            preset.branchLengthFactor = 0.9f;
            preset.branchLengthFalloff = 0.6f;
            preset.branchSpawnStart = 0.15f;
            preset.branchSpawnEnd = 0.95f;
            preset.branchDistributionPower = 1.2f;
            preset.branchAngleMin = 18f;
            preset.branchAngleMax = 40f;
            preset.branchUpwardBias = 0.05f;
            preset.branchDroop = 0.4f;
            preset.branchNoiseScale = 0.35f;
            preset.branchNoiseStrength = 0.1f;
            preset.branchTwistJitter = 10f;
            preset.maxGeneratedBranches = 90;
            preset.minBranchUpward = 0.02f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.03f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 4.8f, 0f);
            preset.canopyRadii = new Vector3(1.9f, 2.8f, 1.9f);
            preset.canopyAttraction = 0.35f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.25f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

            preset.leafMode = TreeGenerator.LeafGenerationMode.Planes;
            preset.leafWidth = 1.25f;
            preset.leafLength = 4.5f;
            preset.leafDensity = 13f;
            preset.leafStartHeight = 0.25f;
            preset.leafSizeVariation = 0.14f;
            preset.doubleSidedLeaves = true;
            preset.leafDistanceFromBranch = 0.04f;
            preset.enablePlaneLeafSizeByHeight = true;
            preset.planeLeafSizeBottom = 7f;
            preset.planeLeafSizeTop = 0.8f;
            preset.leafClumpiness = 0.65f;
            preset.leafClumpSpread = 0.2f;
            preset.leafTipBias = 0.75f;
            preset.leafUpAlignment = 0.15f;
            preset.leafSizeByHeight = 0.55f;
            preset.leafRadialJitter = 0.05f;
            preset.minBranchRadiusForLeaves = 0.015f;
            preset.maxLeafCount = 8200;
            preset.branchBlendDistance = 0.18f;
        });

        CreateOrUpdatePreset(PresetFolder + "/Redwood_Young.asset", preset =>
        {
            preset.barkMaterial = barkRedwoodMat;
            preset.leafMaterial = pineLeafDarkMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.95f;
            preset.baseThickness = 0.6f;
            preset.branchThinningRate = 0.94f;
            preset.childBranchThickness = 0.62f;

            preset.trunkHeight = 9f;
            preset.trunkHeightVariation = 0.12f;
            preset.trunkLeanStrength = 0.08f;
            preset.trunkNoiseScale = 0.25f;
            preset.trunkNoiseStrength = 0.12f;
            preset.branchLevels = 3;
            preset.minBranchesPerLevel = 3;
            preset.maxBranchesPerLevel = 4;
            preset.branchLevelDensityFalloff = 0.8f;
            preset.branchLengthFactor = 0.85f;
            preset.branchLengthFalloff = 0.55f;
            preset.branchSpawnStart = 0.2f;
            preset.branchSpawnEnd = 0.9f;
            preset.branchDistributionPower = 1.4f;
            preset.branchAngleMin = 20f;
            preset.branchAngleMax = 45f;
            preset.branchUpwardBias = 0.2f;
            preset.branchDroop = 0.35f;
            preset.branchNoiseScale = 0.3f;
            preset.branchNoiseStrength = 0.1f;
            preset.branchTwistJitter = 10f;
            preset.maxGeneratedBranches = 95;
            preset.minBranchUpward = 0.02f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.03f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 7.2f, 0f);
            preset.canopyRadii = new Vector3(2.1f, 3.2f, 2.1f);
            preset.canopyAttraction = 0.35f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.6f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

            preset.leafMode = TreeGenerator.LeafGenerationMode.Planes;
            preset.leafWidth = 1.25f;
            preset.leafLength = 4.5f;
            preset.leafDensity = 12f;
            preset.leafStartHeight = 0.4f;
            preset.leafSizeVariation = 0.12f;
            preset.doubleSidedLeaves = true;
            preset.leafDistanceFromBranch = 0.04f;
            preset.enablePlaneLeafSizeByHeight = true;
            preset.planeLeafSizeBottom = 2f;
            preset.planeLeafSizeTop = 0.5f;
            preset.leafClumpiness = 0.6f;
            preset.leafClumpSpread = 0.2f;
            preset.leafTipBias = 0.7f;
            preset.leafUpAlignment = 0.15f;
            preset.leafSizeByHeight = 0.6f;
            preset.leafRadialJitter = 0.04f;
            preset.minBranchRadiusForLeaves = 0.015f;
            preset.maxLeafCount = 8200;
            preset.branchBlendDistance = 0.2f;
        });

        CreateOrUpdatePreset(PresetFolder + "/Leafless.asset", preset =>
        {
            preset.barkMaterial = barkGrayMat;
            preset.leafMaterial = leafClusterLightMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.7f;
            preset.baseThickness = 0.6f;
            preset.branchThinningRate = 0.9f;
            preset.childBranchThickness = 0.72f;

            preset.trunkHeight = 9f;
            preset.trunkHeightVariation = 0.2f;
            preset.trunkLeanStrength = 0.25f;
            preset.trunkNoiseScale = 0.5f;
            preset.trunkNoiseStrength = 0.28f;
            preset.branchLevels = 4;
            preset.branchesPerLevel = 5;
            preset.branchLevelDensityFalloff = 0.9f;
            preset.branchLengthFactor = 0.8f;
            preset.branchLengthFalloff = 0.7f;
            preset.branchAngleMin = 20f;
            preset.branchAngleMax = 65f;
            preset.branchUpwardBias = 0.08f;
            preset.branchDroop = 0.35f;
            preset.branchNoiseScale = 0.75f;
            preset.branchNoiseStrength = 0.25f;
            preset.branchTwistJitter = 24f;
            preset.maxGeneratedBranches = 220;
            preset.minBranchUpward = 0.01f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.05f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 6.5f, 0f);
            preset.canopyRadii = new Vector3(3f, 3f, 3f);
            preset.canopyAttraction = 0.4f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.2f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

            preset.leafMode = TreeGenerator.LeafGenerationMode.Clusters;
            preset.leafDensity = 0f;
            preset.leafStartHeight = 1f;
            preset.leafSizeVariation = 0f;
            preset.clusterRadius = 1.5f;
            preset.clusterSizeMin = 0.85f;
            preset.clusterSizeMax = 1.2f;
            preset.clusterShapeX = 1.2f;
            preset.clusterShapeY = 1f;
            preset.clusterShapeZ = 1.2f;
            preset.clusterNoiseStrength = 0.18f;
            preset.clusterNoiseScale = 2f;
            preset.clusterSegments = 14;
            preset.clusterTextureTiling = 4.5f;
            preset.clusterOffset = 0.2f;
            preset.minBranchRadiusForLeaves = 0.5f;
            preset.maxLeafCount = 0;
            preset.branchBlendDistance = 0.2f;
        });

        CreateOrUpdatePreset(PresetFolder + "/Leafless_Young.asset", preset =>
        {
            preset.barkMaterial = barkGrayMat;
            preset.leafMaterial = leafClusterLightMat;
            preset.structureMode = TreeGenerator.TreeStructureMode.GuidedGrowth;
            preset.segmentLength = 0.6f;
            preset.baseThickness = 0.45f;
            preset.branchThinningRate = 0.9f;
            preset.childBranchThickness = 0.74f;

            preset.trunkHeight = 5.5f;
            preset.trunkHeightVariation = 0.22f;
            preset.trunkLeanStrength = 0.28f;
            preset.trunkNoiseScale = 0.55f;
            preset.trunkNoiseStrength = 0.3f;
            preset.branchLevels = 3;
            preset.branchesPerLevel = 5;
            preset.branchLevelDensityFalloff = 0.9f;
            preset.branchLengthFactor = 0.75f;
            preset.branchLengthFalloff = 0.72f;
            preset.branchAngleMin = 20f;
            preset.branchAngleMax = 65f;
            preset.branchUpwardBias = 0.1f;
            preset.branchDroop = 0.4f;
            preset.branchNoiseScale = 0.8f;
            preset.branchNoiseStrength = 0.28f;
            preset.branchTwistJitter = 26f;
            preset.maxGeneratedBranches = 140;
            preset.minBranchUpward = 0.01f;
            preset.clampBranchesAboveBase = false;
            preset.branchGroundClearance = 0.03f;

            preset.canopyTargetEnabled = false;
            preset.canopyCenterOffset = new Vector3(0f, 3.8f, 0f);
            preset.canopyRadii = new Vector3(2.2f, 2.2f, 2.2f);
            preset.canopyAttraction = 0.4f;
            preset.canopySurfaceTarget = true;
            preset.canopyHeightStart = 0.25f;
            preset.canopyHeightEnd = 1f;
            preset.canopyVolumes = new List<TreeGenerator.CanopyVolumeSettings>();

            preset.leafMode = TreeGenerator.LeafGenerationMode.Clusters;
            preset.leafDensity = 0f;
            preset.leafStartHeight = 1f;
            preset.leafSizeVariation = 0f;
            preset.clusterRadius = 1.4f;
            preset.clusterSizeMin = 0.85f;
            preset.clusterSizeMax = 1.15f;
            preset.clusterShapeX = 1.2f;
            preset.clusterShapeY = 1f;
            preset.clusterShapeZ = 1.2f;
            preset.clusterNoiseStrength = 0.18f;
            preset.clusterNoiseScale = 2f;
            preset.clusterSegments = 14;
            preset.clusterTextureTiling = 4.5f;
            preset.clusterOffset = 0.2f;
            preset.minBranchRadiusForLeaves = 0.5f;
            preset.maxLeafCount = 0;
            preset.branchBlendDistance = 0.2f;
        });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void SaveCurrentPresetsAsDefaults()
    {
        EnsurePresetDefaultsFolder();

        string[] guids = AssetDatabase.FindAssets("t:TreePreset", new[] { PresetFolder });
        for (int i = 0; i < guids.Length; i++)
        {
            string sourcePath = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (sourcePath.StartsWith(PresetDefaultsFolder + "/", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string fileName = Path.GetFileName(sourcePath);
            if (string.IsNullOrEmpty(fileName))
            {
                continue;
            }

            string destPath = PresetDefaultsFolder + "/" + fileName;
            if (AssetDatabase.LoadAssetAtPath<TreePreset>(destPath) != null)
            {
                AssetDatabase.DeleteAsset(destPath);
            }

            AssetDatabase.CopyAsset(sourcePath, destPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsurePresetDefaultsFolder()
    {
        if (!AssetDatabase.IsValidFolder(PresetFolder))
        {
            AssetDatabase.CreateFolder("Assets", "TreePresets");
        }

        if (!AssetDatabase.IsValidFolder(PresetDefaultsFolder))
        {
            AssetDatabase.CreateFolder(PresetFolder, "Defaults");
        }
    }

    private static TreePreset LoadDefaultSource(string assetPath)
    {
        if (!AssetDatabase.IsValidFolder(PresetDefaultsFolder))
        {
            return null;
        }

        string fileName = Path.GetFileName(assetPath);
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        string sourcePath = PresetDefaultsFolder + "/" + fileName;
        if (string.Equals(sourcePath, assetPath, System.StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<TreePreset>(sourcePath);
    }

    private static void CreateOrUpdatePreset(string assetPath, System.Action<TreePreset> applyValues, bool overwriteExisting = false)
    {
        TreePreset preset = AssetDatabase.LoadAssetAtPath<TreePreset>(assetPath);
        if (preset != null && !overwriteExisting)
        {
            return;
        }

        if (preset == null)
        {
            preset = ScriptableObject.CreateInstance<TreePreset>();
            AssetDatabase.CreateAsset(preset, assetPath);
        }

        TreePreset sourcePreset = LoadDefaultSource(assetPath);
        if (sourcePreset != null)
        {
            EditorUtility.CopySerialized(sourcePreset, preset);
            preset.name = Path.GetFileNameWithoutExtension(assetPath);
        }
        else
        {
            applyValues(preset);
        }
        EditorUtility.SetDirty(preset);
    }
}
#endif
