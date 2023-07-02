using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static FX.FXManager;

namespace FX
{
    public static class FXExtensions
    {
        public static void AddFXElements(this MonoBehaviour monoBehaviour)
        {
            // Add FXParameters
            var fields = monoBehaviour.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(FXParameter<>))
                {
                    var fxParameter = field.GetValue(monoBehaviour);
                    var addressProperty = field.FieldType.GetProperty("Address");
                    var address = (string)addressProperty.GetValue(fxParameter, null);
                    if (string.IsNullOrEmpty(address))
                    {
                        address = $"/{monoBehaviour.gameObject.name}/{monoBehaviour.GetType().Name}/{field.Name}";
                        addressProperty.SetValue(fxParameter, address, null);
                    }

                    // Make sure the FXManager instance is not null
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
                    address = $"/{monoBehaviour.gameObject.name}/{monoBehaviour.GetType().Name}/FXEnabled";
                    addressProperty.SetValue(fxEnabledParameter, address);
                }

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
                    if (FXMethodAttribute.Address == null)
                    {
                        var methodName = method.Name;
                        FXMethodAttribute.Address = $"/{monoBehaviour.gameObject.name}/{monoBehaviour.GetType().Name}/{methodName}";
                    }

                    FXManager.Instance.AddFXItem(FXMethodAttribute.Address, FXItemInfoType.Method, method, monoBehaviour);
                }
            }

        }
    }
}
