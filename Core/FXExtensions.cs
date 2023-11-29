using System;
using System.Linq;
using System.Net;
using System.Reflection;
using UnityEngine;
using static FX.FXManager;

namespace FX
{
    public static class FXExtensions
    {
        public static string AddFXElements(this MonoBehaviour monoBehaviour, string adressPrefix = "")
        {
            // Add FXParameters
            var fields = monoBehaviour.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var fieldType = field.FieldType;
                var fieldInstance = field.GetValue(monoBehaviour);

                if (fieldType.IsGenericType && (fieldType.GetGenericTypeDefinition() == typeof(FXParameter<>) || fieldType.Name.StartsWith("FXScaledParameter")))
                {
                    var fxParameter = (IFXParameter)fieldInstance;
                    var address = fxParameter.Address;
                    if (string.IsNullOrEmpty(address))
                    {
                        if (string.IsNullOrEmpty(adressPrefix))
                            address = $"/{monoBehaviour.gameObject.name}/{monoBehaviour.GetType().Name}/{field.Name}";
                        else
                        {
                            if (adressPrefix.StartsWith("/"))
                                adressPrefix = adressPrefix.Substring(1);
                            address = $"/{adressPrefix}/{field.Name}";
                        }                        
                    }
                    address = address.Replace(" ", "");
                    fxParameter.Address = address;

                    if (FXManager.Instance != null)
                    {
                        FXManager.Instance.AddFXItem(address, FXItemInfoType.Parameter, fxParameter, monoBehaviour);
                    }
                    else
                    {
                        Debug.LogWarning("FXManager.Instance is null. Please ensure it is properly initialized.");
                    }
                }
            }


            // Add FXEnabledParameter
            var fxEnabledField = monoBehaviour.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(f => f.FieldType == typeof(FXEnabledParameter));

            if (fxEnabledField != null)
            {
                var fxEnabledParameter = fxEnabledField.GetValue(monoBehaviour);
                var addressProperty = fxEnabledParameter.GetType().GetProperty("Address");
                var address = (string)addressProperty.GetValue(fxEnabledParameter);
                if (string.IsNullOrEmpty(address))
                {
                    if (string.IsNullOrEmpty(adressPrefix)) address = $"/{monoBehaviour.gameObject.name}/{monoBehaviour.GetType().Name}/FXEnabled";
                    else
                    {
                        if (adressPrefix.StartsWith("/")) adressPrefix = adressPrefix.Substring(1); ;
                        address = $"/{adressPrefix}/FXEnabled";
                    }                   
                }
                address = address.Replace(" ", "");
                addressProperty.SetValue(fxEnabledParameter, address);

                if (FXManager.Instance != null)
                {
                    FXManager.Instance.AddFXItem(address, FXItemInfoType.Parameter, fxEnabledParameter, monoBehaviour);
                }
                else
                {
                    Debug.LogWarning("FXManager.Instance is null. Please ensure it is properly initialized.");
                }
            }

            // Add FXMethods
            var methods = monoBehaviour.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var FXMethodAttribute = method.GetCustomAttribute<FXMethodAttribute>();
                if (FXMethodAttribute != null)
                {
                    var address = FXMethodAttribute.Address;
                    if (address == null)
                    {
                        var methodName = method.Name;
                        if (string.IsNullOrEmpty(adressPrefix)) address = $"/{monoBehaviour.gameObject.name}/{monoBehaviour.GetType().Name}/{methodName}";
                        else
                        {
                            if (adressPrefix.StartsWith("/")) adressPrefix = adressPrefix.Substring(1); ;
                            address = $"/{adressPrefix}/{methodName}";
                        }                       
                    }
                    address = address.Replace(" ", "");
                    FXMethodAttribute.Address = address;

                    FXManager.Instance.AddFXItem(FXMethodAttribute.Address, FXItemInfoType.Method, method, monoBehaviour);
                }
            }

            string computedAddressPrefix = adressPrefix;
            if (string.IsNullOrEmpty(computedAddressPrefix))
            {
                computedAddressPrefix = $"/{monoBehaviour.gameObject.name}/{monoBehaviour.GetType().Name}";
                computedAddressPrefix = computedAddressPrefix.Replace(" ", "");
            }
            return computedAddressPrefix;

        }
    }
}
