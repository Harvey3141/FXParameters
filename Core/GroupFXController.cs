using System;
using UnityEngine;
using System.Collections.Generic;
using static FX.FXManager;
using FX.Patterns;
using System.Linq;
using static FX.GroupFXController;

namespace FX
{
    [System.Serializable]
    public class FXGroupData
    {
        public string address = null;
        public string label = null;
        public GroupFXController.SignalSource signalSource = GroupFXController.SignalSource.Default;
        public List<string> fxAddresses = new List<string>();
        public List<string> fxTriggerAddresses = new List<string>();

        public bool isPinned = false;

        public PatternType patternType;
        public int numBeats = 4;
        public FX.Patterns.OscillatorPattern.OscillatorType oscillatorType;
        public FX.Patterns.ArpeggiatorPattern.PatternStyle arpeggiatorStyle;

        public AudioFrequency audioFrequency;
    }
    public class GroupFXController : MonoBehaviour, IFXTriggerable
    {
        [SerializeField]
        public string address;

        private bool active = true;
        public bool Active
        {
            get => active;
            set
            {
                if (active != value) {
                    active = value; 
                }
            }
        }

        public string label;
        public string Label{
            get => label;
            set { 
                if (label != value)
                {
                    label = value;
                    gameObject.name = ($"Group - {label}");
                }
            }
        }

        public bool isPinned = true;

        /// <summary>
        /// Note that these addresses are stored with the leading '/' removed to work with the custom editor list
        /// </summary>
        [SerializeField]
        public List<string> fxAddresses;

        public List<string> FormattedFXAddresses{get {return fxAddresses.Select(address => address.StartsWith("/") ? address : "/" + address).ToList(); }}

        /// <summary>
        /// Note that these addresses are contained with the leading '/' removed to work with the custom editor list
        /// </summary>
        [SerializeField]
        public List<string> fxTriggerAddresses;

        [SerializeField]
        public FXScaledParameter<float> value = new FXScaledParameter<float>(0.0f,0.0f,1.0f, "", false);

        private AudioManager audioManager;
        private bool isAboveAudioThreshold = false;
        public float audioThreshold = 0.8f;

        public event Action OnFXTriggered; //used by GroupFXControllerEditor

        public enum SignalSource { Default, Pattern, Audio };
        [SerializeField]
        public SignalSource signalSource = SignalSource.Default;
        public enum PatternType {None, Tap, Oscillator, Arpeggiator};
        [SerializeField]
        public PatternType patternType= PatternType.None;

        public enum AudioFrequency {Low, Mid, High }
        public AudioFrequency audioFrequency = AudioFrequency.Low;

        public PatternBase pattern;

        public bool presetLoaded = false;


        public void Initialise()
        {
            this.AddFXElements(address);         
            value.OnScaledValueChanged += SetValue;

            if (pattern == null) SetPatternType(patternType);

            audioManager = FindObjectOfType<FX.AudioManager>();   
        }

        void Update () {
            switch (signalSource) { 
                case SignalSource.Default:
                    break;
                case SignalSource.Pattern:
                    if (pattern)
                    {
                        value.Value= pattern._currentValue;
                    }
                    break;
                case SignalSource.Audio:
                    switch (audioFrequency) {
                        case AudioFrequency.Low:
                            value.Value = audioManager.Low;
                            if (!isAboveAudioThreshold && audioManager.Low > audioThreshold)
                            {
                                isAboveAudioThreshold = true;
                                FXTrigger();

                            }
                            else if (isAboveAudioThreshold && audioManager.Low < audioThreshold)
                            {
                                isAboveAudioThreshold = false; 
                            }
                            break;
                        case AudioFrequency.Mid:
                            value.Value = audioManager.Mid;
                            if (!isAboveAudioThreshold && audioManager.Mid > audioThreshold)
                            {
                                isAboveAudioThreshold = true;
                                FXTrigger();

                            }
                            else if (isAboveAudioThreshold && audioManager.Mid < audioThreshold)
                            {
                                isAboveAudioThreshold = false;
                            }
                            break;
                        case AudioFrequency.High:
                            value.Value = audioManager.High;
                            if (!isAboveAudioThreshold && audioManager.High > audioThreshold)
                            {
                                isAboveAudioThreshold = true;
                                FXTrigger();

                            }
                            else if (isAboveAudioThreshold && audioManager.High < audioThreshold)
                            {
                                isAboveAudioThreshold = false;
                            }
                            break;
                    }
                    break;
            }     
        }

        public void SetValue(float value)
        {
            if (!enabled) return;

            if (fxAddresses != null) {
                foreach (string address in fxAddresses)
                {
                    string formattedAddress = address.StartsWith("/") ? address : "/" + address;
                    FXManager.Instance.SetFX(formattedAddress, value);
                }
            }
        }

        public void FXTrigger()
        {
            if (!enabled) return;

            if (fxTriggerAddresses != null) {
                foreach (string address in fxTriggerAddresses)
                {
                    string formattedAddress = address.StartsWith("/") ? address : "/" + address;
                    FXManager.Instance.SetFX(formattedAddress);
                }
            }

            OnFXTriggered?.Invoke();
        }

        public void ClearFXAdresses() 
        { 
            if (fxAddresses!=null) fxAddresses.Clear();
            if (fxTriggerAddresses != null) fxTriggerAddresses.Clear();
        }

        public void AddFXParam(string address)
        {
            if (String.IsNullOrEmpty(address)) return;
            string modifiedAddress = address.Substring(1);
            if (!fxAddresses.Contains(modifiedAddress))
            {
                fxAddresses.Add(modifiedAddress);
            }
        }

        public void RemoveFXParam(string address)
        {
            if (String.IsNullOrEmpty(address)) return;
            string modifiedAddress = address.Substring(1);
            if (fxAddresses.Contains(modifiedAddress))
            {
                fxAddresses.Remove(modifiedAddress);
                FXManager.Instance.ResetParameterToDefault(address);
            }
        }

        public void AddFXTrigger(string address)
        {
            if (String.IsNullOrEmpty(address)) return;
            string modifiedAddress = address.Substring(1);
            if (!fxTriggerAddresses.Contains(modifiedAddress))
            {
                fxTriggerAddresses.Add(modifiedAddress);
            }
        }

        public void RemoveFXTrigger(string address)
        {
            if (String.IsNullOrEmpty(address)) return;
            string modifiedAddress = address.Substring(1);
            if (fxTriggerAddresses.Contains(modifiedAddress))
            {
                fxTriggerAddresses.Remove(modifiedAddress);
            }
        }

        public bool ExistsInFxAddress(string address)
        {
            return !String.IsNullOrEmpty(address) && fxAddresses.Contains(address.Substring(1));
        }

        public void LoadPreset(FXGroupData preset) {
            ClearFXAdresses();
            fxAddresses        = preset.fxAddresses;
            Label              = preset.label;  
            isPinned = preset.isPinned;


            for (int i = 0; i < fxAddresses.Count; i++)
            {
                if (fxAddresses[i].StartsWith('/'))
                {
                    fxAddresses[i] = fxAddresses[i].Substring(1);
                }
            }

            fxTriggerAddresses = preset.fxTriggerAddresses;
            signalSource       = preset.signalSource;

            switch (preset.signalSource)
            {
                case SignalSource.Audio:
                    SetPatternType(PatternType.None);
                    audioFrequency = preset.audioFrequency;
                    break;
                case SignalSource.Pattern:

                    SetPatternType(preset.patternType);
                    pattern.NumBeats = preset.numBeats;

                    switch (patternType)
                    {
                        case PatternType.Oscillator:
                            OscillatorPattern oscillator = (OscillatorPattern)pattern;
                            oscillator.Oscillator = preset.oscillatorType;
                            break;
                        case PatternType.Arpeggiator:
                            ArpeggiatorPattern arp = (ArpeggiatorPattern)pattern;
                            arp.style = preset.arpeggiatorStyle;
                            break;
                    }
                    break;
            }
          
            presetLoaded = true;
        }

        public FXGroupData GetPreset() {

            FXGroupData preset = new FXGroupData();
            preset.address            = address;
            preset.isPinned           = isPinned;
            preset.label              = label;
            preset.fxAddresses        = FormattedFXAddresses;
            preset.fxTriggerAddresses = fxTriggerAddresses;
            preset.signalSource       = signalSource;

            switch (signalSource)
            {
                case SignalSource.Audio:
                    preset.audioFrequency = audioFrequency;
                    break;
                case SignalSource.Pattern:
                    preset.patternType = patternType;
                    preset.numBeats = pattern.NumBeats;

                    switch (patternType)
                    {
                        case PatternType.Oscillator:
                            OscillatorPattern oscillator = (OscillatorPattern)pattern;
                            preset.oscillatorType = oscillator.Oscillator;
                            break;
                        case PatternType.Arpeggiator:
                            ArpeggiatorPattern arp = (ArpeggiatorPattern)pattern;
                            preset.arpeggiatorStyle = arp.style;
                            break;
                    }
                    break;
            }
            return preset;
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

        public void SetOscillatorPatternType (OscillatorPattern.OscillatorType oscillatorType)
        {
            if (signalSource == SignalSource.Pattern && patternType == PatternType.Oscillator) {
                OscillatorPattern oscillator = (OscillatorPattern)pattern;
                oscillator.Oscillator = oscillatorType;
            }
        }

        public void SetArpeggiatorPatternType(ArpeggiatorPattern.PatternStyle patternStyle)
        {
            if (signalSource == SignalSource.Pattern && patternType == PatternType.Arpeggiator)
            {
                ArpeggiatorPattern arp = (ArpeggiatorPattern)pattern;
                arp.style = patternStyle;
            }
        }

        public void SetPatternNumBeats(int value)
        {
            if (signalSource == SignalSource.Pattern)
            {
                pattern.NumBeats = value;
            }
        }

        public string GetLabelBasedOnSignalSource()
        {
            string gameObjectName = this.gameObject.name;
            string label = gameObjectName + " - "; // Prepending the GameObject's name

            switch (signalSource)
            {
                case SignalSource.Default:
                    return label + "Default Signal Source";

                case SignalSource.Pattern:
                    return label + GetPatternLabel();

                case SignalSource.Audio:
                    return label + GetAudioLabel();

                default:
                    return label + "Unknown Signal Source";
            }
        }

        private string GetPatternLabel()
        {
            switch (patternType)
            {
                case PatternType.None:
                    return "Pattern:None";

                case PatternType.Tap:
                    return "Pattern:Tap";

                case PatternType.Oscillator:
                    return "Pattern:Oscillator";

                case PatternType.Arpeggiator:
                    return "Pattern:Arpeggiator";

                default:
                    return "Pattern:Unknown";
            }
        }

        private string GetAudioLabel()
        {
            switch (audioFrequency)
            {
                case AudioFrequency.Low:
                    return "Audio:Low";

                case AudioFrequency.Mid:
                    return "Audio:Mid";

                case AudioFrequency.High:
                    return "Audio:High";

                default:
                    return "Audio:Unknown";
            }
        }


    }
}
