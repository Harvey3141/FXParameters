using System;
using UnityEngine;
using System.Collections.Generic;
using static FX.FXManager;
using FX.Patterns;
using System.Linq;
using static FX.GroupFXController;
using System.Data.Common;
using Newtonsoft.Json;


namespace FX
{
    [System.Serializable]
    public class FXParameterControllerData
    {
        public string key;
        public AffectorFunction? affectorType;
        public bool? invert;
        public bool? enabled;
        public float? valueAtZero;  
        public float? valueAtOne;   
    }

    [System.Serializable]
    public class FXParameterController {
        public string key; // with leading '/' removed to work with editor script popup list
        public string FxAddress { 
            get { 
                return "/" + key; 
            }
        }

        public AffectorFunction affectorType = AffectorFunction.Randomise;
        public bool invert = false;
        public bool enabled = true;
        public float valueAtZero = 0f;
        public float valueAtOne  = 1f;


        public FXParameterController(string address, AffectorFunction affector, bool invert, bool enabled = true, float valueAtZero = 0f, float valueAtOne = 1f) {
            key = address.Substring(1);
            this.enabled      = enabled;
            this.affectorType = affector;
            this.invert       = invert;
            this.valueAtZero  = valueAtZero;
            this.valueAtOne   = valueAtOne;
        }

        public FXParameterController(FXParameterControllerData data)
        {
            key          = data.key.Substring(1);
            enabled      = data.enabled ?? true;  
            affectorType = data.affectorType ?? AffectorFunction.Linear;  
            invert       = data.invert ?? false; 
            valueAtZero  = data.valueAtZero ?? 0f; 
            valueAtOne   = data.valueAtOne ?? 1f; 
        }

        public void SetData(FXParameterControllerData data)
        {
            if (!string.IsNullOrEmpty(data.key))
            {
                key = data.key.Substring(1);
            }

            if (data.enabled.HasValue)
            {
                enabled = data.enabled.Value;
            }

            if (data.affectorType.HasValue)
            {
                affectorType = data.affectorType.Value;
            }

            if (data.invert.HasValue)
            {
                invert = data.invert.Value;
            }

            if (data.valueAtZero.HasValue)
            {
                valueAtZero = data.valueAtZero.Value;
            }

            if (data.valueAtOne.HasValue)
            {
                valueAtOne = data.valueAtOne.Value;
            }
        }

        public FXParameterControllerData GetData() {
            FXParameterControllerData data = new FXParameterControllerData();
            data.key          = FxAddress; 
            data.affectorType = affectorType;
            data.invert       = invert;
            data.enabled      = enabled;
            data.valueAtZero  = valueAtZero;
            data.valueAtOne   = valueAtOne;
            return data;
        }

        public float GetAffectedValue(float valueIn) {
            float remapped = Mathf.Lerp(valueAtZero, valueAtOne, valueIn);  
            float affectedValue = (invert ? (1.0f - Mathf.Clamp01(remapped)) : Mathf.Clamp01(remapped));

            switch (affectorType)
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
    }

    [System.Serializable]
    public class FXGroupData
    {
        public bool isEnabled = true;
        public string address = null;
        public string label = null;
        public bool isPinned = false;
        public List<FXParameterControllerData> fxParameterControllers = new List<FXParameterControllerData>();
        public List<string> fxTriggerAddresses = new List<string>();

        public GroupFXController.SignalSource signalSource = GroupFXController.SignalSource.Default;
        public float valueAtZero = 0.0f;
        public float valueAtOne = 1.0f;

        public AudioFrequency audioFrequency;

        public PatternType patternType;
        public float numBeats = 4;
        public FX.Patterns.OscillatorPattern.OscillatorType oscillatorType;
        public FX.Patterns.ArpeggiatorPattern.PatternStyle arpeggiatorStyle;
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
                    fxManager.OnGroupEnabled(address, value);
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

        private FXManager fxManager;

        private FXGroupData lastLoadedState = null;

        public void Initialise()
        {
            fxManager = FXManager.Instance;
            lastLoadedState = null;
            this.AddFXElements(address);         
            value.OnScaledValueChanged += SetValue;

            if (pattern == null) SetPatternType(patternType);

            audioManager = FindObjectOfType<FX.AudioManager>();

            fxManager.OnGroupListChanged();
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

        public void SetValue(float v)
        {
            if (!active) return;

            if (fxParameterControllers != null)
            {
                foreach (var a in fxParameterControllers)
                {
                    if (a.enabled) {
                        fxManager.SetFX(a.FxAddress, a.GetAffectedValue(value.ScaledValue), false);
                    }                   
                }
            }
        }

        public void FXTrigger()
        {
            if (!active) return;

            if (fxTriggerAddresses != null) {
                foreach (string address in fxTriggerAddresses)
                {
                    string formattedAddress = address.StartsWith("/") ? address : "/" + address;
                    fxManager.SetFX(formattedAddress, false);
                }
            }

            OnFXTriggered?.Invoke();
        }

        public void ClearFXAdresses() 
        {
            if (fxParameterControllers != null) {
                foreach (var a in fxParameterControllers)
                {
                    fxManager.ResetParameterToSceneDefault(a.FxAddress);
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
                FXParameterController p = new FXParameterController(address, AffectorFunction.Linear, false, true, 0f, 1f);
                fxParameterControllers.Add(p);
                OnGroupChanged();
            }
        }

        public void RemoveFXParam(string address)
        {
            if (String.IsNullOrEmpty(address)) return;

            var itemToRemove = fxParameterControllers.FirstOrDefault(a => a.FxAddress == address);

            if (itemToRemove != null)
            {
                fxManager.ResetParameterToSceneDefault(itemToRemove.FxAddress);
                fxParameterControllers.Remove(itemToRemove);
                OnGroupChanged();
            }
        }

        public void AddFXTrigger(string address)
        {
            if (String.IsNullOrEmpty(address)) return;
            string modifiedAddress = address.Substring(1);
            if (!fxTriggerAddresses.Contains(modifiedAddress))
            {
                fxTriggerAddresses.Add(modifiedAddress);
                OnGroupChanged();
            }
        }

        public void RemoveFXTrigger(string address)
        {
            if (String.IsNullOrEmpty(address)) return;
            string modifiedAddress = address.Substring(1);
            if (fxTriggerAddresses.Contains(modifiedAddress))
            {
                fxTriggerAddresses.Remove(modifiedAddress);
                OnGroupChanged();
            }
        }

        public bool ExistsInFxAddress(string address)
        {
            var item = fxParameterControllers.FirstOrDefault(a => a.FxAddress == address);
            return item != null;
        }

        public void SetData(FXGroupData data) {
            ClearFXAdresses();

            fxParameterControllers.AddRange(data.fxParameterControllers.Select(p => new FXParameterController(p)));

            fxTriggerAddresses = data.fxTriggerAddresses.Select(address => address.StartsWith("/") ? address.Substring(1) : address).ToList();

            Label = data.label;  
            isPinned = data.isPinned;

            value.ValueAtZero = data.valueAtZero; 
            value.ValueAtOne = data.valueAtOne;

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
          
            presetLoaded    = true;
            Active          = data.isEnabled;
            lastLoadedState = data;
        }


        public FXGroupData GetData() {

            FXGroupData data            = new FXGroupData();
            data.isEnabled              = active;
            data.address                = address;
            data.isPinned               = isPinned;
            data.label                  = label;
            data.fxParameterControllers = fxParameterControllers.Select(controller => controller.GetData()).ToList();
            data.fxTriggerAddresses     = FormattedFXTriggerAddresses;
            data.valueAtZero            = value.ValueAtZero;
            data.valueAtOne             = value.ValueAtOne;
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


        public void ResetGroupToLastLoadedState()
        {
            if (lastLoadedState != null)
            {
                SetData(lastLoadedState);
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
            OnGroupChanged();

        }

        public void SetOscillatorPatternType (OscillatorPattern.OscillatorType oscillatorType)
        {
            if (signalSource == SignalSource.Pattern && patternType == PatternType.Oscillator) {
                OscillatorPattern oscillator = (OscillatorPattern)pattern;
                oscillator.Oscillator = oscillatorType;
                OnGroupChanged();
            }
        }

        public void SetArpeggiatorPatternType(ArpeggiatorPattern.PatternStyle patternStyle)
        {
            if (signalSource == SignalSource.Pattern && patternType == PatternType.Arpeggiator)
            {
                ArpeggiatorPattern arp = (ArpeggiatorPattern)pattern;
                arp.style = patternStyle;
                OnGroupChanged();
            }
        }

        public void SetPatternNumBeats(int value)
        {
            if (signalSource == SignalSource.Pattern)
            {
                pattern.NumBeats = value;

                switch (patternType)
                {
                    case PatternType.Tap:
                        TapPattern tap = (TapPattern)pattern;
                        tap.ClearTriggers();
                        tap.AddTriggers(1);
                        break;
                }
                OnGroupChanged();
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

        public void OnGroupChanged () {
            fxManager.OnGroupChanged(GetData());
        }

        public FXParameterControllerData GetParameterController(string fxAddress) {

            var item = fxParameterControllers.FirstOrDefault(a => a.FxAddress.Equals(fxAddress, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                return item.GetData();
            }
            else return null;
        }

        public void SetParameterController(FXParameterControllerData data)
        {
            var controller = fxParameterControllers.FirstOrDefault(c => c.FxAddress.Equals(data.key, StringComparison.OrdinalIgnoreCase));

            if (controller != null)
            {
                controller.SetData(data);
                return;
            }

            if (fxManager.FXExists(data.key))
            {
                AddFXParam(data.key);
                controller = fxParameterControllers.FirstOrDefault(c => c.FxAddress.Equals(data.key, StringComparison.OrdinalIgnoreCase));
                if (controller != null)
                {
                    controller.SetData(data);
                }
                return;
            }

            Debug.LogWarning($"Param with address {data.key} not found.");
        }


        public void SetParameterEnabled(string address, bool enabled)
        {

            var item = fxParameterControllers.FirstOrDefault(a => a.FxAddress.Equals(address, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.enabled = enabled;
                OnGroupChanged();
            }
        }

        public void SetParameterInvert(string address, bool invert)
        {

            var item = fxParameterControllers.FirstOrDefault(a => a.FxAddress.Equals(address, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.invert = invert;
                OnGroupChanged();
            }
        }

        public void SetParameterAffectorType(string address, AffectorFunction affectorType)
        {

            var item = fxParameterControllers.FirstOrDefault(a => a.FxAddress.Equals(address, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.affectorType = affectorType;
                OnGroupChanged();
            }
        }


        void OnDestroy()
        {
            ClearFXAdresses();
            fxManager.OnGroupListChanged();
        }

    }
}
