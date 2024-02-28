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


        private void Start()
        {
            SetupNodes();
            StartCoroutine(SendMessagesAtInterval(sendInterval));
            FXManager.Instance.onFXParamChanged += OnFXParamChanged;
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


        void OnFXParamChanged(string address, object value) {
            foreach (var node in oscNodes)
            {
                if (node.SendParamChanges) SendOSCMessage(address, node, value);
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
                default:
                    Debug.LogWarning($"Unsupported value type for OSC message: {value.GetType()}");
                    break;
            }
            node.MessageQueue.Enqueue(message);
        }


    }
}
