using UnityEditor;
using UnityEngine;
using extOSC;
using System.Collections.Generic;
using FX;
using Newtonsoft.Json;

public class OSCMessageSimulator : EditorWindow
{
    private List<OSCMessageData> messages = new List<OSCMessageData>();
    private FX.FXEXTOSCManager oscManager;
    private const string MessageDataKey = "OSCMessageSimulatorData";


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

    private class OSCMessageData
    {
        public string Address = "/example";
        public List<OSCArgument> Arguments = new List<OSCArgument>();
    }

    [MenuItem("Tools/OSC Message Simulator")]
    public static void ShowWindow()
    {
        var window = GetWindow<OSCMessageSimulator>("OSC Message Simulator");
        window.minSize = new Vector2(300, 400);
        window.LoadMessages();

    }
    void OnEnable()
    {
        LoadMessages();
        EditorApplication.playModeStateChanged += SaveBeforePlayMode;
    }

    void OnDisable()
    {
        EditorApplication.playModeStateChanged -= SaveBeforePlayMode;
    }

    private void SaveBeforePlayMode(PlayModeStateChange state)
    {

        SaveMessages();
        
    }


    private void OnGUI()
    {
        EditorGUILayout.LabelField("OSC Message Simulator", EditorStyles.boldLabel);

        //if (GUILayout.Button("Save Messages"))
        //{
        //    SaveMessages();
        //}

        for (int m = 0; m < messages.Count; m++)
        {
            EditorGUILayout.BeginVertical("box");

            messages[m].Address = EditorGUILayout.TextField("Address", messages[m].Address).Trim();

            for (int i = 0; i < messages[m].Arguments.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                switch (messages[m].Arguments[i].Type)
                {
                    case ArgumentType.Int:
                        messages[m].Arguments[i].IntValue = EditorGUILayout.IntField($"Argument {i + 1}", messages[m].Arguments[i].IntValue);
                        break;

                    case ArgumentType.Float:
                        messages[m].Arguments[i].FloatValue = EditorGUILayout.FloatField($"Argument {i + 1}", messages[m].Arguments[i].FloatValue);
                        break;

                    case ArgumentType.Bool:
                        messages[m].Arguments[i].BoolValue = EditorGUILayout.Toggle($"Argument {i + 1}", messages[m].Arguments[i].BoolValue);
                        break;

                    case ArgumentType.String:
                        messages[m].Arguments[i].StringValue = EditorGUILayout.TextField($"Argument {i + 1}", messages[m].Arguments[i].StringValue).Trim();
                        break;
                }

                messages[m].Arguments[i].Type = (ArgumentType)EditorGUILayout.EnumPopup(messages[m].Arguments[i].Type, GUILayout.Width(60));

                if (GUILayout.Button("+", GUILayout.Width(30)))
                {
                    messages[m].Arguments.Insert(i + 1, new OSCArgument());
                }
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    messages[m].Arguments.RemoveAt(i);
                    i--;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Argument"))
            {
                messages[m].Arguments.Add(new OSCArgument());
            }

            if (GUILayout.Button("Send Message"))
            {
                SendOSCMessage(messages[m]);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove Message", GUILayout.Width(150)))
            {
                messages.RemoveAt(m);
                m--;
            }

            if (GUILayout.Button("Duplicate Message", GUILayout.Width(150)))
            {
                var duplicateMessage = new OSCMessageData
                {
                    Address = messages[m].Address,
                    Arguments = new List<OSCArgument>()
                };

                foreach (var arg in messages[m].Arguments)
                {
                    duplicateMessage.Arguments.Add(new OSCArgument
                    {
                        StringValue = arg.StringValue,
                        IntValue = arg.IntValue,
                        FloatValue = arg.FloatValue,
                        BoolValue = arg.BoolValue,
                        Type = arg.Type
                    });
                }

                messages.Insert(m + 1, duplicateMessage);
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Message"))
        {
            messages.Add(new OSCMessageData());
        }

        EditorGUILayout.Space();
    }

    private void SendOSCMessage(OSCMessageData msgData)
    {
        oscManager = FindObjectOfType<FXEXTOSCManager>();
        if (oscManager == null)
        {
            Debug.LogError("OSC Manager is not set.");
            return;
        }

        var message = new OSCMessage(msgData.Address);

        foreach (var arg in msgData.Arguments)
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


    private void SaveMessages()
    {
        var json = JsonConvert.SerializeObject(messages);
        EditorPrefs.SetString(MessageDataKey, json);
    }

    private void LoadMessages()
    {
        if (EditorPrefs.HasKey(MessageDataKey))
        {
            var json = EditorPrefs.GetString(MessageDataKey);
            var data = JsonConvert.DeserializeObject<List<OSCMessageData>>(json);  
            messages = new List<OSCMessageData>(data);
        }
        else
        {
            messages = new List<OSCMessageData>();
        }
    }

    void OnDestroy()
    {
        SaveMessages();
    }

    [System.Serializable]
    private class Serialization<T>
    {
        public List<T> Items;
        public Serialization(List<T> items)
        {
            Items = items;
        }
    }
}
