using UnityEngine;

public partial class TreeGenerator
{
    public void ApplyPreset(TreePreset newPreset)
    {
        preset = newPreset;
        if (newPreset == null)
        {
            return;
        }

        if (newPreset.barkMaterial != null)
        {
            barkMaterial = newPreset.barkMaterial;
        }

        if (newPreset.leafMaterial != null)
        {
            leafMaterial = newPreset.leafMaterial;
        }

        segmentLength = newPreset.segmentLength;

        randomSeed = newPreset.randomSeed;
        trunkHeight = newPreset.trunkHeight;
        trunkHeightVariation = newPreset.trunkHeightVariation;
        trunkLeanStrength = newPreset.trunkLeanStrength;
        trunkNoiseScale = newPreset.trunkNoiseScale;
        trunkNoiseStrength = newPreset.trunkNoiseStrength;

        branchLevels = newPreset.noBranches ? 0 : newPreset.branchLevels;
        branchesPerLevel = newPreset.branchesPerLevel;
        branchLevelDensityFalloff = newPreset.branchLevelDensityFalloff;
        branchLengthFactor = newPreset.branchLengthFactor;
        branchLengthFalloff = newPreset.branchLengthFalloff;
        branchAngleMin = newPreset.branchAngleMin;
        branchAngleMax = newPreset.branchAngleMax;
        branchUpwardBias = newPreset.branchUpwardBias;
        branchDroop = newPreset.branchDroop;
        branchNoiseScale = newPreset.branchNoiseScale;
        branchNoiseStrength = newPreset.branchNoiseStrength;
        branchTwistJitter = newPreset.branchTwistJitter;
        maxGeneratedBranches = newPreset.maxGeneratedBranches;
        minBranchUpward = newPreset.minBranchUpward;
        clampBranchesAboveBase = newPreset.clampBranchesAboveBase;
        branchGroundClearance = newPreset.branchGroundClearance;

        canopyTargetEnabled = newPreset.canopyTargetEnabled;
        canopyCenterOffset = newPreset.canopyCenterOffset;
        canopyRadii = newPreset.canopyRadii;
        canopyAttraction = newPreset.canopyAttraction;
        canopySurfaceTarget = newPreset.canopySurfaceTarget;
        canopyHeightStart = newPreset.canopyHeightStart;
        canopyHeightEnd = newPreset.canopyHeightEnd;

        baseThickness = newPreset.baseThickness;
        branchThinningRate = newPreset.branchThinningRate;
        childBranchThickness = newPreset.childBranchThickness;

        leafMode = newPreset.leafMode;
        leafWidth = newPreset.leafWidth;
        leafLength = newPreset.leafLength;
        leafDensity = newPreset.leafDensity;
        leafStartHeight = newPreset.leafStartHeight;
        leafSizeVariation = newPreset.leafSizeVariation;
        doubleSidedLeaves = newPreset.doubleSidedLeaves;
        leafDistanceFromBranch = newPreset.leafDistanceFromBranch;
        planeLeafTextureTiling = newPreset.planeLeafTextureTiling;
        enablePlaneLeafSizeByHeight = newPreset.enablePlaneLeafSizeByHeight;
        planeLeafSizeBottom = newPreset.planeLeafSizeBottom;
        planeLeafSizeTop = newPreset.planeLeafSizeTop;

        leafTransparency = newPreset.leafTransparency;
        leafUVRandomness = newPreset.leafUVRandomness;
        leafUVNoiseStrength = newPreset.leafUVNoiseStrength;
        leafUVNoiseScale = newPreset.leafUVNoiseScale;

        clusterRadius = newPreset.clusterRadius;
        clusterSizeMin = newPreset.clusterSizeMin;
        clusterSizeMax = newPreset.clusterSizeMax;
        clusterShapeX = newPreset.clusterShapeX;
        clusterShapeY = newPreset.clusterShapeY;
        clusterShapeZ = newPreset.clusterShapeZ;
        clusterNoiseStrength = newPreset.clusterNoiseStrength;
        clusterNoiseScale = newPreset.clusterNoiseScale;
        enableOuterShell = newPreset.enableOuterShell;
        outerShellThickness = newPreset.outerShellThickness;
        outerShellTransparency = newPreset.outerShellTransparency;
        clusterSegments = newPreset.clusterSegments;
        clusterTextureTiling = newPreset.clusterTextureTiling;
        randomizeClusterRotation = newPreset.randomizeClusterRotation;
        clusterOffset = newPreset.clusterOffset;

        domeRadius = newPreset.domeRadius;
        domeShapeX = newPreset.domeShapeX;
        domeShapeY = newPreset.domeShapeY;
        domeShapeZ = newPreset.domeShapeZ;
        domeOffset = newPreset.domeOffset;
        domeSegments = newPreset.domeSegments;
        domeNoiseScale = newPreset.domeNoiseScale;
        domeNoiseStrength = newPreset.domeNoiseStrength;
        randomizeDomeRotation = newPreset.randomizeDomeRotation;
        domeTextureTiling = newPreset.domeTextureTiling;

        maxLeafCount = newPreset.maxLeafCount;
        optimizeLeafDistribution = newPreset.optimizeLeafDistribution;
        minBranchRadiusForLeaves = newPreset.minBranchRadiusForLeaves;

        branchBlendDistance = newPreset.branchBlendDistance;
    }
}
