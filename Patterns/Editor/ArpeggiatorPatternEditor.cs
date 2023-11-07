using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FX.Patterns;

[CustomEditor(typeof(ArpeggiatorPattern))]
public class ArpeggiatorPatternEditor : Editor
{
    public List<float> values = new List<float>();

    private int[] bars = { 1, 4, 8, 16 };
    private string[] barsLabels = { "1", "2", "4", "8" };

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Get the target script and its float value
        ArpeggiatorPattern script = (ArpeggiatorPattern)target;
        float floatValue = script._currentValue;

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

        GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("1", GUILayout.Width(30), GUILayout.Height(20))) script.NumSteps = 1;
        GUILayout.Space(10);
        if (GUILayout.Button("1/4", GUILayout.Width(30), GUILayout.Height(20))) script.NumSteps = 4;
        GUILayout.Space(10);
        if (GUILayout.Button("1/8", GUILayout.Width(30), GUILayout.Height(20))) script.NumSteps = 8;
        GUILayout.Space(10);
        if (GUILayout.Button("1/16", GUILayout.Width(30), GUILayout.Height(20))) script.NumSteps = 16;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
       
        GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);


        // Draw the graph
        Rect graphPosition = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight * 3);
        DrawGraph(graphPosition, script);

    }

    public override bool RequiresConstantRepaint()
    {
        return true;
    }

    private void DrawGraph(Rect position, ArpeggiatorPattern script)
    {
        float colF = script._currentValue * 0.5f;
        Color col = new Color(colF, colF, colF);

        Handles.DrawSolidRectangleWithOutline(position, col, Color.black);

        float x = position.x;
        float y = position.y + position.height;


        //// Draw timeline bars
        //Handles.color = Color.gray;
        //for (int i = 0; i < script.Bars; i++)
        //{
        //    float barLine = x + position.width * ((1.0f / script.Bars) * i);
        //    Handles.DrawLine(new Vector3(barLine, y, 0), new Vector3(barLine, position.y, 0));
        //}
        //
        //// Draw triggers
        //Handles.color = Color.green;
        //if (script.triggers != null)
        //{
        //    foreach (float key in script.triggers.Keys.ToList())
        //    {
        //        float trigger = x + position.width * key;
        //        Handles.DrawLine(new Vector3(trigger, y, 0), new Vector3(trigger, position.y, 0));
        //    }
        //}
        //
        //
        //// Draw playhead
        //float lineX = x + position.width * script._linePosition;
        //Handles.color = Color.white;
        //Handles.DrawLine(new Vector3(lineX, y, 0), new Vector3(lineX, position.y, 0));
        //

        // Draw current value
        Handles.color = Color.green;
        for (int i = 0; i < values.Count - 1; i++)
        {
            float value1 = values[i];
            float value2 = values[i + 1];
        
            float x1 = x + position.width * i / (values.Count - 1);
            float x2 = x + position.width * (i + 1) / (values.Count - 1);
        
            float y1 = y - position.height * value1;
            float y2 = y - position.height * value2;
        
            Vector3 point1 = new Vector3(x1, y1, 0);
            Vector3 point2 = new Vector3(x2, y2, 0);
        
            Handles.DrawLine(point1, point2);
        }
        
        values.Add(script._currentValue);
        
        if (values.Count > 1000)
        {
            values.RemoveAt(0);
        }
    }

}
