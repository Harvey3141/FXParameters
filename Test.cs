using System.Reflection;
using UnityEngine;
using static FXManager;

public class Test : MonoBehaviour
{
    public FXParameter<float> myFloatParameter = new FXParameter<float>(0.0f);

    private void Start()
    {
        AddParameters();
        FXManager.Instance.SetFX(myFloatParameter.Address, 0.6f);
    }

    void AddParameters()
    {
        var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(FXParameter<>))
            {
                var fxParameter = (FXParameter<float>)field.GetValue(this);
                if (string.IsNullOrEmpty(fxParameter.Address)) {
                    fxParameter.Address = $"/{this.gameObject.name}/{GetType().Name}/{field.Name}";
                }
                FXManager.Instance.AddFXItem(fxParameter.Address, FXItemInfoType.Parameter, field,this);

            }
        }
    }

    // void Start()
    //{


    ////var parameter = field.GetValue(this);
    //// dynamic parameter = field.GetValue(this);
    //
    //if (parameter != null)
    //{
    //    string address = parameter.Address;
    //    if (address == null) {
    //    
    //    }
    //    FXManager.Instance.AddFXParameter(address, field);
    //}

    //var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    //foreach (var property in properties)
    //{
    //    var FXPropertyAttribute = property.GetCustomAttribute<FXPropertyAttribute>();
    //    if (FXPropertyAttribute != null)
    //    {
    //        if (FXPropertyAttribute.Address == null)
    //        {
    //            var propertyName = property.Name;
    //            FXPropertyAttribute.Address = $"/{this.gameObject.name}/{GetType().Name}/{propertyName}";
    //        }
    //        Debug.Log("Propert name: " + property.Name);
    //        Debug.Log("Property type: " + property.PropertyType);
    //        Debug.Log("Memeber type: " + property.MemberType);
    //        Debug.Log("Attribute address: " + FXPropertyAttribute.Address);
    //        //FXManager.Instance.re propertiesByAddress_[FXPropertyAttribute.Address] = property;
    //
    //    }
    //
    //}
    //
    //var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    //foreach (var method in methods)
    //{
    //    var FXMethodAttribute = method.GetCustomAttribute<FXMethodAttribute>();
    //    if (FXMethodAttribute != null)
    //    {
    //        if (FXMethodAttribute.Address == null)
    //        {
    //            var methodName = method.Name;
    //            FXMethodAttribute.Address = $"/{gameObject.name}/{GetType().Name}/{methodName}";
    //        }
    //        Debug.Log($"Method name: {method.Name}");
    //        Debug.Log($"Member type: {method.MemberType}");
    //        Debug.Log($"Attribute address: {FXMethodAttribute.Address}");
    //        methodsByAddress_[FXMethodAttribute.Address] = method;
    //    }
    //}
    //
    //
    //var triggers = typeof(ITriggerable).GetMethods(BindingFlags.Public | BindingFlags.Instance);
    //foreach (var trigger in triggers)
    //{
    //    var FXMethodAttribute = trigger.GetCustomAttribute<FXMethodAttribute>();
    //    if (FXMethodAttribute != null)
    //    {
    //        if (FXMethodAttribute.Address == null)
    //        {
    //            var methodName = trigger.Name;
    //            FXMethodAttribute.Address = $"/{gameObject.name}/{typeof(ITriggerable).Name}/{methodName}";
    //        }
    //        Debug.Log($"Trigger name: {trigger.Name}");
    //        Debug.Log($"Member type: {trigger.MemberType}");
    //        Debug.Log($"Attribute address: {FXMethodAttribute.Address}");
    //        triggersByAddress_[FXMethodAttribute.Address] = trigger;
    //    }
    //}
    // }
}
