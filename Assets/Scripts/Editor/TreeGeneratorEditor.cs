using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TreeGenerator))]
public class TreeGeneratorEditor : Editor
{
    // Foldout states
    private bool showMaterials = true;
    private bool showTreeStructure = true;
    private bool showBranching = true;
    private bool showLeaves = true;
    private bool showAdvanced = false;
    
    // Serialized properties
    SerializedProperty leafMode;
    
    // Tree Materials
    SerializedProperty barkMaterial;
    SerializedProperty leafMaterial;
    SerializedProperty barkTilingHorizontal;
    SerializedProperty barkTilingVertical;
    SerializedProperty barkUVRandomness;
    SerializedProperty barkUVNoiseStrength;
    SerializedProperty barkUVNoiseScale;
    
    // Tree Generation
    SerializedProperty lSystemSeed;
    SerializedProperty complexity;
    SerializedProperty segmentLength;
    
    // Thickness
    SerializedProperty baseThickness;
    SerializedProperty branchThinningRate;
    SerializedProperty childBranchThickness;
    
    // Leaves - Common
    SerializedProperty leafDensity;
    SerializedProperty leafStartHeight;
    SerializedProperty leafSizeVariation;
    
    // Leaves - Plane mode
    SerializedProperty leafWidth;
    SerializedProperty leafLength;
    SerializedProperty doubleSidedLeaves;
    SerializedProperty leafDistanceFromBranch;
    
    // Leaf Appearance
    SerializedProperty leafTransparency;
    SerializedProperty leafUVRandomness;
    SerializedProperty leafUVNoiseStrength;
    SerializedProperty leafUVNoiseScale;
    
    // Leaves - Cluster mode
    SerializedProperty clusterRadius;
    SerializedProperty clusterSizeMin;
    SerializedProperty clusterSizeMax;
    SerializedProperty clusterShapeX;
    SerializedProperty clusterShapeY;
    SerializedProperty clusterShapeZ;
    SerializedProperty clusterNoiseStrength;
    SerializedProperty clusterNoiseScale;
    SerializedProperty enableOuterShell;
    SerializedProperty outerShellThickness;
    SerializedProperty outerShellTransparency;
    SerializedProperty clusterSegments;
    SerializedProperty clusterTextureTiling;
    SerializedProperty randomizeClusterRotation;
    SerializedProperty clusterOffset;
    SerializedProperty minBranchesPerCluster;
    
    // Leaf Performance
    SerializedProperty maxLeafCount;
    SerializedProperty optimizeLeafDistribution;
    SerializedProperty minBranchRadiusForLeaves;
    
    // Branch Connection
    SerializedProperty branchBlendDistance;
    
    // Advanced Growth Parameters
    SerializedProperty segmentGrowthChance;
    SerializedProperty branchPatternVariation;
    SerializedProperty minBranchAngle;
    SerializedProperty maxBranchAngle;
    SerializedProperty minVerticalAngle;
    SerializedProperty maxVerticalAngle;

    private void OnEnable()
    {
        // Find all serialized properties
        leafMode = serializedObject.FindProperty("leafMode");
        
        // Tree Materials
        barkMaterial = serializedObject.FindProperty("barkMaterial");
        leafMaterial = serializedObject.FindProperty("leafMaterial");
        barkTilingHorizontal = serializedObject.FindProperty("barkTilingHorizontal");
        barkTilingVertical = serializedObject.FindProperty("barkTilingVertical");
        barkUVRandomness = serializedObject.FindProperty("barkUVRandomness");
        barkUVNoiseStrength = serializedObject.FindProperty("barkUVNoiseStrength");
        barkUVNoiseScale = serializedObject.FindProperty("barkUVNoiseScale");
        
        // Tree Generation
        lSystemSeed = serializedObject.FindProperty("lSystemSeed");
        complexity = serializedObject.FindProperty("complexity");
        segmentLength = serializedObject.FindProperty("segmentLength");
        
        // Thickness
        baseThickness = serializedObject.FindProperty("baseThickness");
        branchThinningRate = serializedObject.FindProperty("branchThinningRate");
        childBranchThickness = serializedObject.FindProperty("childBranchThickness");
        
        // Leaves - Common
        leafDensity = serializedObject.FindProperty("leafDensity");
        leafStartHeight = serializedObject.FindProperty("leafStartHeight");
        leafSizeVariation = serializedObject.FindProperty("leafSizeVariation");
        
        // Leaves - Planes
        leafWidth = serializedObject.FindProperty("leafWidth");
        leafLength = serializedObject.FindProperty("leafLength");
        doubleSidedLeaves = serializedObject.FindProperty("doubleSidedLeaves");
        leafDistanceFromBranch = serializedObject.FindProperty("leafDistanceFromBranch");
        
        // Leaf Appearance
        leafTransparency = serializedObject.FindProperty("leafTransparency");
        leafUVRandomness = serializedObject.FindProperty("leafUVRandomness");
        leafUVNoiseStrength = serializedObject.FindProperty("leafUVNoiseStrength");
        leafUVNoiseScale = serializedObject.FindProperty("leafUVNoiseScale");
        
        // Leaves - Cluster
        clusterRadius = serializedObject.FindProperty("clusterRadius");
        clusterSizeMin = serializedObject.FindProperty("clusterSizeMin");
        clusterSizeMax = serializedObject.FindProperty("clusterSizeMax");
        clusterShapeX = serializedObject.FindProperty("clusterShapeX");
        clusterShapeY = serializedObject.FindProperty("clusterShapeY");
        clusterShapeZ = serializedObject.FindProperty("clusterShapeZ");
        clusterNoiseStrength = serializedObject.FindProperty("clusterNoiseStrength");
        clusterNoiseScale = serializedObject.FindProperty("clusterNoiseScale");
        enableOuterShell = serializedObject.FindProperty("enableOuterShell");
        outerShellThickness = serializedObject.FindProperty("outerShellThickness");
        outerShellTransparency = serializedObject.FindProperty("outerShellTransparency");
        clusterSegments = serializedObject.FindProperty("clusterSegments");
        clusterTextureTiling = serializedObject.FindProperty("clusterTextureTiling");
        randomizeClusterRotation = serializedObject.FindProperty("randomizeClusterRotation");
        clusterOffset = serializedObject.FindProperty("clusterOffset");
        minBranchesPerCluster = serializedObject.FindProperty("minBranchesPerCluster");
        
        // Leaf Performance
        maxLeafCount = serializedObject.FindProperty("maxLeafCount");
        optimizeLeafDistribution = serializedObject.FindProperty("optimizeLeafDistribution");
        minBranchRadiusForLeaves = serializedObject.FindProperty("minBranchRadiusForLeaves");
        
        // Branch Connection
        branchBlendDistance = serializedObject.FindProperty("branchBlendDistance");
        
        // Advanced Growth
        segmentGrowthChance = serializedObject.FindProperty("segmentGrowthChance");
        branchPatternVariation = serializedObject.FindProperty("branchPatternVariation");
        minBranchAngle = serializedObject.FindProperty("minBranchAngle");
        maxBranchAngle = serializedObject.FindProperty("maxBranchAngle");
        minVerticalAngle = serializedObject.FindProperty("minVerticalAngle");
        maxVerticalAngle = serializedObject.FindProperty("maxVerticalAngle");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        TreeGenerator treeGenerator = (TreeGenerator)target;

        // Create a button at the top that regenerates the tree
        if (GUILayout.Button("Regenerate Tree", GUILayout.Height(30)))
        {
            treeGenerator.RegenerateTree();
        }

        EditorGUILayout.Space(10);

        // === MATERIALS ===
        showMaterials = EditorGUILayout.Foldout(showMaterials, "Materials", true, EditorStyles.foldoutHeader);
        if (showMaterials)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(barkMaterial);
            EditorGUILayout.PropertyField(leafMaterial);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Bark Texture Settings", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(barkTilingHorizontal);
            EditorGUILayout.PropertyField(barkTilingVertical);
            EditorGUILayout.PropertyField(barkUVRandomness);
            EditorGUILayout.PropertyField(barkUVNoiseStrength);
            EditorGUILayout.PropertyField(barkUVNoiseScale);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(5);

        // === TREE STRUCTURE ===
        showTreeStructure = EditorGUILayout.Foldout(showTreeStructure, "Tree Structure", true, EditorStyles.foldoutHeader);
        if (showTreeStructure)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(lSystemSeed);
            EditorGUILayout.PropertyField(complexity);
            EditorGUILayout.PropertyField(segmentLength);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Branch Thickness", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(baseThickness);
            EditorGUILayout.PropertyField(branchThinningRate);
            EditorGUILayout.PropertyField(childBranchThickness);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(5);

        // === BRANCHING ===
        showBranching = EditorGUILayout.Foldout(showBranching, "Branching", true, EditorStyles.foldoutHeader);
        if (showBranching)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(segmentGrowthChance);
            EditorGUILayout.PropertyField(branchPatternVariation);
            EditorGUILayout.PropertyField(minBranchAngle);
            EditorGUILayout.PropertyField(maxBranchAngle);
            EditorGUILayout.PropertyField(minVerticalAngle);
            EditorGUILayout.PropertyField(maxVerticalAngle);
            EditorGUILayout.PropertyField(branchBlendDistance);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(5);

        // === LEAVES ===
        showLeaves = EditorGUILayout.Foldout(showLeaves, "Leaves", true, EditorStyles.foldoutHeader);
        if (showLeaves)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(leafMode);
            EditorGUILayout.PropertyField(leafDensity);
            EditorGUILayout.PropertyField(leafStartHeight);
            EditorGUILayout.PropertyField(leafSizeVariation);
            
            EditorGUILayout.Space(5);
            
            // Show mode-specific parameters
            TreeGenerator.LeafGenerationMode mode = (TreeGenerator.LeafGenerationMode)leafMode.enumValueIndex;
            
            if (mode == TreeGenerator.LeafGenerationMode.Planes)
            {
                EditorGUILayout.LabelField("Plane Leaf Settings", EditorStyles.miniLabel);
                EditorGUILayout.PropertyField(leafWidth);
                EditorGUILayout.PropertyField(leafLength);
                EditorGUILayout.PropertyField(leafDistanceFromBranch);
                EditorGUILayout.PropertyField(doubleSidedLeaves);
            }
            else if (mode == TreeGenerator.LeafGenerationMode.Clusters)
            {
                EditorGUILayout.LabelField("Cluster Settings", EditorStyles.miniLabel);
                EditorGUILayout.PropertyField(clusterRadius);
                EditorGUILayout.PropertyField(clusterSizeMin);
                EditorGUILayout.PropertyField(clusterSizeMax);
                EditorGUILayout.PropertyField(clusterOffset);
                EditorGUILayout.PropertyField(clusterSegments);
                EditorGUILayout.PropertyField(clusterTextureTiling);
                EditorGUILayout.PropertyField(randomizeClusterRotation);
                
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Cluster Shape", EditorStyles.miniLabel);
                EditorGUILayout.PropertyField(clusterShapeX);
                EditorGUILayout.PropertyField(clusterShapeY);
                EditorGUILayout.PropertyField(clusterShapeZ);
                EditorGUILayout.PropertyField(clusterNoiseStrength);
                EditorGUILayout.PropertyField(clusterNoiseScale);
                
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Outer Shell", EditorStyles.miniLabel);
                EditorGUILayout.PropertyField(enableOuterShell);
                if (enableOuterShell.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(outerShellThickness);
                    EditorGUILayout.PropertyField(outerShellTransparency);
                    EditorGUI.indentLevel--;
                }
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Leaf Appearance", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(leafTransparency);
            EditorGUILayout.PropertyField(leafUVRandomness);
            EditorGUILayout.PropertyField(leafUVNoiseStrength);
            EditorGUILayout.PropertyField(leafUVNoiseScale);
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(5);
        
        // === ADVANCED ===
        showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced Settings", true, EditorStyles.foldoutHeader);
        if (showAdvanced)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Performance", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(maxLeafCount);
            EditorGUILayout.PropertyField(optimizeLeafDistribution);
            EditorGUILayout.PropertyField(minBranchRadiusForLeaves);
            EditorGUILayout.PropertyField(minBranchesPerCluster);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
