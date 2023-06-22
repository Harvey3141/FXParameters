using System.Net;
using System.Reflection;
using UnityEngine;
using static FXManager;

public class Test : MonoBehaviour
{
    public FXParameter<float>   myFloatParameter    = new FXParameter<float>(0.0f);
    public FXParameter<int>     myIntParameter      = new FXParameter<int>(1);
    public FXParameter<bool>    myBoolParameter     = new FXParameter<bool>(false);
    public FXParameter<string>  myStringParameter   = new FXParameter<string>("no");
    public FXParameter<Color>   myColorParameter    = new FXParameter<Color>(Color.black);


    //public FXParameter<Color> myColorParameter = new FXParameter<Color>(Color.black);


    private void Start()
    {
        AddParameters();
        AddMethods();
        FXManager.Instance.SetFX(myFloatParameter.Address, 0.6f);
        FXManager.Instance.SetFX(myIntParameter.Address, 5);
        FXManager.Instance.SetFX(myBoolParameter.Address, true);
        FXManager.Instance.SetFX(myStringParameter.Address, "yes");
        FXManager.Instance.SetFX(myColorParameter.Address, Color.white);

        //string add = $"/{gameObject.name}/{GetType().Name}/{methodName}";

        string address = "/GameObject/Test/Test2";
        //object[] args = new object[] { 5, 10 }; // Replace with your method arguments

        FXManager.Instance.SetFX(address, 3);

    }


    // move this to subclass or interface
    void AddParameters()
    {
        var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(FXParameter<>))
            {
                var fxParameter = field.GetValue(this);
                var addressProperty = field.FieldType.GetProperty("Address");
                var address = (string)addressProperty.GetValue(fxParameter, null);
                if (string.IsNullOrEmpty(address))
                {
                    address = $"/{gameObject.name}/{GetType().Name}/{field.Name}";
                    addressProperty.SetValue(fxParameter, address, null);
                }
                FXManager.Instance.AddFXItem(address, FXItemInfoType.Parameter, field, this);
            }
        }
    }

    void AddMethods()
    {
        var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var method in methods)
        {
            var FXMethodAttribute = method.GetCustomAttribute<FXMethodAttribute>();
            if (FXMethodAttribute != null)
            {
                if (FXMethodAttribute.Address == null)
                {
                    var methodName = method.Name;
                    FXMethodAttribute.Address = $"/{gameObject.name}/{GetType().Name}/{methodName}";
                }
                Debug.Log($"Method name: {method.Name}");
                Debug.Log($"Member type: {method.MemberType}");
                Debug.Log($"Attribute address: {FXMethodAttribute.Address}");

                FXManager.Instance.AddFXItem(FXMethodAttribute.Address, FXItemInfoType.Method, method, this);
            }
        }

    }

    [FXMethod]
    void Test2(int i) {
        Debug.Log("TEST" + i);
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
