using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static FX.FXGroupController;
using static UnityEditor.Progress;

namespace FX {

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

        public static Dictionary<string, (FXItemInfoType type, object item, object fxInstance)> fxItemsByAddress_ = new Dictionary<string, (FXItemInfoType type, object item, object fxInstance)>();

        public enum FXItemInfoType
        {
            Method,
            Parameter
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
                }
            }
            else
            {
                Debug.LogWarning($"No property, method, or trigger found for address {address}");
            }
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

        private void SetParameter(string address, object arg)
        {
            if (fxItemsByAddress_.TryGetValue(address, out var fxItem))
            {
                if (fxItem.type != FXItemInfoType.Parameter)
                {
                    Debug.LogWarning($"FX item at address {address} is not a parameter.");
                    return;
                }

                object parameter = fxItem.item;
                IFXParameter iFXParameter = parameter as IFXParameter;
                if (iFXParameter == null)
                {
                    Debug.LogWarning($"FXParameter {address} is not an instance of IFXParameter");
                    return;
                }

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
                else
                {
                    Debug.LogWarning($"FXParameter {address} has an unsupported type: {parameterType}");
                }


            }
            else
            {
                Debug.LogWarning($"No parameter found for address {address}");
            }
        }


        [System.Serializable]
        public class FXPreset
        {
            public List<FXPresetParameter<string>> stringParameters = new List<FXPresetParameter<string>>();
            public List<FXPresetParameter<int>> intParameters = new List<FXPresetParameter<int>>();
            public List<FXPresetParameter<float>> floatParameters = new List<FXPresetParameter<float>>();
            public List<FXPresetParameter<bool>> boolParameters = new List<FXPresetParameter<bool>>();
            public List<FXPresetParameter<Color>> colorParameters = new List<FXPresetParameter<Color>>();

            public List<FXGroupPreset> fxGroupPresets = new List<FXGroupPreset>();

        }

        [System.Serializable]
        public class FXPresetParameter<T>
        {
            public string key;
            public T value;
        }

        [System.Serializable]
        public class FXGroupPreset
        {
            public string address;
            public SignalSource signalSource;
            public PatternType patternType;
            public AudioFrequency audioFrequency;
            public List<string> fxAddresses = new List<string>();
            public List<string> fxTriggerAddresses = new List<string>();
        }

        public void SavePreset(string presetName)
        {
            FXPreset preset = new FXPreset();

            foreach (var item in fxItemsByAddress_)
            {
                if (item.Value.type == FXItemInfoType.Parameter)
                {
                    var parameter = item.Value.item as IFXParameter;

                    if (parameter.ShouldSave)
                    {
                        string key_ = item.Key;
                        object value_ = parameter.ObjectValue;

                        if (value_ is string strValue)
                            preset.stringParameters.Add(new FXPresetParameter<string> { key = key_, value = strValue });
                        else if (value_ is int intValue)
                            preset.intParameters.Add(new FXPresetParameter<int> { key = key_, value = intValue });
                        else if (value_ is float floatValue)
                            preset.floatParameters.Add(new FXPresetParameter<float> { key = key_, value = floatValue });
                        else if (value_ is bool boolValue)
                            preset.boolParameters.Add(new FXPresetParameter<bool> { key = key_, value = boolValue });
                        else if (value_ is Color colorValue)
                            preset.colorParameters.Add(new FXPresetParameter<Color> { key = key_, value = colorValue });
                    }
                }
            }

            // Save group presets
            FXGroupController[] allFXGroups = GameObject.FindObjectsOfType<FXGroupController>();
            foreach (var group in allFXGroups)
            {
                FXGroupPreset groupPreset    = new FXGroupPreset();
                groupPreset.address          = group.address;
                groupPreset.fxAddresses      = group.fxAddresses;
                groupPreset.fxTriggerAddresses = group.fxTriggerAddresses;
                groupPreset.signalSource     = group.signalSource; 
                groupPreset.patternType      = group.patternType; 
                groupPreset.audioFrequency   = group.audioFrequency;

                preset.fxGroupPresets.Add(groupPreset);
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
            string filePath = Path.Combine(directoryPath, presetName + ".json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                FXPreset preset = JsonUtility.FromJson<FXPreset>(json);

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


                Dictionary<string, FXGroupPreset> fxGroupPresets = preset.fxGroupPresets.ToDictionary(p => p.address, p => p);

                FXGroupController[] allFXGroups = GameObject.FindObjectsOfType<FXGroupController>();
                foreach (var group in allFXGroups)
                {
                    if (fxGroupPresets.TryGetValue(group.address, out var groupPreset))
                    {
                        group.LoadPreset(groupPreset);
                    }
                }

                return true;
               
            }
            else
            {
                Debug.LogWarning($"Preset {presetName} not found.");
                return false;
            }
        }
    }

}


