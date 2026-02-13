using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TreeGenerator))]
public class TreeGeneratorEditor : Editor
{
    // Foldout states
    private bool showMaterials = true;
    private bool showBarkTextureSettings = false;
    private bool showTreeStructure = true;
    private bool showTrunkSettings = false;
    private bool showThicknessSettings = false;
    private bool showBranching = false;
    private bool showLeaves = true;
    private bool showPlaneLeafSettings = false;
    private bool showClusterSettings = false;
    private bool showClusterShape = false;
    private bool showOuterShell = false;
    private bool showDomeSettings = false;
    private bool showDomeShape = false;
    private bool showLeafAppearance = false;
    private bool showPresetSettings = false;
    private bool showAdvanced = false;

    private const string PresetNoneLabel = "None";
    private TreePreset[] cachedPresets = Array.Empty<TreePreset>();
    private string[] presetNames = new[] { PresetNoneLabel };
    private int selectedPresetIndex;
    
    // Serialized properties
    SerializedProperty leafMode;

    SerializedProperty preset;
    
    // Tree Materials
    SerializedProperty barkMaterial;
    SerializedProperty leafMaterial;
    SerializedProperty barkTilingHorizontal;
    SerializedProperty barkTilingVertical;
    SerializedProperty barkUVRandomness;
    SerializedProperty barkUVNoiseStrength;
    SerializedProperty barkUVNoiseScale;
    
    // Tree Generation
    SerializedProperty segmentLength;

    // Guided Growth
    SerializedProperty randomSeed;
    SerializedProperty trunkHeight;
    SerializedProperty trunkHeightVariation;
    SerializedProperty trunkLeanStrength;
    SerializedProperty trunkNoiseScale;
    SerializedProperty trunkNoiseStrength;
    SerializedProperty branchLevels;
    SerializedProperty branchesPerLevel;
    SerializedProperty branchLevelDensityFalloff;
    SerializedProperty branchLengthFactor;
    SerializedProperty branchLengthFalloff;
    SerializedProperty branchAngleMin;
    SerializedProperty branchAngleMax;
    SerializedProperty branchUpwardBias;
    SerializedProperty branchDroop;
    SerializedProperty branchNoiseScale;
    SerializedProperty branchNoiseStrength;
    SerializedProperty branchTwistJitter;
    SerializedProperty maxGeneratedBranches;
    SerializedProperty minBranchUpward;
    SerializedProperty clampBranchesAboveBase;
    SerializedProperty branchGroundClearance;

    // Canopy Targeting
    SerializedProperty canopyTargetEnabled;
    SerializedProperty canopyCenterOffset;
    SerializedProperty canopyRadii;
    SerializedProperty canopyAttraction;
    SerializedProperty canopySurfaceTarget;
    SerializedProperty canopyHeightStart;
    SerializedProperty canopyHeightEnd;
    
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
    SerializedProperty planeLeafTextureTiling;
    SerializedProperty enablePlaneLeafSizeByHeight;
    SerializedProperty planeLeafSizeBottom;
    SerializedProperty planeLeafSizeTop;
    
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

    // Leaves - Dome mode
    SerializedProperty domeRadius;
    SerializedProperty domeShapeX;
    SerializedProperty domeShapeY;
    SerializedProperty domeShapeZ;
    SerializedProperty domeOffset;
    SerializedProperty domeSegments;
    SerializedProperty domeNoiseScale;
    SerializedProperty domeNoiseStrength;
    SerializedProperty randomizeDomeRotation;
    SerializedProperty domeTextureTiling;
    
    // Leaf Performance
    SerializedProperty maxLeafCount;
    SerializedProperty optimizeLeafDistribution;
    SerializedProperty minBranchRadiusForLeaves;
    
    // Branch Connection
    SerializedProperty branchBlendDistance;
    

    private void OnEnable()
    {
        // Find all serialized properties
        leafMode = serializedObject.FindProperty("leafMode");
        preset = serializedObject.FindProperty("preset");
        
        // Tree Materials
        barkMaterial = serializedObject.FindProperty("barkMaterial");
        leafMaterial = serializedObject.FindProperty("leafMaterial");
        barkTilingHorizontal = serializedObject.FindProperty("barkTilingHorizontal");
        barkTilingVertical = serializedObject.FindProperty("barkTilingVertical");
        barkUVRandomness = serializedObject.FindProperty("barkUVRandomness");
        barkUVNoiseStrength = serializedObject.FindProperty("barkUVNoiseStrength");
        barkUVNoiseScale = serializedObject.FindProperty("barkUVNoiseScale");
        
        // Tree Generation
        segmentLength = serializedObject.FindProperty("segmentLength");

        randomSeed = serializedObject.FindProperty("randomSeed");
        trunkHeight = serializedObject.FindProperty("trunkHeight");
        trunkHeightVariation = serializedObject.FindProperty("trunkHeightVariation");
        trunkLeanStrength = serializedObject.FindProperty("trunkLeanStrength");
        trunkNoiseScale = serializedObject.FindProperty("trunkNoiseScale");
        trunkNoiseStrength = serializedObject.FindProperty("trunkNoiseStrength");
        branchLevels = serializedObject.FindProperty("branchLevels");
        branchesPerLevel = serializedObject.FindProperty("branchesPerLevel");
        branchLevelDensityFalloff = serializedObject.FindProperty("branchLevelDensityFalloff");
        branchLengthFactor = serializedObject.FindProperty("branchLengthFactor");
        branchLengthFalloff = serializedObject.FindProperty("branchLengthFalloff");
        branchAngleMin = serializedObject.FindProperty("branchAngleMin");
        branchAngleMax = serializedObject.FindProperty("branchAngleMax");
        branchUpwardBias = serializedObject.FindProperty("branchUpwardBias");
        branchDroop = serializedObject.FindProperty("branchDroop");
        branchNoiseScale = serializedObject.FindProperty("branchNoiseScale");
        branchNoiseStrength = serializedObject.FindProperty("branchNoiseStrength");
        branchTwistJitter = serializedObject.FindProperty("branchTwistJitter");
        maxGeneratedBranches = serializedObject.FindProperty("maxGeneratedBranches");
        minBranchUpward = serializedObject.FindProperty("minBranchUpward");
        clampBranchesAboveBase = serializedObject.FindProperty("clampBranchesAboveBase");
        branchGroundClearance = serializedObject.FindProperty("branchGroundClearance");

        canopyTargetEnabled = serializedObject.FindProperty("canopyTargetEnabled");
        canopyCenterOffset = serializedObject.FindProperty("canopyCenterOffset");
        canopyRadii = serializedObject.FindProperty("canopyRadii");
        canopyAttraction = serializedObject.FindProperty("canopyAttraction");
        canopySurfaceTarget = serializedObject.FindProperty("canopySurfaceTarget");
        canopyHeightStart = serializedObject.FindProperty("canopyHeightStart");
        canopyHeightEnd = serializedObject.FindProperty("canopyHeightEnd");
        
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
        planeLeafTextureTiling = serializedObject.FindProperty("planeLeafTextureTiling");
        enablePlaneLeafSizeByHeight = serializedObject.FindProperty("enablePlaneLeafSizeByHeight");
        planeLeafSizeBottom = serializedObject.FindProperty("planeLeafSizeBottom");
        planeLeafSizeTop = serializedObject.FindProperty("planeLeafSizeTop");
        
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

        // Leaves - Dome
        domeRadius = serializedObject.FindProperty("domeRadius");
        domeShapeX = serializedObject.FindProperty("domeShapeX");
        domeShapeY = serializedObject.FindProperty("domeShapeY");
        domeShapeZ = serializedObject.FindProperty("domeShapeZ");
        domeOffset = serializedObject.FindProperty("domeOffset");
        domeSegments = serializedObject.FindProperty("domeSegments");
        domeNoiseScale = serializedObject.FindProperty("domeNoiseScale");
        domeNoiseStrength = serializedObject.FindProperty("domeNoiseStrength");
        randomizeDomeRotation = serializedObject.FindProperty("randomizeDomeRotation");
        domeTextureTiling = serializedObject.FindProperty("domeTextureTiling");
        
        // Leaf Performance
        maxLeafCount = serializedObject.FindProperty("maxLeafCount");
        optimizeLeafDistribution = serializedObject.FindProperty("optimizeLeafDistribution");
        minBranchRadiusForLeaves = serializedObject.FindProperty("minBranchRadiusForLeaves");
        
        // Branch Connection
        branchBlendDistance = serializedObject.FindProperty("branchBlendDistance");
        

        RefreshPresets();
        selectedPresetIndex = GetPresetIndex(preset.objectReferenceValue as TreePreset);
    }

    private void RefreshPresets()
    {
        string[] guids = AssetDatabase.FindAssets("t:TreePreset");
        var presets = new List<TreePreset>(guids.Length);

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            TreePreset loaded = AssetDatabase.LoadAssetAtPath<TreePreset>(path);
            if (loaded != null)
            {
                presets.Add(loaded);
            }
        }

        presets.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

        cachedPresets = presets.ToArray();
        presetNames = new string[cachedPresets.Length + 1];
        presetNames[0] = PresetNoneLabel;
        for (int i = 0; i < cachedPresets.Length; i++)
        {
            presetNames[i + 1] = cachedPresets[i].name;
        }
    }

    private int GetPresetIndex(TreePreset current)
    {
        if (current == null)
        {
            return 0;
        }

        for (int i = 0; i < cachedPresets.Length; i++)
        {
            if (cachedPresets[i] == current)
            {
                return i + 1;
            }
        }

        return 0;
    }

    private TreePreset GetPresetByIndex(int index)
    {
        if (index <= 0 || index - 1 >= cachedPresets.Length)
        {
            return null;
        }

        return cachedPresets[index - 1];
    }

    private void DrawPresetSelector(TreeGenerator treeGenerator)
    {
        EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        int newIndex = EditorGUILayout.Popup("Preset", selectedPresetIndex, presetNames);
        if (EditorGUI.EndChangeCheck())
        {
            TreePreset selectedPreset = GetPresetByIndex(newIndex);
            if (preset.objectReferenceValue != selectedPreset)
            {
                Undo.RecordObject(treeGenerator, "Apply Tree Preset");
                treeGenerator.ApplyPreset(selectedPreset);
                preset.objectReferenceValue = selectedPreset;
                EditorUtility.SetDirty(treeGenerator);
                PrefabUtility.RecordPrefabInstancePropertyModifications(treeGenerator);
                serializedObject.Update();
            }

            selectedPresetIndex = newIndex;
        }

        showPresetSettings = EditorGUILayout.Foldout(showPresetSettings, "Preset Settings", true);
        if (showPresetSettings)
        {
            EditorGUI.indentLevel++;
            if (GUILayout.Button(new GUIContent(
                "Create Missing Default Presets",
                "Create any missing default preset assets without overwriting existing ones."
            )))
            {
                TreePresetDefaults.CreateDefaultPresets();
                RefreshPresets();
                selectedPresetIndex = GetPresetIndex(preset.objectReferenceValue as TreePreset);
            }
            if (GUILayout.Button(new GUIContent(
                "Set Defaults From Current Presets",
                "Save current presets as the default source used for future default creation."
            )))
            {
                TreePresetDefaults.SaveCurrentPresetsAsDefaults();
            }
            if (GUILayout.Button(new GUIContent(
                "Refresh Preset List",
                "Re-scan the project for TreePreset assets and update the dropdown."
            )))
            {
                RefreshPresets();
                selectedPresetIndex = GetPresetIndex(preset.objectReferenceValue as TreePreset);
            }
            EditorGUI.indentLevel--;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        TreeGenerator treeGenerator = (TreeGenerator)target;

        DrawPresetSelector(treeGenerator);
        EditorGUILayout.Space(8);

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
            showBarkTextureSettings = EditorGUILayout.Foldout(showBarkTextureSettings, "Bark Texture Settings", true);
            if (showBarkTextureSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(barkTilingHorizontal);
                EditorGUILayout.PropertyField(barkTilingVertical);
                EditorGUILayout.PropertyField(barkUVRandomness);
                EditorGUILayout.PropertyField(barkUVNoiseStrength);
                EditorGUILayout.PropertyField(barkUVNoiseScale);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(5);

        // === TREE STRUCTURE ===
        showTreeStructure = EditorGUILayout.Foldout(showTreeStructure, "Tree Structure", true, EditorStyles.foldoutHeader);
        if (showTreeStructure)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(segmentLength);
            EditorGUILayout.Space(4);
            showTrunkSettings = EditorGUILayout.Foldout(showTrunkSettings, "Trunk Settings", true);
            if (showTrunkSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(trunkHeight);
                EditorGUILayout.PropertyField(trunkHeightVariation);
                EditorGUILayout.PropertyField(trunkLeanStrength);
                EditorGUILayout.PropertyField(trunkNoiseScale);
                EditorGUILayout.PropertyField(trunkNoiseStrength);
                EditorGUILayout.PropertyField(randomSeed);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            showThicknessSettings = EditorGUILayout.Foldout(showThicknessSettings, "Thickness", true);
            if (showThicknessSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(baseThickness);
                EditorGUILayout.PropertyField(branchThinningRate);
                EditorGUILayout.PropertyField(childBranchThickness);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(5);

        // === BRANCHING ===
        showBranching = EditorGUILayout.Foldout(showBranching, "Branching", true, EditorStyles.foldoutHeader);
        if (showBranching)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Guided Growth", EditorStyles.miniLabel);
            bool noBranches = branchLevels.intValue <= 0;
            bool newNoBranches = EditorGUILayout.Toggle("No Branches", noBranches);
            if (newNoBranches != noBranches)
            {
                branchLevels.intValue = newNoBranches ? 0 : Mathf.Max(1, branchLevels.intValue);
            }
            EditorGUI.BeginDisabledGroup(branchLevels.intValue <= 0);
            EditorGUILayout.PropertyField(branchLevels);
            EditorGUILayout.PropertyField(branchesPerLevel);
            EditorGUILayout.PropertyField(branchLevelDensityFalloff);
            EditorGUILayout.PropertyField(branchLengthFactor);
            EditorGUILayout.PropertyField(branchLengthFalloff);
            EditorGUILayout.PropertyField(branchAngleMin);
            EditorGUILayout.PropertyField(branchAngleMax);
            EditorGUILayout.PropertyField(branchUpwardBias);
            EditorGUILayout.PropertyField(branchDroop);
            EditorGUILayout.PropertyField(branchNoiseScale);
            EditorGUILayout.PropertyField(branchNoiseStrength);
            EditorGUILayout.PropertyField(branchTwistJitter);
            EditorGUILayout.PropertyField(maxGeneratedBranches);
            EditorGUILayout.PropertyField(minBranchUpward);
            EditorGUILayout.PropertyField(clampBranchesAboveBase);
            if (clampBranchesAboveBase.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(branchGroundClearance);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(branchBlendDistance);

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Canopy Targeting", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(canopyTargetEnabled);
            if (canopyTargetEnabled.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(canopyCenterOffset);
                EditorGUILayout.PropertyField(canopyRadii);
                EditorGUILayout.PropertyField(canopyAttraction);
                EditorGUILayout.PropertyField(canopySurfaceTarget);
                EditorGUILayout.PropertyField(canopyHeightStart);
                EditorGUILayout.PropertyField(canopyHeightEnd);
                EditorGUI.indentLevel--;
            }
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
                showPlaneLeafSettings = EditorGUILayout.Foldout(showPlaneLeafSettings, "Plane Leaf Settings", true);
                if (showPlaneLeafSettings)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(leafWidth);
                    EditorGUILayout.PropertyField(leafLength);
                    EditorGUILayout.PropertyField(leafDistanceFromBranch);
                    EditorGUILayout.PropertyField(enablePlaneLeafSizeByHeight, new GUIContent("Size By Height"));
                    if (enablePlaneLeafSizeByHeight.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(planeLeafSizeBottom, new GUIContent("Size Bottom"));
                        EditorGUILayout.PropertyField(planeLeafSizeTop, new GUIContent("Size Top"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.PropertyField(doubleSidedLeaves);
                    EditorGUI.indentLevel--;
                }
            }
            else if (mode == TreeGenerator.LeafGenerationMode.Clusters)
            {
                showClusterSettings = EditorGUILayout.Foldout(showClusterSettings, "Cluster Settings", true);
                if (showClusterSettings)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(clusterRadius);
                    EditorGUILayout.PropertyField(clusterSizeMin);
                    EditorGUILayout.PropertyField(clusterSizeMax);
                    EditorGUILayout.PropertyField(clusterOffset);
                    EditorGUILayout.PropertyField(clusterSegments);
                    EditorGUILayout.PropertyField(randomizeClusterRotation);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space(3);
                showClusterShape = EditorGUILayout.Foldout(showClusterShape, "Cluster Shape", true);
                if (showClusterShape)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(clusterShapeX);
                    EditorGUILayout.PropertyField(clusterShapeY);
                    EditorGUILayout.PropertyField(clusterShapeZ);
                    EditorGUILayout.PropertyField(clusterNoiseStrength);
                    EditorGUILayout.PropertyField(clusterNoiseScale);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space(3);
                showOuterShell = EditorGUILayout.Foldout(showOuterShell, "Outer Shell", true);
                if (showOuterShell)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(enableOuterShell);
                    if (enableOuterShell.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(outerShellThickness);
                        EditorGUILayout.PropertyField(outerShellTransparency);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
            }
            else if (mode == TreeGenerator.LeafGenerationMode.Domes)
            {
                showDomeSettings = EditorGUILayout.Foldout(showDomeSettings, "Dome Settings", true);
                if (showDomeSettings)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(domeRadius);
                    EditorGUILayout.PropertyField(domeOffset);
                    EditorGUILayout.PropertyField(domeSegments);
                    EditorGUILayout.PropertyField(randomizeDomeRotation);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space(3);
                showDomeShape = EditorGUILayout.Foldout(showDomeShape, "Dome Shape", true);
                if (showDomeShape)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(domeShapeX);
                    EditorGUILayout.PropertyField(domeShapeY);
                    EditorGUILayout.PropertyField(domeShapeZ);
                    EditorGUILayout.PropertyField(domeNoiseStrength);
                    EditorGUILayout.PropertyField(domeNoiseScale);
                    EditorGUI.indentLevel--;
                }
            }
            
            EditorGUILayout.Space(5);
            showLeafAppearance = EditorGUILayout.Foldout(showLeafAppearance, "Leaf Appearance", true);
            if (showLeafAppearance)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(leafTransparency);
                EditorGUILayout.PropertyField(leafUVRandomness);
                EditorGUILayout.PropertyField(leafUVNoiseStrength);
                EditorGUILayout.PropertyField(leafUVNoiseScale);

                EditorGUILayout.Space(3);
                if (mode == TreeGenerator.LeafGenerationMode.Planes)
                {
                    EditorGUILayout.PropertyField(planeLeafTextureTiling, new GUIContent("Texture Tiling"));
                }
                else if (mode == TreeGenerator.LeafGenerationMode.Clusters)
                {
                    EditorGUILayout.PropertyField(clusterTextureTiling, new GUIContent("Texture Tiling"));
                }
                else if (mode == TreeGenerator.LeafGenerationMode.Domes)
                {
                    EditorGUILayout.PropertyField(domeTextureTiling, new GUIContent("Texture Tiling"));
                }
                EditorGUI.indentLevel--;
            }
            
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
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
