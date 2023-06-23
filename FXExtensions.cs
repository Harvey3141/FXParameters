using System.Reflection;
using UnityEngine;
using static FXManager;

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
                //Debug.Log($"Method name: {method.Name}");
                //Debug.Log($"Member type: {method.MemberType}");
                //Debug.Log($"Method address: {FXMethodAttribute.Address}");

                FXManager.Instance.AddFXItem(FXMethodAttribute.Address, FXItemInfoType.Method, method, monoBehaviour);
            }
        }

        // Add FXProperties
        var properties = monoBehaviour.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var property in properties)
        {
            var FXPropertyAttribute = property.GetCustomAttribute<FXPropertyAttribute>();
            if (FXPropertyAttribute != null)
            {
                if (FXPropertyAttribute.Address == null)
                {
                    var propertyName = property.Name;
                    FXPropertyAttribute.Address = $"/{monoBehaviour.gameObject.name}/{monoBehaviour.GetType().Name}/{propertyName}";
                }
                //Debug.Log("Propert name: " + property.Name);
                //Debug.Log("Property type: " + property.PropertyType);
                //Debug.Log("Memeber type: " + property.MemberType);
                //Debug.Log("Property address: " + FXPropertyAttribute.Address);
                FXManager.Instance.AddFXItem(FXPropertyAttribute.Address, FXItemInfoType.Property, property, monoBehaviour);
            }

        }
    }
}
