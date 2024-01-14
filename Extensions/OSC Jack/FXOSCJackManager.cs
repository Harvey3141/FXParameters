// Use script only when the OSCJack library is included in your Unity project:
// https://github.com/keijiro/OscJack
// 1. Go to Unity Editor, and click on "Edit" in the top menu.
// 2. Navigate to "Project Settings" -> "Player".
// 3. In the Player settings, locate the "Scripting Define Symbols" section.
// 4. Add a new symbol named "OSCJACK" (without quotes) in the text box.
// 5. After typing the symbol, press Enter and then click on "Apply" to save the changes.


#if OSCJACK
using UnityEngine;
using OscJack;
using System.Collections.Generic;
using System;

namespace FX
{
    public class FXOSCJackManager : MonoBehaviour
    {
        [SerializeField]
        private int listeningPort_ = 9101; 
        private OscServer server_;

        public List<OscClient> clients_;

        private void Start()
        {
            server_ = new OscServer(listeningPort_);
            server_.MessageDispatcher.AddCallback(
                "", 
                (string address, OscDataHandle data) =>
                {
                    if (address.ToUpper().EndsWith("/GET"))
                    {
                        string paramAddress = address.Substring(0, address.Length - 4);
                        object value = FXManager.Instance.GetFX(paramAddress);
                         
                        if (value != null)
                        {
                            //SendOSCMessage(address, OscClient(), value);
                        }
                    }
                    else
                    {
                        object[] args = new object[data.GetElementCount()];

                        for (int i = 0; i < args.Length; i++)
                        {
                            args[i] = data.GetElementAsFloat(i);
                        }

                        FXManager.Instance.SetFX(address, args);
                    }

                }
            );
        }

        private void SendOSCMessage(string address, OscClient client, object value)
        {
            switch (value)
            {
                case float floatValue:
                    client.Send(address, floatValue);
                    break;
                case int intValue:
                    client.Send(address, intValue);
                    break;
                case string stringValue:
                    client.Send(address, stringValue);
                    break;
                default:
                    Debug.LogWarning($"Unsupported value type for OSC message: {value.GetType()}");
                    break;
            }
        }


        private void OnDestroy()
        {
            if (server_ != null)
            {
                server_.Dispose();
            }
        }
    }
}
#endif