using UnityEditor;
using UnityEngine;
using FX.Patterns;

[CustomEditor(typeof(TapBpm))]
public class TapBpmEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TapBpm bpmCalculator = (TapBpm)target;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Tap", GUILayout.Width(120), GUILayout.Height(40))) bpmCalculator.Tap();
        GUILayout.Space(10);
        if (GUILayout.Button("Sync Patterns", GUILayout.Width(120), GUILayout.Height(40))) bpmCalculator.ResetPhase();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

    }
}