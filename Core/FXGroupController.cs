using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using static FX.FXManager;

namespace FX
{
    public class FXGroupController : MonoBehaviour, IFXTriggerable
    {
        [SerializeField]
        public string address; 

        [SerializeField]
        public List<string> fxAddresses;

        [SerializeField]
        public List<string> fxTriggerAddresses;

        [SerializeField]
        public FXParameter<float> value = new FXParameter<float>(0.0f, "", false);


        public enum SignalSource { Default, Pattern, Audio };
        [SerializeField]
        public SignalSource signalSource = SignalSource.Default;
        public enum PatternType {None, Tap, Oscillator, Arpeggiator};
        [SerializeField]
        public PatternType patternType= PatternType.None;

        public PatternBase pattern;

        public bool presetLoaded = false;


        public void Start()
        {
            this.AddFXElements(address);         
            value.OnValueChanged += SetValue;

            if (signalSource == SignalSource.Pattern) SetPatternType(patternType);
        }

        void Update () {
            if (signalSource == SignalSource.Pattern) {
                if (pattern) {
                    SetValue(pattern._currentValue);
                }              
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
            foreach (string address in fxTriggerAddresses)
            {
                string formattedAddress = address.StartsWith("/") ? address : "/" + address;
                FXManager.Instance.SetFX(formattedAddress);
            }
        }

        public void ClearFXAdresses() { 
            fxAddresses.Clear();
            fxTriggerAddresses.Clear();
        }

        public void LoadPreset(FXGroupPreset preset) {
            fxAddresses        = preset.fxAddresses;
            fxTriggerAddresses = preset.fxTriggerAddresses;
            SetPatternType(preset.patternType);
            presetLoaded = true;
        }

        public void SetPatternType(PatternType newPatternType)
        {
            patternType = newPatternType;

            if (pattern != null)
            {
                DestroyImmediate(pattern);
            }

            switch (patternType)
            {
                case PatternType.None:                   
                    break;
                case PatternType.Tap:
                    pattern = gameObject.AddComponent<TapPattern>();
                    pattern.OnTrigger += FXTrigger;
                    break;
                case PatternType.Oscillator:
                    pattern = gameObject.AddComponent<OscillatorPattern>();
                    pattern.OnTrigger += FXTrigger;
                    break;
                case PatternType.Arpeggiator:
                    pattern = gameObject.AddComponent<ArpeggiatorPattern>();
                    pattern.OnTrigger += FXTrigger;
                    break;
                default:
                    Debug.LogError("Invalid pattern type!");
                    break;
            }
        }




    }
}
