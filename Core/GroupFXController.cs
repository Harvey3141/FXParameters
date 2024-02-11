using UnityEngine;
using System.Collections.Generic;
using static FX.FXManager;
using System;
using FX.Patterns;
using System.Linq;

namespace FX
{
    public class GroupFXController : MonoBehaviour, IFXTriggerable
    {
        [SerializeField]
        public string address; 

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


        public void Start()
        {
            this.AddFXElements(address);         
            value.OnScaledValueChanged += SetValue;

            SetPatternType(patternType);

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
            OnFXTriggered?.Invoke();
        }

        public void ClearFXAdresses() 
        { 
            fxAddresses.Clear();
            fxTriggerAddresses.Clear();
        }

        public void AddFxAddress(string address)
        {
            if (String.IsNullOrEmpty(address)) return;
            string modifiedAddress = address.Substring(1);
            if (!fxAddresses.Contains(modifiedAddress))
            {
                fxAddresses.Add(modifiedAddress);
            }
        }

        public void RemoveFxAddress(string address)
        {
            if (String.IsNullOrEmpty(address)) return;
            string modifiedAddress = address.Substring(1);
            if (fxAddresses.Contains(modifiedAddress))
            {
                fxAddresses.Remove(modifiedAddress);
            }
        }

        public bool ExistsInFxAddress(string address)
        {
            return !String.IsNullOrEmpty(address) && fxAddresses.Contains(address.Substring(1));
        }

        public void LoadPreset(FXGroupPreset preset) {
            ClearFXAdresses();
            fxAddresses        = preset.fxAddresses;

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
                    }
                    break;
            }
          
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
