using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FX.Patterns;

[CustomEditor(typeof(OscillatorPattern))]
public class OscillatorPatternEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Get the target script and its float value
        OscillatorPattern script = (OscillatorPattern)target;
        float floatValue = script._currentValue;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Sine", GUILayout.Width(50), GUILayout.Height(20))) script.Oscillator = OscillatorPattern.OscillatorType.Sine;
        GUILayout.Space(10);
        if (GUILayout.Button("Squa", GUILayout.Width(50), GUILayout.Height(20))) script.Oscillator = OscillatorPattern.OscillatorType.Square;
        GUILayout.Space(10);
        if (GUILayout.Button("Tri", GUILayout.Width(50), GUILayout.Height(20))) script.Oscillator = OscillatorPattern.OscillatorType.Triangle;
        GUILayout.Space(10);
        if (GUILayout.Button("Saw", GUILayout.Width(50), GUILayout.Height(20))) script.Oscillator = OscillatorPattern.OscillatorType.Sawtooth;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(EditorGUIUtility.singleLineHeight*0.5f);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("1", GUILayout.Width(30), GUILayout.Height(20))) script.NumBeats = 1;
        GUILayout.Space(10);
        if (GUILayout.Button("2", GUILayout.Width(30), GUILayout.Height(20))) script.NumBeats = 2;
        GUILayout.Space(10);
        if (GUILayout.Button("4", GUILayout.Width(30), GUILayout.Height(20))) script.NumBeats = 4;
        GUILayout.Space(10);
        if (GUILayout.Button("8", GUILayout.Width(30), GUILayout.Height(20))) script.NumBeats = 8;
        GUILayout.Space(10);
        if (GUILayout.Button("16", GUILayout.Width(30), GUILayout.Height(20))) script.NumBeats = 16;
        GUILayout.Space(10);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);

        // Draw the float value
        EditorGUI.BeginChangeCheck();
        //floatValue = EditorGUILayout.FloatField("Value", floatValue);
        if (EditorGUI.EndChangeCheck())
        {
            // Update the float value in the script
            Undo.RecordObject(script, "Change Float Value");
            script._currentValue = floatValue;

            // Force a repaint of the inspector
            EditorApplication.QueuePlayerLoopUpdate();
        }

        // Draw the graph
        Rect graphPosition = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight * 3);
        DrawGraph(graphPosition, script);
    }

    public override bool RequiresConstantRepaint()
    {
        return true;
    }

    private void DrawGraph(Rect position, OscillatorPattern script)
    {
        float colF = script._currentValue * 0.5f;
        Color col = new Color(colF, colF, colF);

        Handles.DrawSolidRectangleWithOutline(position, col, Color.black);

        float x = position.x;
        float y = position.y + position.height;

        Vector3 previousPoint = new Vector3(x, y, 0);
        Vector3 currentPoint = new Vector3(x + position.width, y, 0);

        // Draw beat line
        Handles.color = Color.gray;
        for (int i = 0; i < script.NumBeats; i++)
        {
            float barLine = x + position.width * ((1.0f / script.NumBeats) * i);
            Handles.DrawLine(new Vector3(barLine, y, 0), new Vector3(barLine, position.y, 0));
        }

        // Draw playhead
        float lineX = x + position.width * script._phase;
        Handles.color = Color.white;
        Handles.DrawLine(new Vector3(lineX, y, 0), new Vector3(lineX, position.y, 0));

        Handles.color = Color.green;
        if (script._pattern != null) {
            for (int i = 0; i < script._pattern.Count - 1; i++)
            {
                float value1 = script._pattern[i];
                float value2 = script._pattern[i + 1];

                float x1 = x + position.width * i / (script._pattern.Count - 1);
                float x2 = x + position.width * (i + 1) / (script._pattern.Count - 1);

                float y1 = y - position.height * value1;
                float y2 = y - position.height * value2;

                Vector3 point1 = new Vector3(x1, y1, 0);
                Vector3 point2 = new Vector3(x2, y2, 0);

                Handles.DrawLine(point1, point2);
            }
        }
    }
}
