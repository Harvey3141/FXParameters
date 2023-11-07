using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using FX.Patterns;

[CustomEditor(typeof(TapPattern))]
public class TapPatternEditor : Editor
{
    public List<float> values = new List<float>();

    private int[] bars = { 1, 4, 8, 16 };
    private string[] barsLabels = { "1", "2", "4", "8" };

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Get the target script and its float value
        TapPattern script = (TapPattern)target;
        float floatValue = script._currentValue;

        // Draw the float value
       EditorGUI.BeginChangeCheck();
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
        if (GUILayout.Button("1", GUILayout.Width(30), GUILayout.Height(20))) {
            script.NumBeats = 1;
            script.ClearTriggers();
            script.AddTriggers(1);
        } 
        GUILayout.Space(10);
        if (GUILayout.Button("2", GUILayout.Width(30), GUILayout.Height(20))) {
            script.NumBeats = 2;
            script.ClearTriggers();
            script.AddTriggers(2);
        }
        GUILayout.Space(10);
        if (GUILayout.Button("4", GUILayout.Width(30), GUILayout.Height(20))) {
            script.NumBeats = 4;
            script.ClearTriggers();
            script.AddTriggers(4);
        }         
        GUILayout.Space(10);
        if (GUILayout.Button("8", GUILayout.Width(30), GUILayout.Height(20))) {
            script.NumBeats = 8;
            script.ClearTriggers();
            script.AddTriggers(8);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("16", GUILayout.Width(30), GUILayout.Height(20))) {
            script.NumBeats = 16;
            script.ClearTriggers();
            script.AddTriggers(16);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);

        //if (GUILayout.Button("Trigger")) script.AddTriggerAtCurrentTime();
        //if (GUILayout.Button("Clear")) script.ClearTriggers();
        
        Rect graphPosition = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight * 3);
        DrawGraph(graphPosition, script);

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && graphPosition.Contains(Event.current.mousePosition)) script.AddTriggerAtCurrentTime();
        if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && graphPosition.Contains(Event.current.mousePosition)) script.ClearTriggers();

    }

    public override bool RequiresConstantRepaint()
    {
        return true;
    }

    private void DrawGraph(Rect position, TapPattern script)
    {
        float colF = script._currentValue * 0.5f;
        Color col = new Color(colF, colF, colF);

        Handles.DrawSolidRectangleWithOutline(position, col, Color.black);

        float x = position.x;
        float y = position.y + position.height;


        // Draw beat line
        Handles.color = Color.gray;
        for (int i = 0; i < script.NumBeats; i++)
        {
            float barLine = x + position.width * ((1.0f / script.NumBeats) * i);
            Handles.DrawLine(new Vector3(barLine, y, 0), new Vector3(barLine, position.y, 0));
        }

        // Draw triggers
        Handles.color = Color.green;
        if (script.triggers != null) {
            foreach (float key in script.triggers.Keys.ToList())
            {
                float trigger = x + position.width * key;
                Handles.DrawLine(new Vector3(trigger, y, 0), new Vector3(trigger, position.y, 0));
            }
        } 

        // Draw playhead
        float lineX = x + position.width * script._previousPlayhead;
        Handles.color = Color.white;
        Handles.DrawLine(new Vector3(lineX, y, 0), new Vector3(lineX, position.y, 0));

    }

}
