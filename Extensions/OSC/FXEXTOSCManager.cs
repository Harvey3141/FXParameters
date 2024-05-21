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
using System;
using FX.Patterns;
using Newtonsoft.Json;

namespace FX
{

    [System.Serializable]
    public class OSCNodeData
    {
        public int localPort;
        public string remoteHost;
        public int remotePort;
        public bool sendParamChanges;
        public bool sendColoursAsJson;

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

        public bool SendParamChanges = true;
        public bool SendColoursAsJson = false;

        public OSCNode(OSCReceiver receiver, OSCTransmitter transmitter, bool sendParamChanges, bool sendColoursAsJson)
        {
            Receiver          = receiver;
            MessageQueue      = new Queue<OSCMessage>();
            Transmitter       = transmitter;
            SendParamChanges  = sendParamChanges;
            SendColoursAsJson = sendColoursAsJson;
        }
    }

    public class FXEXTOSCManager : MonoBehaviour
    {
        public List<OSCNode> oscNodes = new List<OSCNode>();
        public float sendInterval = 0.1f;
        public int maxMessagesPerInterval = 10;

        private FXManager fXManager;
        public FXSceneManager fxSceneManager;

        private void Start()
        {
            SetupNodes();
            fXManager = FXManager.Instance;
            fXManager.onFXParamValueChanged      += OnFXParamValueChanged;
            fXManager.onFXParamAffectorChanged   += OnFXParamAffectorChanged;
            fXManager.onPresetLoaded             += OnPresetLoaded;

            fXManager.onFXGroupChanged           += OnFXGroupChanged;
            fXManager.onFXGroupListChanged       += OnFXGroupListChanged;
            fXManager.onFXGroupEnabled           += OnFXGroupEnabled;


            //fxSceneManager.onSceneListUpdated += OnPresetListUpdated;
            fxSceneManager.onCurrentSceneChanged += OnCurrentSceneChanged;
            StartCoroutine(SendMessagesAtInterval(sendInterval));
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

                    OSCNode node = new OSCNode(receiver, transmitter, nodeData.sendParamChanges, nodeData.sendColoursAsJson);
                    oscNodes.Add(node);
                }
            }
            else
            {
                Debug.LogError($"Config file {filePath} not found.");
            }
        }

        public void SendInternalMessage(OSCMessage message)
        {
            int port = 8000;
            MessageReceived(message, port);
        }

        protected void MessageReceived(OSCMessage message, int port)
        {
            string address = message.Address;
            if (address == "/FX/GET")
            {
                if (message.Values.Count > 0)
                {
                    string fxAddress = message.Values[0].StringValue;
                    object value = fXManager.GetFX(fxAddress);

                    if (value != null)
                    {
                        OSCNode matchingNode = oscNodes.Find(node => node.Receiver.LocalPort == port);
                        if (matchingNode != null)
                        {
                            string senderIp = matchingNode.Transmitter.RemoteHost.ToString();
                            int senderPort = matchingNode.Transmitter.RemotePort;
                            SendOSCMessage(fxAddress, matchingNode, value);
                            
                        }
                        // TODO - create a transmitter here to send back a response.
                    }
                }
            }
            else if (address.ToUpper() == "/FX/SET")
            {
                string fxAddress = message.Values[0].StringValue;
                object[] args = new object[message.Values.Count - 1];

                for (int i = 1; i < message.Values.Count; i++)
                {
                    int argsIndex = i - 1;

                    switch (message.Values[i].Type)
                    {
                        case OSCValueType.Float:
                            args[argsIndex] = message.Values[i].FloatValue;
                            break;
                        case OSCValueType.True:
                            args[argsIndex] = true;
                            break;
                        case OSCValueType.False:
                            args[argsIndex] = false;
                            break;
                        case OSCValueType.Int:
                            args[argsIndex] = message.Values[i].IntValue;
                            break;
                        case OSCValueType.String:
                            string stringValue = message.Values[i].StringValue;
                            if (TryGetColorFromJsonString(stringValue, out Color colorValue))
                            {
                                args[argsIndex] = colorValue; 
                            }
                            else
                            {
                                args[argsIndex] = stringValue;
                            }
                            break;
                        case OSCValueType.Color:
                            args[argsIndex] = message.Values[i].ColorValue;
                            break;
                    }
                }
                fXManager.SetFX(fxAddress, args, true);
            }
            else if (address == "/FX/RESET")
            {
                if (message.Values.Count > 0)
                {
                    string fxAddress = message.Values[0].StringValue;
                    fXManager.ResetParameterToDefault(fxAddress);
                }
            }
            else if (address == "/FX/RESETTOSCENEDEFAULT")
            {
                if (message.Values.Count > 0)
                {
                    string fxAddress = message.Values[0].StringValue;
                    fXManager.ResetParameterToSceneDefault(fxAddress);
                }
            }
            else if (address.ToUpper() == "/SCENE/LOAD")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    string sceneName = message.Values[0].StringValue;
                    fxSceneManager.LoadScene(sceneName);
                }
            }
            else if (address.ToUpper() == "/SCENE/NAME/GET")
            {
                OSCNode matchingNode = oscNodes.Find(node => node.Receiver.LocalPort == port);
                if (matchingNode != null)
                {
                    string senderIp = matchingNode.Transmitter.RemoteHost.ToString();
                    int senderPort = matchingNode.Transmitter.RemotePort;
                    SendOSCMessage("/scene/name/get", matchingNode, fxSceneManager.CurrentScene.Name);
                }
            }
            else if (address.ToUpper() == "/SCENE/NAME/SET")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    string sceneName = message.Values[0].StringValue;
                    fxSceneManager.CurrentScene.Name = sceneName;
                }
            }
            else if (address.ToUpper() == "/SCENE/SAVE")
            {
                if (message.Values.Count == 0) fxSceneManager.SaveScene();
                else if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    string sceneName = message.Values[0].StringValue;
                    fxSceneManager.SaveScene();
                }
            }
            else if (address.ToUpper() == "/SCENE/REMOVE")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    string sceneName = message.Values[0].StringValue;
                    fxSceneManager.RemoveScene(sceneName);
                }
            }
            else if (address.ToUpper() == "/SCENE/NEW")
            {
                fxSceneManager.CreateNewScene();
            }
            else if (address.ToUpper() == "/SCENE/RESET")
            {
                fxSceneManager.ResetCurrentScene();
            }
            else if (address.ToUpper() == "/SCENELIST/GET")
            {

                string json = JsonConvert.SerializeObject(fxSceneManager.scenes);

                OSCNode matchingNode = oscNodes.Find(node => node.Receiver.LocalPort == port);
                if (matchingNode != null)
                {
                    string senderIp = matchingNode.Transmitter.RemoteHost.ToString();
                    int senderPort = matchingNode.Transmitter.RemotePort;
                    SendOSCMessage("/sceneList/get", matchingNode, json);
                }
            }
            else if (address.ToUpper() == "/SCENE/TAG/ADD")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String )
                {
                    fxSceneManager.AddTagToCurrentScene(message.Values[0].StringValue);
                }
            }
            else if (address.ToUpper() == "/SCENE/TAG/REMOVE")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    fxSceneManager.RemoveTagFromCurrentScene(message.Values[0].StringValue);
                }
            }
            else if (address.ToUpper() == "/SCENE/TAGS/CLEAR")
            {
                fxSceneManager.RemoveAllTagsFromCurrentScene();              
            }

            else if (address.ToUpper() == "/TAG/NEW")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.String)
                {
                    fxSceneManager.AddTagToConfiguration(message.Values[0].StringValue, message.Values[1].StringValue);
                }
            }
            else if (address.ToUpper() == "/TAG/REMOVE")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    fxSceneManager.RemoveTagFromConfiguration(message.Values[0].StringValue);
                }
            }
            else if (address.ToUpper() == "/TAG/SET")
            {
                FX.Tag tag = JsonConvert.DeserializeObject<FX.Tag>(message.Values[0].StringValue);
                fxSceneManager.SetTag(tag);
            }

            else if (address.ToUpper() == "/GROUP/NEW")
            {
                if (message.Values.Count > 0) {
                    string json = message.Values[0].StringValue;
                    FXGroupData preset = JsonConvert.DeserializeObject<FXGroupData>(json);
                    fXManager.CreateGroup(preset);
                }
                else fXManager.CreateGroup();
            }
            else if (address.ToUpper() == "/GROUP/REMOVE")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    string a = message.Values[0].StringValue;
                    fXManager.RemoveGroup(a);
                }
            }
            else if (address.ToUpper() == "/GROUP/CLEAR")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    string a = message.Values[0].StringValue;
                    fXManager.ClearGroup(a);
                }
            }
            else if (address.ToUpper() == "/GROUP/RESET")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    GroupFXController group = fXManager.FindGroupByAddress(message.Values[0].StringValue);
                    if (group != null)
                    {
                        group.ResetGroupToLastLoadedState();
                    }
                }
            }
            else if (address.ToUpper() == "/GROUP/GET")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    GroupFXController group = fXManager.FindGroupByAddress(message.Values[0].StringValue);
                    if (group != null)
                    {
                        OnFXGroupChanged(group.GetData());
                    }
                }
            }
            else if (address.ToUpper() == "/GROUP/SET")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.String)
                {
                    string json = message.Values[1].StringValue;
                    FXGroupData preset = JsonConvert.DeserializeObject<FXGroupData>(json);
                    fXManager.SetGroup(preset);
                }
            }
            else if (address.ToUpper() == "/GROUPLIST/GET")
            {
                string json = JsonConvert.SerializeObject(fXManager.GetGroupList());

                OSCNode matchingNode = oscNodes.Find(node => node.Receiver.LocalPort == port);
                if (matchingNode != null)
                {
                    string senderIp = matchingNode.Transmitter.RemoteHost.ToString();
                    int senderPort = matchingNode.Transmitter.RemotePort;
                    SendOSCMessage("/groupList/get", matchingNode, json);
                }
            }
            else if (address.ToUpper() == "/GROUP/PARAM/ADD")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.String)
                {
                    string groupAddress = message.Values[0].StringValue;
                    string paramAddress = message.Values[1].StringValue;
                    fXManager.AddFXParamToGroup(groupAddress, paramAddress);
                }
            }
            else if (address.ToUpper() == "/GROUP/PARAM/REMOVE")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.String)
                {
                    string groupAddress = message.Values[0].StringValue;
                    string paramAddress = message.Values[1].StringValue;
                    fXManager.RemoveFXParamFromGroup(groupAddress, paramAddress);
                }
            }
            else if (address.ToUpper() == "/GROUP/PARAMS/REMOVE")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    string groupAddress = message.Values[0].StringValue;
                    fXManager.RemoveFXParamsFromGroup(groupAddress);
                }
            }
            else if (address.ToUpper() == "/GROUP/PARAM/GET")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.String)
                {
                    FXParameterControllerData param = fXManager.GetGroupFXParamData(message.Values[0].StringValue, message.Values[1].StringValue);
                    if (param != null)
                    {
                        string json = JsonConvert.SerializeObject(param);
                        OSCNode matchingNode = oscNodes.Find(node => node.Receiver.LocalPort == port);
                        if (matchingNode != null)
                        {
                            string senderIp = matchingNode.Transmitter.RemoteHost.ToString();
                            int senderPort = matchingNode.Transmitter.RemotePort;
                            SendOSCMessage("/group/param/get", matchingNode, json);
                        }
                    }
                }
            }
            else if (address.ToUpper() == "/GROUP/PARAM/SET")
            {
                if (message.Values.Count > 2 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.String && message.Values[2].Type == OSCValueType.String)
                {
                    string json = message.Values[2].StringValue;
                    FXParameterControllerData param = JsonConvert.DeserializeObject<FXParameterControllerData>(json);
                    if (string.IsNullOrEmpty(param.key)) param.key = message.Values[1].StringValue;
                    fXManager.SetGroupFXParam(message.Values[0].StringValue, message.Values[1].StringValue, param);
                }
            }
            else if (address.ToUpper() == "/GROUP/PARAM/ENABLED/GET")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.String)
                {
                    FXParameterControllerData param = fXManager.GetGroupFXParamData(message.Values[0].StringValue, message.Values[1].StringValue);
                    if (param != null) 
                    {
                        OSCNode matchingNode = oscNodes.Find(node => node.Receiver.LocalPort == port);
                        if (matchingNode != null)
                        {
                            string senderIp = matchingNode.Transmitter.RemoteHost.ToString();
                            int senderPort = matchingNode.Transmitter.RemotePort;
                            SendOSCMessage("/group/param/enabled/get", matchingNode, param.enabled);
                        }
                    }
                }
            }
            else if (address.ToUpper() == "/GROUP/PARAM/ENABLED/SET")
            {
                if (message.Values.Count > 2 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.String && (message.Values[2].Type == OSCValueType.True || message.Values[2].Type == OSCValueType.False ))
                {
                    GroupFXController group = fXManager.FindGroupByAddress(message.Values[0].StringValue);
                    group.SetParameterEnabled(message.Values[1].StringValue, message.Values[2].BoolValue);
                }
                else if (message.Values.Count > 2 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.String && (message.Values[2].Type == OSCValueType.Int))
                {
                    GroupFXController group = fXManager.FindGroupByAddress(message.Values[0].StringValue);
                    group.SetParameterEnabled(message.Values[1].StringValue, message.Values[2].IntValue == 0 ? false : true );
                }

            }
            else if (address.ToUpper() == "/GROUP/TRIGGER/ADD")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.String)
                {
                    string groupAddress = message.Values[0].StringValue;
                    string paramAddress = message.Values[1].StringValue;
                    fXManager.AddFXTriggerToGroup(groupAddress, paramAddress);
                }
            }
            else if (address.ToUpper() == "/GROUP/TRIGGER/REMOVE")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.String)
                {
                    string groupAddress = message.Values[0].StringValue;
                    string paramAddress = message.Values[1].StringValue;
                    fXManager.RemoveFXTriggerFromGroup(groupAddress, paramAddress);
                }
            }
            else if (address.ToUpper() == "/GROUP/ENABLED/SET")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && (message.Values[1].Type == OSCValueType.True || message.Values[1].Type == OSCValueType.False))
                {
                    string groupAddress = message.Values[0].StringValue;
                    bool state = message.Values[1].BoolValue;
                    var g = fXManager.FindGroupByAddress(groupAddress);
                    if (g != null)
                    {
                        g.Active = state;
                    }

                }
            }
            else if (address.ToUpper() == "/GROUP/ENABLED/GET")
            {
                if (message.Values.Count > 0 && message.Values[0].Type == OSCValueType.String)
                {
                    string groupAddress = message.Values[0].StringValue;
                    var g = fXManager.FindGroupByAddress(groupAddress);
                    if (g != null)
                    {
                        bool state = g.Active;
                        OSCNode matchingNode = oscNodes.Find(node => node.Receiver.LocalPort == port);
                        if (matchingNode != null)
                        {
                            string senderIp = matchingNode.Transmitter.RemoteHost.ToString();
                            int senderPort = matchingNode.Transmitter.RemotePort;
                            SendOSCMessage("/group/enabled/get", matchingNode, state);
                        }
                    }
                }
            }
            else if (address.ToUpper() == "/GROUP/PATTERN/NUMBEATS")
            {
                if (message.Values.Count > 1 && message.Values[0].Type == OSCValueType.String && message.Values[1].Type == OSCValueType.Int)
                {
                    string groupAddress = message.Values[0].StringValue;
                    int numBeats = message.Values[1].IntValue;
                    var g = fXManager.FindGroupByAddress(groupAddress);
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
                    var g = fXManager.FindGroupByAddress(groupAddress);
                    if (g != null)
                    {
                        if (g.signalSource == GroupFXController.SignalSource.Pattern && g.patternType == GroupFXController.PatternType.Tap)
                        {
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

                    var g = fXManager.FindGroupByAddress(groupAddress);
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
                    var g = fXManager.FindGroupByAddress(groupAddress);
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
        }


        void OnFXParamValueChanged(string address, object value)
        {
            if (!string.IsNullOrEmpty(address) && !address.Contains("/Group/"))
            {
                foreach (var node in oscNodes)
                {
                    if (node.SendParamChanges) SendOSCMessage("/fx/get", node, address, value);
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

        void OnFXGroupChanged(FXGroupData data)
        {
            var message = new OSCMessage("/group/get");
            message.AddValue(OSCValue.String(data.address));
            message.AddValue(OSCValue.String(JsonConvert.SerializeObject(data)));
            foreach (var node in oscNodes)
            {
                if (node.SendParamChanges) node.MessageQueue.Enqueue(message);
            }
        }

        void OnFXGroupEnabled(string adress, bool state)
        {
            var message = new OSCMessage("/group/enabled/get");
            message.AddValue(OSCValue.String(adress));
            message.AddValue(OSCValue.Bool(state));
            foreach (var node in oscNodes)
            {
                if (node.SendParamChanges) node.MessageQueue.Enqueue(message);
            }
        }

        void OnFXGroupListChanged(List<string> groupListIn)
        {

            string json = JsonConvert.SerializeObject(groupListIn);

            var message = new OSCMessage("/groupList/get");
            message.AddValue(OSCValue.String(json));

            foreach (var node in oscNodes)
            {
                if (node.SendParamChanges) node.MessageQueue.Enqueue(message);
            }
        }

        void OnPresetLoaded(string name) 
        {
            foreach (var node in oscNodes)
            {
                if (node.SendParamChanges) SendOSCMessage("/scene/load", node, name);
            }
        }

        void OnPresetListUpdated(List<string> presets) 
        {

            string json = JsonConvert.SerializeObject(presets);

            foreach (var node in oscNodes)
            {
                if (node.SendParamChanges) SendOSCMessage("/sceneList/get", node, json);
            }
        }

        void OnCurrentSceneChanged(Scene scene)
        {
            foreach (var node in oscNodes)
            {
                if (node.SendParamChanges) SendOSCMessage("/scene/name/get", node, scene.Name);
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
                            node.Transmitter.Send(node.MessageQueue.Dequeue());
                        }
                    }
                }
                yield return new WaitForSeconds(interval);
            }
        }

        void SendOSCMessage(string address, OSCNode node, object value = null)
        {
            var message = new OSCMessage(address);
            OSCValue oscValue = CreateOSCValueFromObject(value, node.SendColoursAsJson);

            if (oscValue != null)
            {
                message.AddValue(oscValue);
            }
            else
            {
                Debug.LogWarning($"Unsupported value type for OSC message: {value.GetType()}, address: {address}");
            }

            /// TODO - optimisation, replace messages with matching address 
            node.MessageQueue.Enqueue(message);
        }

        void SendOSCMessage(string address, OSCNode node, string value1S, object value2)
        {
            var message = new OSCMessage(address);
            message.AddValue(OSCValue.String(value1S));
            OSCValue oscValue2 = CreateOSCValueFromObject(value2, node.SendColoursAsJson);

            if (oscValue2 == null)
            {
                Debug.LogWarning($"Unsupported value type for the second value: {value2.GetType()}, address: {address}");
                return; 
            }          
            message.AddValue(oscValue2);
            node.MessageQueue.Enqueue(message);
        }


        OSCValue CreateOSCValueFromObject(object value, bool sendColoursAsJson)
        {
            return value switch
            {
                float floatValue                        => OSCValue.Float(floatValue),
                int intValue                            => OSCValue.Int(intValue),
                string stringValue                      => OSCValue.String(stringValue),
                bool boolValue                          => OSCValue.Bool(boolValue),
                Enum enumValue                          => OSCValue.Int(Convert.ToInt32(enumValue)),
                Color colorValue when sendColoursAsJson => OSCValue.String(ColorToJson(colorValue)),
                Color colorValue                        => OSCValue.Color(colorValue),
                _                                       => null
            };
        }

        private string ColorToJson(Color color)
        {
            ColorData colorData = new ColorData
            {
                r = Mathf.RoundToInt(color.r * 255),
                g = Mathf.RoundToInt(color.g * 255),
                b = Mathf.RoundToInt(color.b * 255),
                a = color.a
            };
            return JsonUtility.ToJson(colorData);
        }

        private bool TryGetColorFromJsonString(string jsonString, out Color color)
        {
            color = default;
            try
            {
                ColorData colorData = JsonUtility.FromJson<ColorData>(jsonString);
                if (colorData != null && colorData.r >= 0 && colorData.g >= 0 && colorData.b >= 0)
                {
                    float alpha = colorData.a >= 0 ? colorData.a : 1f; 
                    color = new Color(colorData.r / 255f, colorData.g / 255f, colorData.b / 255f, alpha);
                    return true;
                }
            }
            catch
            {
               
            }
            return false;
        }

        [System.Serializable]
        private class ColorData
        {
            public float r;
            public float g;
            public float b;
            public float a = 1;
        }

    }
}
