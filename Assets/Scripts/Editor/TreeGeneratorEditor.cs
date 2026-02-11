using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TreeGenerator))]
public class TreeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TreeGenerator treeGenerator = (TreeGenerator)target;

        // Create a button at the top that regenerates the tree
        if (GUILayout.Button("Generate Tree", GUILayout.Height(30)))
        {
            treeGenerator.RegenerateTree();
        }

        // Add some space after the button
        EditorGUILayout.Space(10);

        // Draw the default inspector
        DrawDefaultInspector();
    }
}
