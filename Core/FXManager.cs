using FX.Patterns;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static FX.GroupFXController;

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

        public event Action OnFXItemAdded;

        public static Dictionary<string, (FXItemInfoType type, object item, object fxInstance)> fxItemsByAddress_ = new Dictionary<string, (FXItemInfoType type, object item, object fxInstance)>(StringComparer.OrdinalIgnoreCase);

        public delegate void OnFXParamValueChanged(string address, object value);
        public event OnFXParamValueChanged onFXParamValueChanged;

        public delegate void OnFXParamAffectorChanged(string address, AffectorFunction affector);
        public event OnFXParamAffectorChanged onFXParamAffectorChanged;

        public delegate void OnPresetLoaded(string name);
        public event OnPresetLoaded onPresetLoaded;

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

                OnFXItemAdded?.Invoke();
            }
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
                    else if (parameterType.IsEnum)
                    {
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
                if (fxItem.type == FXItemInfoType.ScaledParameter)
                {
                    switch (fxItem.item)
                    {
                        case FXScaledParameter<float> floatParam:
                            floatParam.ResetToDefaultValue();
                            break;
                        case FXScaledParameter<int> intParam:
                            intParam.ResetToDefaultValue();
                            break;
                        case FXScaledParameter<bool> boolParam:
                            boolParam.ResetToDefaultValue();
                            break;
                        case FXScaledParameter<Enum> enumParam:
                            enumParam.ResetToDefaultValue();
                            break;
                        case FXScaledParameter<Color> colorParam:
                            colorParam.ResetToDefaultValue();
                            break;
                        case FXScaledParameter<String> stringParam:
                            stringParam.ResetToDefaultValue();
                            break;
                    }
                }
                else if (fxItem.type == FXItemInfoType.Parameter)
                {
                    switch (fxItem.item)
                    {
                        case FXParameter<float> floatParam:
                            floatParam.ResetToDefaultValue();
                            break;
                        case FXParameter<int> intParam:
                            intParam.ResetToDefaultValue();
                            break;
                        case FXParameter<bool> boolParam:
                            boolParam.ResetToDefaultValue();
                            break;
                        case FXParameter<Enum> enumParam:
                            enumParam.ResetToDefaultValue();
                            break;
                        case FXParameter<Color> colorParam:
                            colorParam.ResetToDefaultValue();
                            break;
                        case FXParameter<String> stringParam:
                            stringParam.ResetToDefaultValue();
                            break;
                    }
                }
                else
                {
                    Debug.LogWarning($"No parameter found for address {address}.");
                }
            }
        }

        public void ResetAllParamsToDefault() { 
        
        }

        public void OnParameterValueChanged<T>(string address, T value) {
            if (onFXParamValueChanged != null) onFXParamValueChanged.Invoke(address, value);    
        }

        public void OnParameterAffertorChanged(string address, AffectorFunction affector)
        {
            if (onFXParamAffectorChanged != null) onFXParamAffectorChanged.Invoke(address, affector);
        }



        [System.Serializable]
        public class FXData
        {
            public List<FXParameterData<string>>  stringParameters    = new List<FXParameterData<string>>();
            public List<FXParameterData<int>>     intParameters       = new List<FXParameterData<int>>();
            public List<FXParameterData<float>>   floatParameters     = new List<FXParameterData<float>>();
            public List<FXParameterData<bool>>    boolParameters      = new List<FXParameterData<bool>>();
            public List<FXParameterData<Color>>   colorParameters     = new List<FXParameterData<Color>>();
            public List<FXEnumParameterData>      enumParameters      = new List<FXEnumParameterData>();

            public List<FXGroupData> fxGroupPresets               = new List<FXGroupData>();

            public List<FXMethodData> fXPresetMethods = new List<FXMethodData>();


        }

        [System.Serializable]
        public class FXMethodData
        {
            public string key;

        }

        [System.Serializable]
        public class FXParameterData<T>
        {
            public string key;
            public T value;
            public T defaultValue;
            public T minValue;
            public T maxValue;
            public bool hasMinValue = false;
            public bool hasMaxValue = false;

            // Settings specific to FXScaledParamers 
            public bool isScaled = false;
            public AffectorFunction affector = AffectorFunction.Linear;
            public bool isInverted = false;
        }


        [System.Serializable]
        public class FXEnumParameterData : FXParameterData<int> 
        {
            public List<string> availableNames = new List<string>();
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
                            preset.floatParameters.Add(new FXParameterData<float>
                            {
                                key = key_,
                                value = (float)value_,
                                defaultValue = scaledParamFloat.GetDefaultValue(),
                                minValue = scaledParamFloat.GetMinValue(),
                                maxValue = scaledParamFloat.GetMaxValue(),
                                hasMaxValue = scaledParamFloat.HasMaxValue,
                                hasMinValue = scaledParamFloat.HasMinValue,
                                isScaled = true,
                                affector = scaledParamFloat.AffectorFunction,
                                isInverted = scaledParamFloat.InvertValue
                            }) ;
                        }
                        else if (item.Value.item is FXScaledParameter<Color> scaledParamColor)
                        {
                            preset.floatParameters.Add(new FXParameterData<float>
                            {
                                key = key_,
                                value = (float)value_,
                                defaultValue = scaledParamColor.GetDefaultValue(),

                                minValue = scaledParamColor.GetMinValue(),
                                maxValue = scaledParamColor.GetMaxValue(),
                                hasMaxValue = scaledParamColor.HasMaxValue,
                                hasMinValue = scaledParamColor.HasMinValue,
                                isScaled = true,
                                affector = scaledParamColor.AffectorFunction,
                                isInverted = scaledParamColor.InvertValue
                            });
                        }
                        else if (item.Value.item is FXScaledParameter<int> scaledParamInt)
                        {
                            preset.floatParameters.Add(new FXParameterData<float>
                            {
                                key = key_,
                                value = (float)value_,
                                defaultValue = scaledParamInt.GetDefaultValue(),

                                minValue = scaledParamInt.GetMinValue(),
                                maxValue = scaledParamInt.GetMaxValue(),
                                hasMaxValue = scaledParamInt.HasMaxValue,
                                hasMinValue = scaledParamInt.HasMinValue,
                                isScaled = true,
                                affector = scaledParamInt.AffectorFunction,
                                isInverted = scaledParamInt.InvertValue
                            });
                        }
                        else if (item.Value.item is FXScaledParameter<Vector3> scaledParamVector3)
                        {
                            preset.floatParameters.Add(new FXParameterData<float>
                            {
                                key = key_,
                                value = (float)value_,
                                defaultValue = scaledParamVector3.GetDefaultValue(),

                                minValue = scaledParamVector3.GetMinValue(),
                                maxValue = scaledParamVector3.GetMaxValue(),
                                hasMaxValue = scaledParamVector3.HasMaxValue,
                                hasMinValue = scaledParamVector3.HasMinValue,
                                isScaled = true,
                                affector = scaledParamVector3.AffectorFunction,
                                isInverted = scaledParamVector3.InvertValue
                            });
                        }
                    }
                }

                else if (item.Value.type == FXItemInfoType.Parameter)
                {
                    var parameter = item.Value.item as IFXParameter;

                    if (parameter.ShouldSave || includeAll)
                    {
                        string key_   = item.Key;
                        object value_ = parameter.ObjectValue;

                        if (parameter is FXParameter<float> floatParam)
                        {
                            bool hasMinValue = floatParam.HasMinValue;
                            bool hasMaxValue = floatParam.HasMaxValue;

                            if (hasMinValue || hasMaxValue)
                            {
                                preset.floatParameters.Add(new FXParameterData<float>
                                {
                                    key = key_,
                                    value = (float)value_,
                                    defaultValue = floatParam.GetDefaultValue(),
                                    minValue = floatParam.GetMinValue(),
                                    maxValue = floatParam.GetMaxValue(),
                                    hasMaxValue = floatParam.HasMaxValue,
                                    hasMinValue = floatParam.HasMinValue
                                }) ;
                            }
                            else
                            {
                                preset.floatParameters.Add(new FXParameterData<float> { key = key_, value = (float)value_ });

                            }
                        }
                        else if (parameter is FXParameter<int> intParam)
                        {
                            bool hasMinValue = intParam.HasMinValue;
                            bool hasMaxValue = intParam.HasMaxValue;

                            if (hasMinValue || hasMaxValue)
                            {
                                preset.intParameters.Add(new FXParameterData<int>
                                {
                                    key = key_,
                                    value = (int)value_,
                                    defaultValue = intParam.GetDefaultValue(),
                                    minValue = intParam.GetMinValue(),
                                    maxValue = intParam.GetMaxValue(),
                                    hasMaxValue = intParam.HasMaxValue,
                                    hasMinValue = intParam.HasMinValue,
                                });

                            }
                            else
                            {
                                preset.intParameters.Add(new FXParameterData<int> { key = key_, value = (int)value_ });

                            }
                        }
                        else if (parameter is FXParameter<string> stringParam)
                        {
                            preset.stringParameters.Add(new FXParameterData<string> { key = key_, value = (string)value_ });
                        }
                        else if (parameter is FXParameter<bool> boolParam)
                        {
                            preset.boolParameters.Add(new FXParameterData<bool> { key = key_, value = (bool)value_ , defaultValue = boolParam.GetDefaultValue()});
                        }
                        else if (parameter is FXParameter<Color> colorParam)
                        {
                            preset.colorParameters.Add(new FXParameterData<Color> { key = key_, value = (Color)value_ , defaultValue = colorParam.GetDefaultValue()});
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
                        preset.fXPresetMethods.Add(new FXMethodData { key = item.Key}); 
                    }

                }
            }

            GroupFXController[] allFXGroups = GameObject.FindObjectsOfType<GroupFXController>();
            foreach (var group in allFXGroups)
            {
                preset.fxGroupPresets.Add(group.GetPreset());
            }

            string json = JsonUtility.ToJson(preset);

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
            string filePath      = Path.Combine(directoryPath, presetName + ".json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                FXData preset = JsonUtility.FromJson<FXData>(json);

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

                Dictionary<string, FXGroupData> fxGroupPresets = preset.fxGroupPresets.ToDictionary(p => p.address, p => p);

                GroupFXController[] allFXGroups = GameObject.FindObjectsOfType<GroupFXController>();
                foreach (var group in allFXGroups)
                {
                    if (fxGroupPresets.TryGetValue(group.address, out var groupPreset))
                    {
                        CleanInvalidFXAddresses(groupPreset.fxAddresses);
                        group.LoadPreset(groupPreset);
                    }
                }

                // TODO - Create groups which could nt be found

                onPresetLoaded.Invoke(presetName);

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

        private GroupFXController CreateGroup()
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

            GameObject parent =  GameObject.FindObjectOfType<FXSceneManager>().gameObject;
            if (parent != null) { 
                groupObject.transform.parent = parent.transform;
            }

            group.address      = address;
            group.Initialise();
            return group;
        }

        public GroupFXController CreateGroup(FXGroupData preset) {            
            GroupFXController group = CreateGroup();
            group.LoadPreset(preset);
            return group;
        }

        public void RemoveGroup(string address)
        {
            GroupFXController group = FindGroupByAddress(address);
            if (group != null)
            {
                if (!group.isPinned) GameObject.Destroy(group.gameObject);
                else Debug.LogWarning($"Group with address {address} is pinned so cannot be removed");
            }
            else
            {
                Debug.LogWarning($"Group with address {address} not found and cannot be removed.");
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
    }

}


