#if UNITY_EDITOR
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FXTest))]
public class FXTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draw the default inspector

        FXTest myScript = (FXTest)target;
        if (GUILayout.Button("Save Preset 1"))
        {
            myScript.SavePreset1();
        }

        if (GUILayout.Button("Load Preset 1"))
        {
            myScript.LoadPreset1();
        }

        if (GUILayout.Button("Save Preset 2"))
        {
            myScript.SavePreset2();
        }

        if (GUILayout.Button("Load Preset 2"))
        {
            myScript.LoadPreset2();
        }
    }
}
#endif
