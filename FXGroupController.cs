using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

namespace FX
{
    public class FXGroupController : MonoBehaviour, IFXTriggerable
    {
        [SerializeField]
        public FXManager.FXItemInfoType fxType = FXManager.FXItemInfoType.Method;

        [SerializeField]
        public string address; 

        [SerializeField]
        public List<string> fxAddresses;

        [SerializeField]
        public FXParameter<float> value = new FXParameter<float>(0.0f, "", false);

        public bool presetLoaded = false;

        public void Start()
        {
            this.AddFXElements(address);         
            value.OnValueChanged += SetValue;
        }

        public void SetValue(float value)
        {
            foreach (string address in fxAddresses)
            {
                string formattedAddress = address.StartsWith("/") ? address : "/" + address;
                FXManager.Instance.SetFX(formattedAddress, value);
            }
        }

        public void FXTrigger()
        {
            foreach (string address in fxAddresses)
            {
                string formattedAddress = address.StartsWith("/") ? address : "/" + address;
                FXManager.Instance.SetFX(formattedAddress);
            }
        }

        public void ClearFXAdresses() { 
            fxAddresses.Clear();
        }
    }
}
