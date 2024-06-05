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

        public delegate void OnFXColourParamGlobalColourPaletteIndexChanged(string address, int index);
        public event OnFXColourParamGlobalColourPaletteIndexChanged onFXColourParamGlobalColourPaletteIndexChanged;

        public delegate void OnFXColourParamUseGlobalPaletteChanged(string address, bool value);
        public event OnFXColourParamUseGlobalPaletteChanged onFXColourParamUseGlobalPaletteChanged;

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
                Debug.LogWarning($"FX List does not contain an item with the key: {address}");
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


        public void SetFX(string address, bool setDefaultSceneValue)
        {
            SetFX(address, new object[0], setDefaultSceneValue);
        }

        public void SetFX(string address, object arg, bool setDefaultSceneValue)
        {
            SetFX(address, new object[] {arg}, setDefaultSceneValue);
        }

        public void SetFX(string address, object[] args, bool setDefaultSceneValue)
        {
            if (fxItemsByAddress_.TryGetValue(address, out var fxItem))
            {
                switch (fxItem.type)
                {
                    case FXItemInfoType.Method:
                        SetMethod(address, args);
                        break;
                    case FXItemInfoType.Parameter:
                        SetParameter(address,args[0], setDefaultSceneValue);
                        break;
                    case FXItemInfoType.ScaledParameter:
                        SetParameter(address, args[0], setDefaultSceneValue);
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

        private void SetParameter(string address, object arg = null, bool setDefaultSceneValue = true)
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
                            if (setDefaultSceneValue) scaledParamFloat.Value = floatValue;
                            else scaledParamFloat.SetValue(floatValue,false);
                        }
                        else if (fxItem.item is FXScaledParameter<Color> scaledParamColor)
                        {
                            if (setDefaultSceneValue) scaledParamColor.Value = floatValue;
                            else scaledParamColor.SetValue(floatValue, false);
                        }
                        else if (fxItem.item is FXScaledParameter<int> scaledParamInt)
                        {
                            if (setDefaultSceneValue) scaledParamInt.Value = floatValue;
                            else scaledParamInt.SetValue(floatValue, false);
                        }
                        else if (fxItem.item is FXScaledParameter<Vector3> scaledParamVector3)
                        {
                            if (setDefaultSceneValue) scaledParamVector3.Value = floatValue;
                            else scaledParamVector3.SetValue(floatValue, false);
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
                        if (setDefaultSceneValue) ((FXParameter<float>)parameter).Value = fValueFloat;
                        else ((FXParameter<float>)parameter).SetValue(fValueFloat,false);
                    }
                    else if (parameterType == typeof(int))
                    {
                        if (arg is int iValue)
                        {
                            if (setDefaultSceneValue) ((FXParameter<int>)parameter).Value = iValue;
                            else ((FXParameter<int>)parameter).SetValue(iValue,false);
                        }
                        else if (arg is float fValueInt)
                        {
                            if (setDefaultSceneValue) ((FXParameter<int>)parameter).Value = Mathf.CeilToInt(fValueInt);
                            else ((FXParameter<int>)parameter).SetValue(Mathf.CeilToInt(fValueInt),false);
                        }
                    }
                    else if (parameterType == typeof(bool))
                    {
                        if (arg is bool bValue)
                        {
                            if (setDefaultSceneValue) ((FXParameter<bool>)parameter).Value = bValue;
                            else ((FXParameter<bool>)parameter).SetValue(bValue,false);
                        }
                        else if (arg is float fValueBool)
                        {
                            if (setDefaultSceneValue) ((FXParameter<bool>)parameter).Value = (fValueBool != 0f);
                            else ((FXParameter<bool>)parameter).SetValue((fValueBool != 0f),false);
                        }
                    }
                    else if (parameterType == typeof(string) && arg is string sValue)
                    {
                        if (setDefaultSceneValue) ((FXParameter<string>)parameter).Value = sValue;
                        else ((FXParameter<string>)parameter).SetValue(sValue,false);

                    }
                    else if (parameterType == typeof(Color) && arg is Color cValue)
                    {
                        if (setDefaultSceneValue)((FXParameter<Color>)parameter).Value = cValue;
                        else ((FXParameter<Color>)parameter).SetValue(cValue , false);
                    }
                    else if (parameterType == typeof(Color) && arg is float fValue)
                    {
                        var param = (FXParameter<Color>)parameter; 
                        var hsbColor = new HSBColor(param.Value);  
                        hsbColor.h = fValue;                          
                        if (setDefaultSceneValue) param.Value = hsbColor.ToColor();
                        else param.SetValue (hsbColor.ToColor(),false);
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

        public void ResetParameterToSceneDefault(string address)
        {
            if (fxItemsByAddress_.TryGetValue(address, out var fxItem))
            {

                if (fxItem.type == FXItemInfoType.Parameter || fxItem.type == FXItemInfoType.ScaledParameter)
                {
                    IFXParameter parameter = fxItem.item as IFXParameter;
                    parameter?.ResetToSceneDefaultValue();
                }
            }
            else
            {
                Debug.LogWarning($"No parameter found for address {address}.");
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

        public void InvokeAllParamsValueChanged()
        {
            foreach (var item in fxItemsByAddress_)
            {
                if (item.Value.type == FXItemInfoType.Parameter || item.Value.type == FXItemInfoType.ScaledParameter)
                {
                    IFXParameter parameter = item.Value.item as IFXParameter;
                    parameter?.InvokeParameterValueChanged();
                }
            }
        }

        public void SetParameterGlobalColourIndex(string address, int index)
        {
            if (fxItemsByAddress_.TryGetValue(address, out var fxItem))
            {
                if (fxItem.type == FXItemInfoType.Parameter && fxItem.item is FXParameter<Color> colorParam)
                {
                    colorParam.GlobalColourPaletteIndex = index;
                }
            }
        }

        public void SetParameterUseGlobalColourPalette(string address, bool value)
        {
            if (fxItemsByAddress_.TryGetValue(address, out var fxItem))
            {
                if (fxItem.type == FXItemInfoType.Parameter && fxItem.item is FXParameter<Color> colorParam)
                {
                    colorParam.UseGlobalColourPalette = value;
                }
            }
        }

        public bool TryGetParameterGlobalColourIndex(string address, out int index)
        {
            index = -1; 
            if (fxItemsByAddress_.TryGetValue(address, out var fxItem))
            {
                if (fxItem.type == FXItemInfoType.Parameter && fxItem.item is FXParameter<Color> colorParam)
                {
                    index = colorParam.GlobalColourPaletteIndex;
                    return true;
                }
            }
            return false;
        }

        public bool TryGetParameterUseGlobalColourPalette(string address, out bool value)
        {
            value = false; 
            if (fxItemsByAddress_.TryGetValue(address, out var fxItem))
            {
                if (fxItem.type == FXItemInfoType.Parameter && fxItem.item is FXParameter<Color> colorParam)
                {
                    value = colorParam.UseGlobalColourPalette;
                    return true;
                }
            }
            return false;
        }


        public void OnParameterValueChanged<T>(string address, T value) {
            if (onFXParamValueChanged != null) onFXParamValueChanged.Invoke(address, value);
        }

        public void OnParameterAffertorChanged(string address, AffectorFunction affector)
        {
            if (onFXParamAffectorChanged != null) onFXParamAffectorChanged.Invoke(address, affector);
        }

        public void OnGlobalColourPaletteIndexChanged(string address, int index)
        {
            UpdateColorParameterFromGlobalPalette(address);
            if (onFXColourParamGlobalColourPaletteIndexChanged != null) onFXColourParamGlobalColourPaletteIndexChanged.Invoke(address, index);
        }

        public void OnUseGlobalPaletteChanged(string address, bool value)
        {
            UpdateColorParameterFromGlobalPalette(address);
            if (onFXColourParamUseGlobalPaletteChanged != null) onFXColourParamUseGlobalPaletteChanged.Invoke(address, value);
        }

        /// <summary>
        /// Updates color parameters that use the global color palette with the specified color at the given palette index.
        /// If 'force' is true, it will override the useGlobalColourPalette setting.
        /// </summary>
        /// <param name="paletteIndex">The index of the color in the global color palette.</param>
        /// <param name="color">The color to set for the parameters that match the palette index.</param>
        /// <param name="force">If true, overrides the useGlobalColourPalette setting and forces the update.</param>
        public void UpdateColorParametersWithPaletteIndex(int paletteIndex, Color color, bool force = false)
        {
            foreach (var item in fxItemsByAddress_)
            {
                if (item.Value.type == FXItemInfoType.Parameter && item.Value.item is FXParameter<Color> colorParam)
                {
                    if (force || colorParam.UseGlobalColourPalette)
                    {
                        if (colorParam.GlobalColourPaletteIndex == paletteIndex)
                        {
                            colorParam.SetValue(color, false);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Updates all color parameters to the colors from the given palette.
        /// If 'force' is true, it will override the useGlobalColourPalette setting.
        /// </summary>
        /// <param name="palette">The color palette to update the parameters with.</param>
        /// <param name="force">If true, overrides the useGlobalColourPalette setting and forces the update.</param>
        public void UpdateAllColorParametersToPalette(FX.ColourPalette palette, bool force = false)
        {
            foreach (var item in fxItemsByAddress_)
            {
                if (item.Value.type == FXItemInfoType.Parameter && item.Value.item is FXParameter<Color> colorParam)
                {
                    if (force || colorParam.UseGlobalColourPalette)
                    {
                        int index = colorParam.GlobalColourPaletteIndex;
                        if (index >= 0 && index < palette.colours.Count)
                        {
                            colorParam.SetValue(palette.colours[index], false);
                        }
                        else
                        {
                            Debug.LogWarning($"GlobalColourPaletteIndex {colorParam.GlobalColourPaletteIndex} is out of range for palette {palette.name}.");
                            colorParam.ResetToSceneDefaultValue();
                        }
                    }
                    else
                    {
                        colorParam.ResetToSceneDefaultValue();
                    }
                }
            }
        }



        /// <summary>
        /// Updates the color parameter at the specified address using the global color palette.
        /// If the global color palette is not in use or the index is out of range, the parameter is reset to its scene default value.
        /// </summary>
        /// <param name="address">The address of the color parameter to update.</param>
        /// <param name="force">If true, overrides the useGlobalColourPalette setting and forces the update.</param>
        public void UpdateColorParameterFromGlobalPalette(string address, bool force = false)
        {
            if (fxItemsByAddress_.TryGetValue(address, out var fxItem))
            {
                if (fxItem.type == FXItemInfoType.Parameter && fxItem.item is FXParameter<Color> colorParam)
                {
                    if (force || colorParam.UseGlobalColourPalette)
                    {
                        var palette = FXColourPaletteManager.Instance.activePalette;
                        if (palette != null && colorParam.GlobalColourPaletteIndex >= 0 && colorParam.GlobalColourPaletteIndex < palette.colours.Count)
                        {
                            Color c = palette.colours[colorParam.GlobalColourPaletteIndex];
                            colorParam.SetValue(c, false);
                        }
                        else
                        {
                            Debug.LogWarning($"GlobalColourPaletteIndex {colorParam.GlobalColourPaletteIndex} is out of range for palette {palette?.name}.");
                            colorParam.ResetToSceneDefaultValue();
                        }
                    }
                    else
                    {
                        colorParam.ResetToSceneDefaultValue();
                    }
                }
            }
        }




        public void OnGroupChanged(FXGroupData data)
        {
            if (onFXGroupChanged != null) onFXGroupChanged.Invoke(data);
        }

        public void InvokeAllGroupChanged() {
            GroupFXController[] allGroups = GameObject.FindObjectsOfType<GroupFXController>();
            foreach (var group in allGroups)
            {
                OnGroupChanged(group.GetData());
            }
        }

        public void InvokeAllSceneStateUpdates()
        {
            InvokeAllParamsValueChanged();
            InvokeAllGroupChanged();
        }


        [System.Serializable]
        public class FXEnumData
        {
            public List<string> signalTypes        = new List<string>();
            public List<string> patternTypes       = new List<string>();
            public List<string> oscillatorTypes    = new List<string>();
            public List<string> arpeggiatorTypes   = new List<string>();
            public List<string> affectorTypes      = new List<string>();
            public List<string> frequencyTypes     = new List<string>();
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

            AudioFrequency aFR = AudioFrequency.Low;
            Type taFR = aFR.GetType();

            data.signalTypes      = Enum.GetNames(tS).ToList();
            data.patternTypes     = Enum.GetNames(tP).ToList();
            data.oscillatorTypes  = Enum.GetNames(tO).ToList();
            data.arpeggiatorTypes = Enum.GetNames(tA).ToList();
            data.affectorTypes    = Enum.GetNames(tAF).ToList();
            data.frequencyTypes   = Enum.GetNames(taFR).ToList();

            return data;
        }


        [System.Serializable]
        public class FXData
        {
            public List<FXParameterData<string>> stringParameters = new List<FXParameterData<string>>();
            public List<FXParameterData<int>> intParameters       = new List<FXParameterData<int>>();
            public List<FXParameterData<float>> floatParameters   = new List<FXParameterData<float>>();
            public List<FXParameterData<bool>> boolParameters     = new List<FXParameterData<bool>>();
            public List<FXColourParameterData> colorParameters   = new List<FXColourParameterData>();
            public List<FXEnumParameterData> enumParameters       = new List<FXEnumParameterData>();

            // FXGroups 
            public FXEnumData fXEnumData = new FXEnumData();    
            public List<FXGroupData> fxGroupPresets               = new List<FXGroupData>();
            public List<FXMethodData> fXPresetMethods             = new List<FXMethodData>();

            public List<string> sceneTagIds = new List<string>();
        }

        [System.Serializable]
        public class FXMethodData
        {
            public string key;
        }

        public void SaveScene(FX.Scene scene, bool includeAll = false)
        {
            FXData preset = new FXData();

            foreach (var item in fxItemsByAddress_)
            {
                if (item.Value.type == FXItemInfoType.ScaledParameter)
                {
                    var parameter = item.Value.item as IFXParameter;

                    if (parameter.ShouldSave || includeAll)
                    {
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
                            preset.colorParameters.Add((FXColourParameterData)colorParam.GetData());
                        }
                        else
                        {
                            Type valueType = parameter.ObjectValue.GetType();
                            if (valueType.IsEnum)
                            {
                                FXEnumParameterData enumParameter = new FXEnumParameterData
                                {
                                    key = item.Key,
                                    value = (int)parameter.ObjectValue,
                                    availableNames = Enum.GetNames(valueType).ToList()
                                };
                                preset.enumParameters.Add(enumParameter);
                            }
                        }
                    }
                }
                if (includeAll)
                {
                    if (item.Value.type == FXItemInfoType.Method)
                    {
                        preset.fXPresetMethods.Add(new FXMethodData { key = item.Key });
                    }
                }
            }

            preset.fXEnumData = GetFXEnumData();

            preset.sceneTagIds = scene.TagIds;  // Update to store tag IDs

            GroupFXController[] allFXGroups = GameObject.FindObjectsOfType<GroupFXController>();
            foreach (var group in allFXGroups)
            {
                preset.fxGroupPresets.Add(group.GetData());
            }

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {
                new ColourHandler()
                },
            };

            string json = JsonConvert.SerializeObject(preset, settings);

            string directoryPath = Path.Combine(Application.streamingAssetsPath, "FX Scenes");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, scene.Name + ".json");
            File.WriteAllText(filePath, json);
        }



        public bool LoadScene(string presetName, out List<string> loadedTagIds) 
        {
            string directoryPath = Path.Combine(Application.streamingAssetsPath, "FX Scenes");
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
                FXData preset = JsonConvert.DeserializeObject<FXData>(json, settings);

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

                    if (existingGroup != null)
                    {
                        existingGroup.SetData(groupPreset.Value);
                    }
                    else
                    {
                        CreateGroup(groupPreset.Value);
                    }
                }

                foreach (var param in preset.stringParameters) { SetFX(param.key, param.value, true); }
                foreach (var param in preset.intParameters)    { SetFX(param.key, param.value, true); }
                foreach (var param in preset.floatParameters)  { SetFX(param.key, param.value, true); }
                foreach (var param in preset.boolParameters)   { SetFX(param.key, param.value, true); }
                foreach (var param in preset.colorParameters) {
                    if (fxItemsByAddress_.TryGetValue(param.key, out var fxItem))
                    {
                        if (fxItem.type == FXItemInfoType.Parameter && fxItem.item is FXParameter<Color> colorParam)
                        {
                            colorParam.UseGlobalColourPalette   = param.useGlobalColourPalette;
                            colorParam.GlobalColourPaletteIndex = param.globalColourPaletteIndex;
                            colorParam.SetValue(param.value, true);
                        }
                    }                    
                }
                foreach (var param in preset.enumParameters) { SetFX(param.key, param.value, true); }

                HashSet<string> presetAddresses = new HashSet<string>(
                    preset.boolParameters.Select(p => p.key)
                    .Concat(preset.stringParameters.Select(p => p.key))
                    .Concat(preset.intParameters.Select(p => p.key))
                    .Concat(preset.floatParameters.Select(p => p.key))
                    .Concat(preset.colorParameters.Select(p => p.key))
                    .Concat(preset.enumParameters.Select(p => p.key))
                );

                foreach (var fxItem in fxItemsByAddress_)
                {
                    if (fxItem.Value.type == FXItemInfoType.Parameter)
                    {
                        IFXParameter parameter = fxItem.Value.item as IFXParameter;
                        if (parameter != null && parameter.ShouldSave && !presetAddresses.Contains(fxItem.Key))
                        {
                            parameter.ResetToDefaultValue();
                            Debug.Log($"Parameter '{fxItem.Key}' not found in the preset. Resetting to default value");
                        }
                    }
                }

                loadedTagIds = preset.sceneTagIds; 

                if (onPresetLoaded != null) onPresetLoaded.Invoke(presetName);
                return true;
            }
            else
            {
                Debug.LogWarning($"Preset {presetName} not found.");
                loadedTagIds = null;
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

        public void RemoveFXParamsFromGroup(string groupAddress)
        {
            GroupFXController group = FindGroupByAddress(groupAddress);
            if (group != null)
            {
                group.ClearFXAdresses();
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

        public void RemoveFXTriggersFromGroup(string groupAddress)
        {
            GroupFXController group = FindGroupByAddress(groupAddress);
            if (group != null)
            {
                group.RemoveFXTriggers();
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
            group.isPinned = false;
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
                    RemoveFXItem(group.value.Address);
                    GameObject.DestroyImmediate(group.gameObject);
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


