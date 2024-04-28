using FX.Patterns;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static FX.GroupFXController;
using Newtonsoft.Json;


namespace FX
{

    public interface IFXTriggerable
    {
        [FXMethod]
        public void FXTrigger();
    }

    public class FXMethodAttribute : Attribute
    {
        public string Address { get; set; }

        public FXMethodAttribute(string address = null)
        {
            Address = address;
        }
    }

    [SerializeField]
    [AttributeUsage(AttributeTargets.Property)]
    public class FXPropertyAttribute : Attribute
    {
        [SerializeField]
        public string Address { get; set; }

        public FXPropertyAttribute(string address = null)
        {
            Address = address;
        }
    }

    public sealed class FXManager
    {

        private static FXManager _instance;
        public static FXManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FXManager();
                }
                return _instance;
            }
        }

        // Called when an item is added or removed
        public event Action OnFXListChaned;

        public static Dictionary<string, (FXItemInfoType type, object item, object fxInstance)> fxItemsByAddress_ = new Dictionary<string, (FXItemInfoType type, object item, object fxInstance)>(StringComparer.OrdinalIgnoreCase);

        public delegate void OnFXParamValueChanged(string address, object value);
        public event OnFXParamValueChanged onFXParamValueChanged;

        public delegate void OnFXParamAffectorChanged(string address, AffectorFunction affector);
        public event OnFXParamAffectorChanged onFXParamAffectorChanged;

        public delegate void OnPresetLoaded(string name);
        public event OnPresetLoaded onPresetLoaded;

        public delegate void OnFXGroupChanged(FXGroupData data);
        public event OnFXGroupChanged onFXGroupChanged;

        public delegate void OnFXGroupEnabled(string address, bool state);
        public event OnFXGroupEnabled onFXGroupEnabled;

        public delegate void OnFXGroupListChanged(List<string> groupList);
        public event OnFXGroupListChanged onFXGroupListChanged;

        public enum FXItemInfoType
        {
            Method,
            Parameter,
            ScaledParameter
        }

        public void AddFXItem(string address, FXItemInfoType type, object item, object fxInstance)
        {
            if (fxItemsByAddress_.ContainsKey(address))
            {
                Debug.LogError($"An FX item with address {address} is already registered.");
            }
            else
            {
                if (type == FXItemInfoType.Parameter && !(item is IFXParameter))
                {
                    Debug.LogError($"Item with address {address} is not implementing IFXParameter.");
                    return;
                }
                if (item is FXScaledParameter<float> || item is FXScaledParameter<int> || item is FXScaledParameter<Color> || item is FXScaledParameter<Vector3>)
                {
                    type = FXItemInfoType.ScaledParameter;
                }
                fxItemsByAddress_.Add(address, (type, item, fxInstance));

                OnFXListChaned?.Invoke();
            }
        }

        public void RemoveFXItem(string address)
        {
            if (fxItemsByAddress_.ContainsKey(address))
            {
                GroupFXController[] allFXGroups = GameObject.FindObjectsOfType<GroupFXController>();

                foreach (var g in allFXGroups)
                {
                    g.RemoveFXParam(address);
                }

                fxItemsByAddress_.Remove(address);
                OnFXListChaned?.Invoke();
            }
            else {
                Debug.LogError($"An FX item with address {address} is already registered.");
            }
        }

        public bool FXExists(string address)
        {
            if (fxItemsByAddress_.TryGetValue(address, out var fxItem))
            {
                return fxItem.type == FXItemInfoType.Parameter || fxItem.type == FXItemInfoType.ScaledParameter;
            }
            return false;
        }


        public void SetFX(string address)
        {
            SetFX(address, new object[0]);
        }

        public void SetFX(string address, object arg)
        {
            SetFX(address, new object[] { arg });
        }

        public void SetFX(string address, object[] args)
        {
            if (fxItemsByAddress_.TryGetValue(address, out var fxItem))
            {
                switch (fxItem.type)
                {
                    case FXItemInfoType.Method:
                        SetMethod(address, args);
                        break;
                    case FXItemInfoType.Parameter:
                        SetParameter(address, args[0]);
                        break;
                    case FXItemInfoType.ScaledParameter:
                        SetParameter(address, args[0]);
                        break;
                }
            }
            else
            {
                Debug.LogWarning($"No property, method, or trigger found for address {address}");
            }
        }

        public object GetFX(string address)
        {
            if (fxItemsByAddress_.TryGetValue(address, out var fxItem) && (fxItem.type == FXItemInfoType.Parameter || fxItem.type == FXItemInfoType.ScaledParameter))
            {
                IFXParameter parameter = fxItem.item as IFXParameter;

                Type parameterType = parameter.ObjectValue.GetType();

                if (parameterType == typeof(float))
                {
                    return ((FXParameter<float>)parameter).Value;
                }

                //return parameter?.ObjectValue;
            }

            Debug.LogWarning($"FX parameter not found for address {address}");
            return null;
        }

        private void SetMethod(string address, object[] args)
        {
            if (fxItemsByAddress_.TryGetValue(address, out var item))
            {
                if (item.type != FXItemInfoType.Method)
                {
                    Debug.LogWarning($"Item at address {address} is not a method");
                    return;
                }

                var method = (MethodInfo)item.item;
                var instance = item.fxInstance;

                var parameters = method.GetParameters();
                if (parameters.Length != args.Length)
                {
                    Debug.LogWarning($"Method {method.Name} expects {parameters.Length} arguments but {args.Length} were provided");
                    return;
                }

                object[] convertedArgs = new object[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    Type expectedType = parameters[i].ParameterType;
                    object arg = args[i];

                    if (expectedType == typeof(float))
                    {
                        if (arg is float)
                        {
                            convertedArgs[i] = arg;
                        }
                        else if (arg is int)
                        {
                            convertedArgs[i] = (float)(int)arg;
                        }
                        else
                        {
                            Debug.LogWarning($"Argument {i} of method {method.Name} is expected to be float but is {arg.GetType().Name}");
                            return;
                        }
                    }
                    else if (expectedType == typeof(int))
                    {
                        if (arg is int)
                        {
                            convertedArgs[i] = arg;
                        }
                        else if (arg is float)
                        {
                            convertedArgs[i] = (int)(float)arg;
                        }
                        else
                        {
                            Debug.LogWarning($"Argument {i} of method {method.Name} is expected to be int but is {arg.GetType().Name}");
                            return;
                        }
                    }
                    else if (expectedType == typeof(bool))
                    {
                        if (arg is bool)
                        {
                            convertedArgs[i] = arg;
                        }
                        else if (arg is float)
                        {
                            convertedArgs[i] = (int)(float)arg;
                        }
                        else
                        {
                            Debug.LogWarning($"Argument {i} of method {method.Name} is expected to be bool but is {arg.GetType().Name}");
                            return;
                        }
                    }
                    else if (expectedType == typeof(string))
                    {
                        if (arg is string)
                        {
                            convertedArgs[i] = arg;
                        }
                        else
                        {
                            Debug.LogWarning($"Argument {i} of method {method.Name} is expected to be string but is {arg.GetType().Name}");
                            return;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Method {method.Name} has an unsupported argument type: {expectedType}");
                        return;
                    }
                }

                method.Invoke(instance, convertedArgs);
            }
            else
            {
                Debug.LogWarning($"No method found for address {address}");
            }
        }

        private void SetParameter(string address, object arg = null, bool resetToDefault = false)
        {
            if (fxItemsByAddress_.TryGetValue(address, out var fxItem))
            {
                if (fxItem.type != FXItemInfoType.Parameter && fxItem.type != FXItemInfoType.ScaledParameter)
                {
                    Debug.LogWarning($"FX item at address {address} is not a parameter or scaled parameter.");
                    return;
                }

                object parameter = fxItem.item;
                IFXParameter iFXParameter = parameter as IFXParameter;
                if (iFXParameter == null)
                {
                    Debug.LogWarning($"FXParameter {address} is not an instance of IFXParameter");
                    return;
                }

                // TODO - Why was this here? 
                //if (!iFXParameter.ShouldSave) {
                //    Debug.LogError($"FXParameter {address}, should save is set to false therefore param will not be set");
                //    return;
                //}

                if (fxItem.type == FXItemInfoType.ScaledParameter)
                {

                    if (arg is float floatValue)
                    {
                        if (fxItem.item is FXScaledParameter<float> scaledParamFloat)
                        {
                            scaledParamFloat.Value = floatValue;
                        }
                        else if (fxItem.item is FXScaledParameter<Color> scaledParamColor)
                        {
                            scaledParamColor.Value = floatValue;
                        }
                        else if (fxItem.item is FXScaledParameter<int> scaledParamInt)
                        {
                            scaledParamInt.Value = floatValue;
                        }
                        else if (fxItem.item is FXScaledParameter<Vector3> scaledParamVector3)
                        {
                            scaledParamVector3.Value = floatValue;
                        }
                        else
                        {
                            Debug.LogWarning($"FXScaledParameter at address {address} has an unsupported generic type.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Argument for setting ScaledParameter {address} must be a float");
                    }
                }
                else if (fxItem.type == FXItemInfoType.Parameter)
                {
                    Type parameterType = iFXParameter.ObjectValue.GetType();

                    if (parameterType == typeof(float) && arg is float fValueFloat)
                    {
                        ((FXParameter<float>)parameter).Value = fValueFloat;
                    }
                    else if (parameterType == typeof(int))
                    {
                        if (arg is int iValue)
                        {
                            ((FXParameter<int>)parameter).Value = iValue;
                        }
                        else if (arg is float fValueInt)
                        {
                            ((FXParameter<int>)parameter).Value = Mathf.CeilToInt(fValueInt);
                        }
                    }
                    else if (parameterType == typeof(bool))
                    {
                        if (arg is bool bValue)
                        {
                            ((FXParameter<bool>)parameter).Value = bValue;
                        }
                        else if (arg is float fValueBool)
                        {
                            ((FXParameter<bool>)parameter).Value = (fValueBool != 0f);
                        }
                    }
                    else if (parameterType == typeof(string) && arg is string sValue)
                    {
                        ((FXParameter<string>)parameter).Value = sValue;
                    }
                    else if (parameterType == typeof(Color) && arg is Color cValue)
                    {
                        ((FXParameter<Color>)parameter).Value = cValue;
                    }
                    else if (parameterType == typeof(Color) && arg is float fValue)
                    {
                        var param = (FXParameter<Color>)parameter; 
                        var hsbColor = new HSBColor(param.Value);  
                        hsbColor.h = fValue;                       
                        param.Value = hsbColor.ToColor();          
                    }
                    else if (parameterType.IsEnum)
                    {
                        if (arg is float f) {
                            if (f > 0.0f && f < 1.0f)
                            {
                                var enumValues = Enum.GetValues(parameterType);
                                int numValues = enumValues.Length;

                                int enumIndex = (int)(f * (numValues - 1));
                                arg = enumIndex;
                            }
                            else {
                                arg = (int)f;
                            } 
                        }
                        if (arg is int enumInt)
                        {
                            if (Enum.IsDefined(parameterType, enumInt))
                            {
                                object enumValue = Enum.ToObject(parameterType, enumInt);
                                iFXParameter.ObjectValue = enumValue;
                            }
                            else
                            {
                                Debug.LogWarning($"The integer value '{enumInt}' is not defined in the enum '{parameterType.Name}'");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Argument for setting enum parameter {address} is not an int");
                        }
                    }

                    else
                    {
                        Debug.LogWarning($"FXParameter {address} has an unsupported type: {parameterType}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"No parameter found for address {address}");
            }
        }

        public void ResetParameterToDefault(string address)
        {
            if (fxItemsByAddress_.TryGetValue(address, out var fxItem))
            {

                if (fxItem.type == FXItemInfoType.Parameter || fxItem.type == FXItemInfoType.ScaledParameter)
                {
                    IFXParameter parameter = fxItem.item as IFXParameter;
                    parameter?.ResetToDefaultValue();
                }
            }
            else
            {
                Debug.LogWarning($"No parameter found for address {address}.");
            }
        }

        public void ResetAllParamsToDefault()
        {
            foreach (var item in fxItemsByAddress_)
            {
                if (item.Value.type == FXItemInfoType.Parameter || item.Value.type == FXItemInfoType.ScaledParameter)
                {
                    IFXParameter parameter = item.Value.item as IFXParameter;
                    parameter?.ResetToDefaultValue();
                }
            }
        }


        public void OnParameterValueChanged<T>(string address, T value) {
            if (onFXParamValueChanged != null) onFXParamValueChanged.Invoke(address, value);
        }

        public void OnParameterAffertorChanged(string address, AffectorFunction affector)
        {
            if (onFXParamAffectorChanged != null) onFXParamAffectorChanged.Invoke(address, affector);
        }

        public void OnGroupChanged(FXGroupData data)
        {
            if (onFXGroupChanged != null) onFXGroupChanged.Invoke(data);
        }

        [System.Serializable]
        public class FXEnumData
        {
            public List<string> signalTypes       = new List<string>();
            public List<string> patternTypes      = new List<string>();
            public List<string> oscillatorTypes   = new List<string>();
            public List<string> arpeggiatorTypes  = new List<string>();
            public List<string> affectorTypes     = new List<string>();
        }

        private FXEnumData GetFXEnumData() {
            FXEnumData data = new FXEnumData();

            SignalSource s = SignalSource.Default;
            Type tS = s.GetType();

            PatternType p = PatternType.None;
            Type tP = p.GetType();

            OscillatorPattern.OscillatorType o = OscillatorPattern.OscillatorType.Sine;
            Type tO = o.GetType();

            ArpeggiatorPattern.PatternStyle a = ArpeggiatorPattern.PatternStyle.Up;
            Type tA = a.GetType();

            AffectorFunction af = AffectorFunction.Linear;
            Type tAF = af.GetType();

            data.signalTypes      = Enum.GetNames(tS).ToList();
            data.patternTypes     = Enum.GetNames(tP).ToList();
            data.oscillatorTypes  = Enum.GetNames(tO).ToList();
            data.arpeggiatorTypes = Enum.GetNames(tA).ToList();
            data.affectorTypes    = Enum.GetNames(tAF).ToList();
            
            return data;
        }


        [System.Serializable]
        public class FXData
        {
            public List<FXParameterData<string>> stringParameters = new List<FXParameterData<string>>();
            public List<FXParameterData<int>> intParameters       = new List<FXParameterData<int>>();
            public List<FXParameterData<float>> floatParameters   = new List<FXParameterData<float>>();
            public List<FXParameterData<bool>> boolParameters     = new List<FXParameterData<bool>>();
            public List<FXParameterData<Color>> colorParameters   = new List<FXParameterData<Color>>();
            public List<FXEnumParameterData> enumParameters       = new List<FXEnumParameterData>();

            // FXGroups 
            public FXEnumData fXEnumData = new FXEnumData();    
            public List<FXGroupData> fxGroupPresets               = new List<FXGroupData>();
            public List<FXMethodData> fXPresetMethods             = new List<FXMethodData>();
        }

        [System.Serializable]
        public class FXMethodData
        {
            public string key;
        }

        public void SavePreset(string presetName, bool includeAll = false)
        {
            FXData preset = new FXData();

            foreach (var item in fxItemsByAddress_)
            {
                if (item.Value.type == FXItemInfoType.ScaledParameter)
                {
                    var parameter = item.Value.item as IFXParameter;

                    if (parameter.ShouldSave || includeAll)
                    {
                        string key_ = item.Key;
                        object value_ = parameter.ObjectValue;

                        if (item.Value.item is FXScaledParameter<float> scaledParamFloat)
                        {
                            preset.floatParameters.Add(scaledParamFloat.GetData());
                        }
                        else if (item.Value.item is FXScaledParameter<Color> scaledParamColor)
                        {
                            preset.floatParameters.Add(scaledParamColor.GetData());
                        }
                        else if (item.Value.item is FXScaledParameter<int> scaledParamInt)
                        {
                            preset.floatParameters.Add(scaledParamInt.GetData());
                        }
                        else if (item.Value.item is FXScaledParameter<Vector3> scaledParamVector3)
                        {
                            preset.floatParameters.Add(scaledParamVector3.GetData());
                        }
                    }
                }

                else if (item.Value.type == FXItemInfoType.Parameter)
                {
                    var parameter = item.Value.item as IFXParameter;

                    if (parameter.ShouldSave || includeAll)
                    {
                        string key_ = item.Key;
                        object value_ = parameter.ObjectValue;

                        if (parameter is FXParameter<float> floatParam)
                        {
                            preset.floatParameters.Add(floatParam.GetData());
                        }
                        else if (parameter is FXParameter<int> intParam)
                        {
                            preset.intParameters.Add(intParam.GetData());
                        }
                        else if (parameter is FXParameter<string> stringParam)
                        {
                            preset.stringParameters.Add(stringParam.GetData());
                        }
                        else if (parameter is FXParameter<bool> boolParam)
                        {
                            preset.boolParameters.Add(boolParam.GetData());
                        }
                        else if (parameter is FXParameter<Color> colorParam)
                        {
                            preset.colorParameters.Add(colorParam.GetData());
                        }
                        else
                        {
                            Type valueType = value_.GetType();
                            if (valueType.IsEnum)
                            {
                                FXEnumParameterData enumParameter = new FXEnumParameterData
                                {
                                    key = key_,
                                    value = (int)value_,
                                    availableNames = Enum.GetNames(valueType).ToList()
                                };
                                preset.enumParameters.Add(enumParameter);
                            }
                        }
                    }
                }
                if (includeAll) {

                    if (item.Value.type == FXItemInfoType.Method) {
                        preset.fXPresetMethods.Add(new FXMethodData { key = item.Key });
                    }

                }
            }

            preset.fXEnumData = GetFXEnumData();

            GroupFXController[] allFXGroups = GameObject.FindObjectsOfType<GroupFXController>();
            foreach (var group in allFXGroups)
            {
                preset.fxGroupPresets.Add(group.GetData());
            }

            var settings = new JsonSerializerSettings
            {
                Converters = new List <JsonConverter> {
                    new ColourHandler() 
                },               
            };

            string json = JsonConvert.SerializeObject(preset, settings);

            string directoryPath = Path.Combine(Application.streamingAssetsPath, "FX Presets");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, presetName + ".json");
            File.WriteAllText(filePath, json);
        }


        public bool LoadPreset(string presetName)
        {
            string directoryPath = Path.Combine(Application.streamingAssetsPath, "FX Presets"); ;
            string filePath = Path.Combine(directoryPath, presetName + ".json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var settings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> {
                    new ColourHandler()
                },
                };
                FXData preset = JsonConvert.DeserializeObject<FXData>(json,settings);

                Dictionary<string, FXGroupData> fxGroupPresets = preset.fxGroupPresets.ToDictionary(p => p.address, p => p);

                GroupFXController[] allFXGroups = GameObject.FindObjectsOfType<GroupFXController>();

                foreach (var g in allFXGroups)
                {
                    if (!g.isPinned) RemoveGroup(g.address);
                }

                foreach (var groupPreset in fxGroupPresets)
                {
                    foreach (var paramController in groupPreset.Value.fxParameterControllers)
                    {
                        if (!paramController.valueAtZero.HasValue) paramController.valueAtZero = 0.0f;
                        if (!paramController.valueAtOne.HasValue) paramController.valueAtOne = 1.0f;
                    }

                    var existingGroup = allFXGroups.FirstOrDefault(g => g.address == groupPreset.Key);

                    //CleanInvalidFXAddresses(groupPreset.Value.fxAddresses);

                    if (existingGroup != null)
                    {
                        // If it exists, load the preset into the existing group, these will be the pinned groups                     
                        existingGroup.SetData(groupPreset.Value);
                    }
                    else
                    {
                        // If it doesn't exist, create a new GroupFXController with the preset
                        CreateGroup(groupPreset.Value);
                    }
                }

                foreach (var param in preset.stringParameters)
                {
                    SetFX(param.key, param.value);
                }

                foreach (var param in preset.intParameters)
                {
                    SetFX(param.key, param.value);
                }

                foreach (var param in preset.floatParameters)
                {
                    SetFX(param.key, param.value);
                }

                foreach (var param in preset.boolParameters)
                {
                    SetFX(param.key, param.value);
                }

                foreach (var param in preset.colorParameters)
                {
                    SetFX(param.key, param.value);
                }

                foreach (var param in preset.enumParameters)
                {
                    SetFX(param.key, param.value);
                }

                HashSet<string> presetAddresses = new HashSet<string>(preset.boolParameters.Select(p => p.key));

                // Filter and process relevant FX items
                var relevantFXItems = fxItemsByAddress_
                    .Where(item => item.Key.EndsWith("fxEnabled") && item.Value.type == FXItemInfoType.Parameter && item.Value.item is FXParameter<bool>)
                    .ToList();

                // TODO - change this to set them to their default values
                // Set FXParameter<bool> items to false if not included in the preset
                foreach (var fxItem in relevantFXItems)
                {
                    IFXParameter parameter = fxItem.Value.item as IFXParameter;
                    if (parameter != null && parameter.ShouldSave && !presetAddresses.Contains(fxItem.Key))
                    {
                        ((FXParameter<bool>)parameter).Value = false;
                    }
                }
                if (onPresetLoaded != null) onPresetLoaded.Invoke(presetName);
                return true;

            }
            else
            {
                Debug.LogWarning($"Preset {presetName} not found.");
                return false;
            }
        }

        public void CleanInvalidFXAddresses(List<string> fxAddresses)
        {
            fxAddresses.RemoveAll(fxAddress => !fxItemsByAddress_.ContainsKey(fxAddress));
        }

        private bool IsAddressInPreset(string address, FXData preset)
        {
            return preset.boolParameters.Any(p => p.key == address);
        }


        public void AddFXParamToGroup(string groupAddress, string fxAddress)
        {
            GroupFXController group = FindGroupByAddress(groupAddress);
            if (group != null)
            {
                group.AddFXParam(fxAddress);
            }
            else
            {
                Debug.LogWarning($"Group with address {groupAddress} not found.");
            }
        }

        public void RemoveFXParamFromGroup(string groupAddress, string fxAddress)
        {
            GroupFXController group = FindGroupByAddress(groupAddress);
            if (group != null)
            {
                group.RemoveFXParam(fxAddress);
            }
            else
            {
                Debug.LogWarning($"Group with address {groupAddress} not found.");
            }
        }

        public FXParameterControllerData GetGroupFXParamData(string groupAddress, string fxAddress)
        {
            GroupFXController group = FindGroupByAddress(groupAddress);
            if (group != null)
            {
                return group.GetParameterController(fxAddress);
            }
            else
            {
                Debug.LogWarning($"Group with address {groupAddress} not found.");
                return null;
            }
        }

        public void SetGroupFXParam(string groupAddress, string fxAddress, FXParameterControllerData param)
        {
            GroupFXController group = FindGroupByAddress(groupAddress);
            if (group != null)
            {
                group.SetParameterController(param);
            }
            else
            {
                Debug.LogWarning($"Group with address {groupAddress} not found.");
            }
        }

        public void AddFXTriggerToGroup(string groupAddress, string fxAddress)
        {
            GroupFXController group = FindGroupByAddress(groupAddress);
            if (group != null)
            {
                group.AddFXTrigger(fxAddress);
            }
            else
            {
                Debug.LogWarning($"Group with address {groupAddress} not found.");
            }
        }

        public void RemoveFXTriggerFromGroup(string groupAddress, string fxAddress)
        {
            GroupFXController group = FindGroupByAddress(groupAddress);
            if (group != null)
            {
                group.RemoveFXTrigger(fxAddress);
            }
            else
            {
                Debug.LogWarning($"Group with address {groupAddress} not found.");
            }
        }
        public GroupFXController CreateGroup()
        {
            int maxIndex = 0;
            GroupFXController[] allGroups = GameObject.FindObjectsOfType<GroupFXController>();
            foreach (var g in allGroups)
            {
                string groupPrefix = "/group/";
                string groupIndexS = g.address.Substring(groupPrefix.Length);
                bool ok = Int32.TryParse(groupIndexS, out int index);
                if (ok) {
                    if (index > maxIndex) maxIndex = index;
                }
            }

            string address = "/Group/" + (maxIndex + 1);
            GameObject groupObject = new GameObject("Group - ");
            GroupFXController group = groupObject.AddComponent<GroupFXController>();

            GameObject parent = GameObject.FindObjectOfType<FXSceneManager>().gameObject;
            if (parent != null) {
                groupObject.transform.parent = parent.transform;
            }

            group.address = address;
            group.Initialise();
            return group;
        }

        public GroupFXController CreateGroup(FXGroupData data) {
            GroupFXController group = CreateGroup();
            group.SetData(data);
            return group;
        }

        public bool SetGroup(FXGroupData data)
        {
            GroupFXController group = FindGroupByAddress(data.address);
            if (group != null)
            {
                group.SetData(data);
                return true;
            }
            else { 
                return false; 
            }
        }

        public void RemoveGroup(string address)
        {
            GroupFXController group = FindGroupByAddress(address);
            if (group != null)
            {
                if (!group.isPinned) {
                    GameObject.Destroy(group.gameObject);
                } 
                else Debug.LogWarning($"Group with address {address} is pinned so cannot be removed");
            }
            else
            {
                Debug.LogWarning($"Group with address {address} not found and cannot be removed.");
            }
        }

        public List<string> GetGroupList() {

            List<string> list = new List<string>();
            GroupFXController[] allGroups = GameObject.FindObjectsOfType<GroupFXController>();
            foreach (var group in allGroups)
            {
                list.Add(group.address);
            }
            return list;
        }

        public void ClearGroup(string address)
        {
            GroupFXController group = FindGroupByAddress(address);
            if (group != null)
            {
                group.ClearFXAdresses();
            }
        }

        public void ClearAllGroups()
        {
            GroupFXController[] allGroups = GameObject.FindObjectsOfType<GroupFXController>();
            foreach (var group in allGroups)
            {
                group.ClearFXAdresses();
            }
        }

        public GroupFXController FindGroupByAddress(string address)
        {
            GroupFXController[] allGroups = GameObject.FindObjectsOfType<GroupFXController>();
            foreach (var group in allGroups)
            {
                if (group.address == address)
                {
                    return group;
                }
            }
            return null; 
        }

        public void OnGroupListChanged() { 
            if(onFXGroupListChanged != null) onFXGroupListChanged(GetGroupList()); 
        }

        public void OnGroupEnabled(string address, bool state)
        {
            if (onFXGroupEnabled != null) onFXGroupEnabled(address,state);
        }
    }

}


