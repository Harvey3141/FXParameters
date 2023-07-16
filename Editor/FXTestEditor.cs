#if UNITY_EDITOR
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FXExample))]
public class FXTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draw the default inspector

        FXExample myScript = (FXExample)target;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Save Preset 1"))
        {
            myScript.SavePreset1();
        }

        if (GUILayout.Button("Load Preset 1"))
        {
            myScript.LoadPreset1();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Save Preset 2"))
        {
            myScript.SavePreset2();
        }

        if (GUILayout.Button("Load Preset 2"))
        {
            myScript.LoadPreset2();
        }

        EditorGUILayout.EndHorizontal();
    }
}
#endif
