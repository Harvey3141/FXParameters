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


        public enum SignalSource { Default, Pattern, Audio };
        SignalSource signalSource = SignalSource.Default;
        public enum PatternType {Tap, Oscillator, Arpeggiator};
        PatternType patternType= PatternType.Tap;

        public PatternBase pattern;

        public bool presetLoaded = false;

        public void Start()
        {
            this.AddFXElements(address);         
            value.OnValueChanged += SetValue;
        }

        void Update () {
            if (signalSource == SignalSource.Pattern) {
                if (pattern) SetValue(pattern._currentValue);
            }           
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
