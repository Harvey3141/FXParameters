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
    public class FXParameterController {
        public string key; // with leading '/' removed to work with editor script popup list
        public string FxAddress { 
            get { 
                return "/" + key; 
            }
        }

        public AffectorFunction affector = AffectorFunction.Linear;
        public bool invert = false;


        public FXParameterController(string address, AffectorFunction affector, bool invert) {
            key = address.Substring(1);
            this.affector = affector;
            this.invert = invert;
        }
       
        public float GetAffectedValue(float valueIn) {
            float affectedValue = (invert ? (1.0f - Mathf.Clamp01(valueIn)) : Mathf.Clamp01(valueIn));

            switch (affector)
            {
                case AffectorFunction.Linear:
                    break;
                case AffectorFunction.EaseIn:
                    affectedValue = Mathf.Pow(affectedValue, 2);
                    break;
                case AffectorFunction.EaseOut:
                    affectedValue = Mathf.Sqrt(affectedValue);
                    break;
                case AffectorFunction.Randomise:
                    affectedValue = UnityEngine.Random.Range(0f, 1f);
                    break;
            }
            return affectedValue;
        }

        public void SetFXValue(float value) {
            FXManager.Instance.SetFX(FxAddress, GetAffectedValue(value));
        }

        public void SetDefaultFXValue() {
            FXManager.Instance.ResetParameterToDefault(FxAddress);
        }
    }

    [System.Serializable]
    public class FXGroupData
    {
        public string address = null;
        public string label = null;
        public GroupFXController.SignalSource signalSource = GroupFXController.SignalSource.Default;
        public List<FXParameterController> fxParameterControllers = new List<FXParameterController>();
        public List<string> fxTriggerAddresses = new List<string>();

        public bool isPinned = false;

        public PatternType patternType;
        public float numBeats = 4;
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
        /// Note that the key containing the fxAddresses are stored with the leading '/' removed to work with the custom editor list
        /// </summary>
        /// [SerializeField]
        public List<FXParameterController> fxParameterControllers = new List<FXParameterController>();

        public List<FXParameterController> FormattedFxParameterControllers
        {
            get
            {
                return fxParameterControllers.Select(item =>
                {
                    var clonedItem = new FXParameterController(item.FxAddress, item.affector, item.invert)
                    {
                        // Ensure key has a leading '/'
                        key = item.key.StartsWith("/") ? item.key : "/" + item.key
                    };
                    return clonedItem;
                }).ToList();
            }
        }


        /// <summary>
        /// Note that these addresses are contained with the leading '/' removed to work with the custom editor list
        /// </summary>
        [SerializeField]
        public List<string> fxTriggerAddresses;

        public List<string> FormattedFXTriggerAddresses { get { return fxTriggerAddresses.Select(address => address.StartsWith("/") ? address : "/" + address).ToList(); } }


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

            FXManager.Instance.OnGroupListChanged();
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

            if (fxParameterControllers != null)
            {
                foreach (var a in fxParameterControllers)
                {
                    a.SetFXValue(value);
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
            if (fxParameterControllers != null) {
                foreach (var a in fxParameterControllers)
                {
                    a.SetDefaultFXValue();
                }
                fxParameterControllers.Clear();
            }
            if (fxTriggerAddresses != null) {
                foreach (string address in FormattedFXTriggerAddresses)
                {
                    RemoveFXTrigger(address);
                }
                fxTriggerAddresses.Clear();

            } 
        }

        public void AddFXParam(string address)
        {
            if (String.IsNullOrEmpty(address)) return;


            if (!fxParameterControllers.Any(a => a.FxAddress == address))
            {
                fxParameterControllers.Add(new FXParameterController(address, AffectorFunction.Linear, false));
            }
        }

        public void RemoveFXParam(string address)
        {
            if (String.IsNullOrEmpty(address)) return;

            var itemToRemove = fxParameterControllers.FirstOrDefault(a => a.FxAddress == address);

            if (itemToRemove != null)
            {
                itemToRemove.SetDefaultFXValue();
                fxParameterControllers.Remove(itemToRemove);
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
            var item = fxParameterControllers.FirstOrDefault(a => a.FxAddress == address);
            return item != null;
        }

        public void SetData(FXGroupData data) {
            ClearFXAdresses();

            fxParameterControllers = data.fxParameterControllers;

            fxParameterControllers = data.fxParameterControllers.Select(item =>
            {
                // Modify fxAddress to ensure it does not start with '/'
                item.key = item.key.StartsWith("/") ? item.key.Substring(1) : item.key;
                return item;
            }).ToList();


            fxTriggerAddresses = data.fxTriggerAddresses.Select(address => address.StartsWith("/") ? address.Substring(1) : address).ToList();

            Label = data.label;  
            isPinned = data.isPinned;


            signalSource       = data.signalSource;

            switch (data.signalSource)
            {
                case SignalSource.Audio:
                    SetPatternType(PatternType.None);
                    audioFrequency = data.audioFrequency;
                    break;
                case SignalSource.Pattern:

                    SetPatternType(data.patternType);
                    pattern.NumBeats = data.numBeats;

                    switch (patternType)
                    {
                        case PatternType.Oscillator:
                            OscillatorPattern oscillator = (OscillatorPattern)pattern;
                            oscillator.Oscillator = data.oscillatorType;
                            break;
                        case PatternType.Arpeggiator:
                            ArpeggiatorPattern arp = (ArpeggiatorPattern)pattern;
                            arp.style = data.arpeggiatorStyle;
                            break;
                    }
                    break;
            }
          
            presetLoaded = true;
        }

        public FXGroupData GetData() {

            FXGroupData data = new FXGroupData();
            data.address                = address;
            data.isPinned               = isPinned;
            data.label                  = label;
            data.fxParameterControllers = FormattedFxParameterControllers;
            data.fxTriggerAddresses     = FormattedFXTriggerAddresses;
            data.signalSource           = signalSource;

            switch (signalSource)
            {
                case SignalSource.Audio:
                    data.audioFrequency = audioFrequency;
                    break;
                case SignalSource.Pattern:
                    data.patternType = patternType;
                    data.numBeats = pattern.NumBeats;

                    switch (patternType)
                    {
                        case PatternType.Oscillator:
                            OscillatorPattern oscillator = (OscillatorPattern)pattern;
                            data.oscillatorType = oscillator.Oscillator;
                            break;
                        case PatternType.Arpeggiator:
                            ArpeggiatorPattern arp = (ArpeggiatorPattern)pattern;
                            data.arpeggiatorStyle = arp.style;
                            break;
                    }
                    break;
            }
            return data;
        }

        public void SetAffectorType(string address, AffectorFunction affectorType)
        {

            var item = fxParameterControllers.FirstOrDefault(a => a.FxAddress.Equals(address, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.affector = affectorType;
            }
        }


        public void SetAffectorInvert(string address, bool invert)
        {

            var item = fxParameterControllers.FirstOrDefault(a => a.FxAddress.Equals(address, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.invert = invert;
            }
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

        void OnGroupModified () { 

        }

        void OnDestroy()
        {
            ClearFXAdresses();
            FXManager.Instance.OnGroupListChanged();
        }

    }
}
