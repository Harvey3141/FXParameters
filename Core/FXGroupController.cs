using UnityEngine;
using System.Collections.Generic;
using static FX.FXManager;
using System;
using FX.Patterns;
using System.Linq;

namespace FX
{
    public class FXGroupController : MonoBehaviour, IFXTriggerable
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
        public FXParameter<float> value = new FXParameter<float>(0.0f, "", false);

        private AudioManager audioManager;
        private bool isAboveAudioThreshold = false;
        public float audioThreshold = 0.8f;

        public event Action OnFXTriggered; //used by FXGroupControllerEditor

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
            value.OnValueChanged += SetValue;

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

        public void ClearFXAdresses() { 
            fxAddresses.Clear();
            fxTriggerAddresses.Clear();
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
            audioFrequency     = preset.audioFrequency;

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
