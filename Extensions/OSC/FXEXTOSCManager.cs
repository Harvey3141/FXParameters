// Manages OSC communication for FX parameters, including dynamic osc node (sender / reciever) setup and message control.

// 1. Loads OSC node settings from a JSON file for flexible setup.
// 2. Sends messages at specified intervals with a controlled rate to manage traffic.
// 3. Processes "/GET" requests and other messages to update or fetch FX parameters.
// 4.  Optionally sends real-time FX parameter changes to configured OSC nodes.


using UnityEngine;
using extOSC;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System;
using FX.Patterns;

namespace FX
{

    [System.Serializable]
    public class OSCNodeData
    {
        public int localPort;
        public string remoteHost;
        public int remotePort;
        public bool sendParamChanges;
    }

    [System.Serializable]
    public class OSCNodeList
    {
        public float sendInterval = 0.1f; 
        public int maxMessagesPerInterval = 10; 
        public List<OSCNodeData> nodes;
    }

    public class OSCNode
    {
        public OSCReceiver Receiver { get; private set; }
        public Queue<OSCMessage> MessageQueue { get; private set; }
        public OSCTransmitter Transmitter { get; private set; }

        public bool SendParamChanges;

        public OSCNode(OSCReceiver receiver, OSCTransmitter transmitter, bool sendParamChanges)
        {
            Receiver = receiver;
            MessageQueue = new Queue<OSCMessage>();
            Transmitter = transmitter;
            SendParamChanges = sendParamChanges;
        }
    }

    public class FXEXTOSCManager : MonoBehaviour
    {

        public List<OSCNode> oscNodes = new List<OSCNode>();
        public float sendInterval = 0.1f;
        public int maxMessagesPerInterval = 10;

        public FXSceneManager fxSceneManager;

        private void Start()
        {
            SetupNodes();
            StartCoroutine(SendMessagesAtInterval(sendInterval));
            FXManager.Instance.onFXParamValueChanged    += OnFXParamValueChanged;
            FXManager.Instance.onFXParamAffectorChanged += OnFXParamAffectorChanged;
            FXManager.Instance.onPresetLoaded           += OnPresetLoaded;

            fxSceneManager.onPresetListUpdated        += OnPresetListUpdated;
            fxSceneManager.onCurrentPresetNameChanged += OnCurrentPresetNameChanged;

        }

        private void SetupNodes()
        {
            string directoryPath = Path.Combine(Application.streamingAssetsPath, "FX");
            string filePath = Path.Combine(directoryPath, "OSCConfig.json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                OSCNodeList nodeList = JsonUtility.FromJson<OSCNodeList>(json);

                sendInterval = nodeList.sendInterval;
                maxMessagesPerInterval = nodeList.maxMessagesPerInterval;

                foreach (OSCNodeData nodeData in nodeList.nodes)
                {
                    var receiver = gameObject.AddComponent<OSCReceiver>();
                    receiver.LocalPort = nodeData.localPort;
                    receiver.Bind("/*", (message) => MessageReceived(message, nodeData.localPort));

                    var transmitter = gameObject.AddComponent<OSCTransmitter>();
                    transmitter.RemoteHost = nodeData.remoteHost;
                    transmitter.RemotePort = nodeData.remotePort;

                    OSCNode node = new OSCNode(receiver, transmitter, nodeData.sendParamChanges);
                    oscNodes.Add(node);
                }
            }
            else
            {
                Debug.LogError($"Config file {filePath} not found.");
            }
        }



        protected void MessageReceived(OSCMessage message, int port)
        {
            string address = message.Address;
            if (address.ToUpper().EndsWith("/GET"))
            {
                string paramAddress = address.Substring(0, address.Length - 4);
                object value = FXManager.Instance.GetFX(paramAddress);

                if (value != null)
                {
                    OSCNode matchingNode = oscNodes.Find(node => node.Receiver.LocalPort == port);
                    if (matchingNode != null)
                    {
                        string senderIp = matchingNode.Transmitter.RemoteHost.ToString();
                        int senderPort = matchingNode.Transmitter.RemotePort;
                        SendOSCMessage(paramAddress, matchingNode, value);
                    }
                    // TODO - create transmitter 
                }
            }
            if (address.ToUpper().EndsWith("/RESET"))
            {
                string paramAddress = address.Substring(0, address.Length - 4);
                FXManager.Instance.ResetParameterToDefault(paramAddress);
            }
            else if (address.ToUpper() == "/SCENE/LOAD")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    string sceneName = message.Values[0].StringValue;
                    fxSceneManager.LoadPreset(sceneName);
                }
            }
            else if (address.ToUpper() == "/SCENE/NAME/SET")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    string sceneName = message.Values[0].StringValue;
                    fxSceneManager.CurrentPresetName = sceneName;
                }
            }
            else if (address.ToUpper() == "/SCENE/SAVE")
            {
                if (message.Values.Count == 0) fxSceneManager.SavePreset();
                else if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    string sceneName = message.Values[0].StringValue;
                    fxSceneManager.SavePreset(sceneName);
                }
            }
            else if (address.ToUpper() == "/SCENE/REMOVE")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    string sceneName = message.Values[0].StringValue;
                    fxSceneManager.RemovePreset(sceneName);
                }
            }
            else if (address.ToUpper() == "/SCENE/NEW")
            {
                // Reset to all params to default
                // Load default groups ? 
            }
            else if (address.ToUpper() == "/GROUP/CREATE/JSON")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    string json = message.Values[0].StringValue;
                    FXGroupData preset = JsonUtility.FromJson<FXGroupData>(json);
                    FXManager.Instance.CreateGroup(preset);
                }
            }
            else if (address.ToUpper() == "/GROUP/PARAM/ADD")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.String)
                {
                    string groupAddress = message.Values[0].StringValue;
                    string paramAddress = message.Values[1].StringValue;
                    FXManager.Instance.AddFXParamToGroup(groupAddress, paramAddress);
                }
            }
            else if (address.ToUpper() == "/GROUP/PARAM/REMOVE")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.String)
                {
                    string groupAddress = message.Values[0].StringValue;
                    string paramAddress = message.Values[1].StringValue;
                    FXManager.Instance.RemoveFXParamFromGroup(groupAddress, paramAddress);
                }
            }
            else if (address.ToUpper() == "/GROUP/TRIGGER/ADD")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.String)
                {
                    string groupAddress = message.Values[0].StringValue;
                    string paramAddress = message.Values[1].StringValue;
                    FXManager.Instance.AddFXTriggerToGroup(groupAddress, paramAddress);
                }
            }
            else if (address.ToUpper() == "/GROUP/TRIGGER/REMOVE")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.String)
                {
                    string groupAddress = message.Values[0].StringValue;
                    string paramAddress = message.Values[1].StringValue;
                    FXManager.Instance.RemoveFXTriggerFromGroup(groupAddress, paramAddress);
                }
            }
            else if (address.ToUpper() == "/GROUP/ENABLED/SET")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && (message.Values[1].Type == OSCValueType.True || message.Values[1].Type == OSCValueType.False))
                {
                    string groupAddress = message.Values[0].StringValue;
                    bool state = message.Values[1].BoolValue;
                    var g = FXManager.Instance.FindGroupByAddress(groupAddress);
                    if (g != null) {
                        g.Active = state;
                    }
                }
            }
            else if (address.ToUpper() == "/GROUP/PATTERN/NUMBEATS")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.Int)
                {
                    string groupAddress = message.Values[0].StringValue;
                    int numBeats  = message.Values[1].IntValue;
                    var g = FXManager.Instance.FindGroupByAddress(groupAddress);
                    if (g != null)
                    {
                        if (g.signalSource == GroupFXController.SignalSource.Pattern) g.SetPatternNumBeats(numBeats);
                    }
                }
            }
            else if (address.ToUpper() == "/GROUP/TAP/ADDTRIGGERATCURRENTTIME")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    string groupAddress = message.Values[0].StringValue;
                    var g = FXManager.Instance.FindGroupByAddress(groupAddress);
                    if (g != null)
                    {
                        if (g.signalSource == GroupFXController.SignalSource.Pattern && g.patternType == GroupFXController.PatternType.Tap) { 
                            TapPattern tp = (TapPattern)g.pattern;
                            tp.AddTriggerAtCurrentTime();
                        }
                    }
                }
            }
            else if (address.ToUpper() == "/GROUP/TAP/NUMBEROFTRIGGERS/SET")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.Int)
                {
                    string groupAddress = message.Values[0].StringValue;
                    int numTriggers = message.Values[1].IntValue;

                    var g = FXManager.Instance.FindGroupByAddress(groupAddress);
                    if (g != null)
                    {
                        if (g.signalSource == GroupFXController.SignalSource.Pattern && g.patternType == GroupFXController.PatternType.Tap)
                        {
                            TapPattern tp = (TapPattern)g.pattern;
                            tp.AddTriggers(numTriggers);
                        }
                    }
                }
            }
            else if (address.ToUpper() == "/GROUP/TAP/CLEARTRIGGERS")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    string groupAddress = message.Values[0].StringValue;
                    var g = FXManager.Instance.FindGroupByAddress(groupAddress);
                    if (g != null)
                    {
                        if (g.signalSource == GroupFXController.SignalSource.Pattern && g.patternType == GroupFXController.PatternType.Tap)
                        {
                            TapPattern tp = (TapPattern)g.pattern;
                            tp.ClearTriggers();
                        }
                    }
                }
            }
            else
            {
                object[] args = new object[message.Values.Count];

                for (int i = 0; i < args.Length; i++)
                {
                    switch (message.Values[i].Type)
                    {
                        case OSCValueType.Float:
                            args[i] = message.Values[0].FloatValue;
                            break;
                        case OSCValueType.True:
                            args[i] = true;
                            break;
                        case OSCValueType.False:
                            args[i] = false;
                            break;
                        case OSCValueType.Int:
                            args[i] = message.Values[0].IntValue;
                            break;
                        case OSCValueType.String:
                            args[i] = message.Values[0].StringValue;
                            break;
                        case OSCValueType.Color:
                            args[i] = message.Values[0].ColorValue;
                            break;
                    }
                }
                FXManager.Instance.SetFX(address, args);
            }
        }


        void OnFXParamValueChanged(string address, object value)
        {
            if (!string.IsNullOrEmpty(address) && !Regex.IsMatch(address, @"^/Group\d"))
            {
                foreach (var node in oscNodes)
                {
                    if (node.SendParamChanges) SendOSCMessage(address, node, value);
                }
            }
        }

        void OnFXParamAffectorChanged(string address, AffectorFunction affector)
        {
            address += "/affector";
            foreach (var node in oscNodes)
            {
                if (node.SendParamChanges) SendOSCMessage(address, node, affector.ToString());
            }         
        }

        void OnPresetLoaded(string name) 
        {
            foreach (var node in oscNodes)
            {
                if (node.SendParamChanges) SendOSCMessage("/scene/loaded", node, name);
            }
        }

        void OnPresetListUpdated(List<string> presets) 
        {
            string presetListString = "{" + string.Join(",", presets) + "}";

            foreach (var node in oscNodes)
            {
                if (node.SendParamChanges) SendOSCMessage("/sceneList/get", node, presetListString);
            }
        }

        void OnCurrentPresetNameChanged(string name)
        {
            foreach (var node in oscNodes)
            {
                if (node.SendParamChanges) SendOSCMessage("/scene/name/get", node, name);
            }
        }


        IEnumerator SendMessagesAtInterval(float interval)
        {
            while (true)
            {
                foreach (var node in oscNodes)
                {
                    int messagesToSend = Mathf.Min(node.MessageQueue.Count, maxMessagesPerInterval);
                    for (int i = 0; i < messagesToSend; i++)
                    {
                        if (node.MessageQueue.Count > 0) 
                        {
                            OSCMessage messageToSend = node.MessageQueue.Dequeue();
                            node.Transmitter.Send(messageToSend);
                        }
                    }
                }
                yield return new WaitForSeconds(interval);
            }
        }


        void SendOSCMessage(string address, OSCNode node, object value)
        {
            var message = new OSCMessage(address);
            switch (value)
            {
                case float floatValue:
                    message.AddValue(OSCValue.Float(floatValue));
                    break;
                case int intValue:
                    message.AddValue(OSCValue.Int(intValue));
                    break;
                case string stringValue:
                    message.AddValue(OSCValue.String(stringValue));
                    break;
                case bool boolValue:
                    message.AddValue(OSCValue.Bool(boolValue));
                    break;
                case Enum enumValue:
                    int enumIntValue = Convert.ToInt32(enumValue);
                    message.AddValue(OSCValue.Int(enumIntValue));
                    break;
                case Color colorValue:
                    message.AddValue(OSCValue.Color(colorValue));
                    break;
                default:
                    Debug.LogWarning($"Unsupported value type for OSC message: {value.GetType()} , address: {address} ");
                    break;
            }
            node.MessageQueue.Enqueue(message);
        }

    }
}
