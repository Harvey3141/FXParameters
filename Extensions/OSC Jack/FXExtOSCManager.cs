using UnityEngine;
using extOSC;
using System.Collections.Generic;
using System;

namespace FX
{
    public class FXExtOSCManager : MonoBehaviour
    {
        [SerializeField]
        private int listeningPort_ = 9101;

        private OSCReceiver receiver_;

        private List<OSCTransmitter> transmitters_;

        private void Start()
        {
            receiver_ = gameObject.AddComponent<OSCReceiver>();
            receiver_.LocalPort = 9101;
            receiver_.Bind("/*", MessageReceived);

            transmitters_ = new List<OSCTransmitter>();
        }

        protected void MessageReceived(OSCMessage message)
        {
            string address = message.Address;
            if (address.ToUpper().EndsWith("/GET"))
            {
                string paramAddress = address.Substring(0, address.Length - 4);
                object value = FXManager.Instance.GetFX(paramAddress);

                if (value != null)
                {
                    string senderIp = message.Ip.ToString();
                    int senderPort = 9102;

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
