using UnityEditor;
using UnityEngine;
using extOSC;
using System.Collections.Generic;
using FX;

public class OSCMessageSimulator : EditorWindow
{
    private string oscAddress = "/example";
    private List<OSCArgument> oscArguments = new List<OSCArgument>();
    private FX.FXEXTOSCManager oscManager;

    private enum ArgumentType
    {
        Int,
        Float,
        Bool,
        String
    }

    private class OSCArgument
    {
        public string StringValue = "";
        public int IntValue = 0;
        public float FloatValue = 0.0f;
        public bool BoolValue = false;
        public ArgumentType Type = ArgumentType.String;
    }

    [MenuItem("Tools/OSC Message Simulator")]
    public static void ShowWindow()
    {
        GetWindow<OSCMessageSimulator>("OSC Message Simulator").minSize = new Vector2(300, 200);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("OSC Message Simulator", EditorStyles.boldLabel);
        oscAddress = EditorGUILayout.TextField("Address", oscAddress).Trim();

        for (int i = 0; i < oscArguments.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            switch (oscArguments[i].Type)
            {
                case ArgumentType.Int:
                    oscArguments[i].IntValue = EditorGUILayout.IntField($"Argument {i + 1}", oscArguments[i].IntValue);
                    break;

                case ArgumentType.Float:
                    oscArguments[i].FloatValue = EditorGUILayout.FloatField($"Argument {i + 1}", oscArguments[i].FloatValue);
                    break;

                case ArgumentType.Bool:
                    oscArguments[i].BoolValue = EditorGUILayout.Toggle($"Argument {i + 1}", oscArguments[i].BoolValue);
                    break;

                case ArgumentType.String:
                    oscArguments[i].StringValue = EditorGUILayout.TextField($"Argument {i + 1}", oscArguments[i].StringValue).Trim();
                    break;
            }

            oscArguments[i].Type = (ArgumentType)EditorGUILayout.EnumPopup(oscArguments[i].Type, GUILayout.Width(60));

            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                oscArguments.Insert(i + 1, new OSCArgument());
            }
            if (GUILayout.Button("-", GUILayout.Width(30)))
            {
                oscArguments.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Argument"))
        {
            oscArguments.Add(new OSCArgument());
        }

        if (GUILayout.Button("Send OSC Message"))
        {
            SendOSCMessage();
        }

        EditorGUILayout.Space();
    }

    private void SendOSCMessage()
    {
        oscManager = FindObjectOfType<FXEXTOSCManager>();
        if (oscManager == null)
        {
            Debug.LogError("OSC Manager is not set.");
            return;
        }

        var message = new OSCMessage(oscAddress);

        foreach (var arg in oscArguments)
        {
            switch (arg.Type)
            {
                case ArgumentType.Int:
                    message.AddValue(OSCValue.Int(arg.IntValue));
                    break;

                case ArgumentType.Float:
                    message.AddValue(OSCValue.Float(arg.FloatValue));
                    break;

                case ArgumentType.Bool:
                    message.AddValue(OSCValue.Bool(arg.BoolValue));
                    break;

                case ArgumentType.String:
                    message.AddValue(OSCValue.String(arg.StringValue));
                    break;
            }
        }

        oscManager.SendInternalMessage(message);
    }
}
