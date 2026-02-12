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

        lSystemSeed = newPreset.lSystemSeed;
        complexity = newPreset.complexity;
        segmentLength = newPreset.segmentLength;

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
        leafClumpiness = newPreset.leafClumpiness;
        leafClumpSpread = newPreset.leafClumpSpread;
        leafTipBias = newPreset.leafTipBias;
        leafUpAlignment = newPreset.leafUpAlignment;
        leafSizeByHeight = newPreset.leafSizeByHeight;
        leafRadialJitter = newPreset.leafRadialJitter;

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

        segmentGrowthChance = newPreset.segmentGrowthChance;
        branchPatternVariation = newPreset.branchPatternVariation;
        minBranchAngle = newPreset.minBranchAngle;
        maxBranchAngle = newPreset.maxBranchAngle;
        minVerticalAngle = newPreset.minVerticalAngle;
        maxVerticalAngle = newPreset.maxVerticalAngle;

        branchBendChance = newPreset.branchBendChance;
        branchBendStrength = newPreset.branchBendStrength;
    }
}
