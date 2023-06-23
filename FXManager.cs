using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

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

    public static Dictionary<string, (FXItemInfoType type, object item, object fxInstance)> fxItemsByAddress_ = new Dictionary<string, (FXItemInfoType type, object item, object fxInstance)>();

    public enum FXItemInfoType
    {
        Property,
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
            fxItemsByAddress_.Add(address, (type, item,fxInstance));
        }
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
                case FXItemInfoType.Property:
                    SetProperty(address, args[0]);
                    break;
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

    private void SetProperty(string address, object arg)
    {
        if (fxItemsByAddress_.TryGetValue(address, out var item) && item.type == FXItemInfoType.Property)
        {
            PropertyInfo property = item.Item2 as PropertyInfo;
            if (property == null)
            {
                Debug.LogWarning($"FX item at address {address} is not a property.");
                return;
            }
            object instance = item.fxInstance;

            Type propertyType = property.PropertyType;

            if (propertyType == typeof(float) && arg is float fValue)
            {
                property.SetValue(instance, fValue);
            }
            else if (propertyType == typeof(int) && arg is int iValue)
            {
                property.SetValue(instance, iValue);
            }
            else if (propertyType == typeof(bool) && arg is bool bValue)
            {
                property.SetValue(instance, bValue);
            }
            else if (propertyType == typeof(string) && arg is string sValue)
            {
                property.SetValue(instance, sValue);
            }
            else
            {
                Debug.LogWarning($"Property {property.Name} has an unsupported type: {propertyType}");
            }
        }
        else
        {
            Debug.LogWarning($"No property found for address {address}");
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

            if (parameterType == typeof(float) && arg is float fValue)
            {
                ((FXParameter<float>)parameter).Value = fValue;
            }
            else if (parameterType == typeof(int) && arg is int iValue)
            {
                ((FXParameter<int>)parameter).Value = iValue;
            }
            else if (parameterType == typeof(bool) && arg is bool bValue)
            {
                ((FXParameter<bool>)parameter).Value = bValue;
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



    //public void SetTrigger(string address)
    //{
    //    if (triggersByAddress_.TryGetValue(address, out var trigger))
    //    {
    //        trigger.Item1.Invoke(trigger.Item2, null);
    //    }
    //}
    //


    [FXMethod()]
    private void MyFunction(float arg1, int arg2)
    {
        Debug.Log($"MyFunction triggered with args: {arg1}, {arg2}");
    }


    public void Trigger()
    {
        Debug.Log("Triggered");
    }




    [System.Serializable]
    public class FXPreset
    {
        public List<FXPresetParameter<string>> stringParameters = new List<FXPresetParameter<string>>();
        public List<FXPresetParameter<int>> intParameters = new List<FXPresetParameter<int>>();
        public List<FXPresetParameter<float>> floatParameters = new List<FXPresetParameter<float>>();
        public List<FXPresetParameter<bool>> boolParameters = new List<FXPresetParameter<bool>>();
        public List<FXPresetParameter<Color>> colorParameters = new List<FXPresetParameter<Color>>();
    }

    [System.Serializable]
    public class FXPresetParameter<T>
    {
        public string key;
        public T value;
    }

    public void SavePreset(string presetName)
    {
        FXPreset preset = new FXPreset();
        foreach (var item in fxItemsByAddress_)
        {
            if (item.Value.type == FXManager.FXItemInfoType.Parameter)
            {
                var parameter = item.Value.item as IFXParameter;
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

        string json = JsonUtility.ToJson(preset);

        string directoryPath = Application.streamingAssetsPath;
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = Path.Combine(directoryPath, presetName + ".json");
        File.WriteAllText(filePath, json);
    }






    //public void LoadPreset(string presetName)
    //{
    //    // Load json from file...
    //    FXPreset preset = JsonUtility.FromJson<FXPreset>(json);
    //    foreach (var pair in preset.parameters)
    //    {
    //        if (fxItemsByAddress_.ContainsKey(pair.Key))
    //        {
    //            var item = fxItemsByAddress[pair.Key];
    //            if (item.type == FXManager.FXItemInfoType.Parameter)
    //            {
    //                var parameter = item.item as IFXParameter;
    //                parameter.ObjectValue = pair.Value;
    //            }
    //        }
    //    }
    //}

}
