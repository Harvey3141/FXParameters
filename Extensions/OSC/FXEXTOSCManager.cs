// 1. Define Listening Ports:
//    - The `listeningPorts` list holds the ports that the OSC receivers will listen on.
//    - You can add or remove ports in the Unity Inspector

// 2. Receiver and Transmitter Creation:
//    - Receivers are automatically created for each port defined in `listeningPorts`.
//    - Transmitters are dynamically created to send responses. 
//      They respond to the sender's IP but on a different port.

// 3. Handling "/GET" Requests:
//    - If a message with an address ending in "/GET" is received, the manager will 
//      look up a corresponding value from `FXManager` and send it back.
//    - The response is sent to the sender's IP but on the next port.
//      For example, if a message is received on port 9101, the response is sent to port 9102.



using UnityEngine;
using extOSC;
using System.Collections.Generic;
using System.Xml.Schema;

namespace FX
{
    public class FXEXTOSCManager : MonoBehaviour
    {

        [SerializeField]
        public List<int> listeningPorts = new List<int> { 9101, 9103 };

        private List<OSCReceiver> receivers_;
        private List<OSCTransmitter> transmitters_;

        

        private void Start()
        {
            receivers_ = new List<OSCReceiver>();
            for (int i = 0; i < listeningPorts.Count; i++)
            {
                OSCReceiver receiver = gameObject.AddComponent<OSCReceiver>();
                receiver.LocalPort = listeningPorts[i];
                receiver.Bind("/*", (message) => MessageReceived(message, receiver.LocalPort));

                receivers_.Add(receiver);
            }

            transmitters_ = new List<OSCTransmitter>();

            OSCTransmitter newTransmitter = gameObject.AddComponent<OSCTransmitter>();
            newTransmitter.RemotePort = 10021;
            newTransmitter.RemoteHost = "127.0.0.1";
            transmitters_.Add(newTransmitter);
            FXManager.Instance.onFXParamChanged += OnFXParamChanged;
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
                    string senderIp = message.Ip.ToString();
                    int senderPort = port += 1;

                    bool transmitterFound = false;

                    foreach (OSCTransmitter t in transmitters_)
                    {
                        if (t.RemotePort == senderPort && t.RemoteHost == senderIp)
                        {
                            SendOSCMessage(paramAddress, t, value);
                            transmitterFound = true;
                            break;
                        }
                    }

                    if (!transmitterFound)
                    {
                        OSCTransmitter newTransmitter = gameObject.AddComponent<OSCTransmitter>();
                        newTransmitter.RemotePort = senderPort;
                        newTransmitter.RemoteHost = senderIp;
                        transmitters_.Add(newTransmitter);
                        SendOSCMessage(paramAddress, newTransmitter, value);
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
            SendOSCMessage(address, transmitters_[0], value);
        }

        void SendOSCMessage(string address, OSCTransmitter transmitter, object value)
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
            transmitter.Send(message);
        }


    }
}
