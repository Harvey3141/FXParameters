using UnityEditor;
using UnityEngine;
using FX.Patterns;

[CustomEditor(typeof(BPMManager))]
public class BPMManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BPMManager bpmCalculator = (BPMManager)target;
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Tap", GUILayout.Width(120), GUILayout.Height(40))) bpmCalculator.Tap();
        GUILayout.Space(10);
        if (GUILayout.Button("Sync Patterns", GUILayout.Width(120), GUILayout.Height(40))) bpmCalculator.ResetPhase();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(new GUIContent("÷2\n(Half BPM)", "Halves the BPM"), GUILayout.Width(120), GUILayout.Height(40)))
        {
            bpmCalculator.HalfBPM();
        }
        GUILayout.Space(10);
        if (GUILayout.Button(new GUIContent("x2\n(Double BPM)", "Doubles the BPM"), GUILayout.Width(120), GUILayout.Height(40)))
        {
            bpmCalculator.DoubleBPM();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        Rect rect = EditorGUILayout.GetControlRect(false, 20);
        EditorGUI.DrawRect(rect, bpmCalculator.indicatorColor);

    }

    public override bool RequiresConstantRepaint()
    {
        return true;
    }
}