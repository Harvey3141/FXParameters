using System.Reflection;
using UnityEngine;
using FX;

// This class is an example of how to integrate FXManager with MonoBehaviour.
// It implements both IFXTriggerable and IFXEnabler interfaces, indicating it has methods that can be triggered by the FX system,
// and it has an 'enabled' state that can be controlled by the FX system.
public class FXExample : MonoBehaviour, IFXTriggerable, IFXEnabler
{

    // FXParameters are used to expose parameters to the FX system. They can be manipulated through the FX system
    // and their current values are stored and retrieved via the FX system.
    public FXParameter<float>   myFloatParameter     = new FXParameter<float>(0.0f);
    public FXParameter<int>     myIntParameter       = new FXParameter<int>(1);
    public FXParameter<bool>    myBoolParameter      = new FXParameter<bool>(false);
    public FXParameter<string>  myStringParameter    = new FXParameter<string>("no");
    public FXParameter<Color>   myColorParameter     = new FXParameter<Color>(Color.black);

    public FXParameter<float> myFloatParameterWithEvent = new FXParameter<float>(0.0f);


    //// Not working - DO NOT USE
    //// FXProperty attribute marks a property to be managed by the FX system. The FX system will automatically assign an address to it.
    //[FXProperty]
    //[field: SerializeField] // Exposes the FXProperty in the inspector
    //public int IntProperty { get; set; } = 0;

    // The FXEnabled property is required when implementing the IFXEnabler interface. 
    // This property indicates the enabled state of the component and can be manipulated through the FX system.
    [FXProperty]
    [field: SerializeField] // Exposes the FXProperty in the inspector
    public bool FXEnabled { get; set; } = false;


    // These methods save and load presets. Presets store the current values of all FXParameters and FXProperties managed by the FX system.
    public void SavePreset1() { FXManager.Instance.SavePreset("Preset1"); }

    public void LoadPreset1() { FXManager.Instance.LoadPreset("Preset1"); }

    public void SavePreset2() { FXManager.Instance.SavePreset("Preset2"); }

    public void LoadPreset2() { FXManager.Instance.LoadPreset("Preset2"); }

    private void Start()
    {
        // Adds all FXElements (FXParameters, FXProperties, FXMethods) of this MonoBehaviour to the FX system.
        this.AddFXElements();

        myFloatParameterWithEvent.OnValueChanged += HandleFloatValueChanged;

        // Here we use the FX system to set the values of the FXParameters.
        FXManager.Instance.SetFX(myFloatParameter.Address, 0.6f);
        FXManager.Instance.SetFX(myIntParameter.Address, 5);
        FXManager.Instance.SetFX(myBoolParameter.Address, true);
        FXManager.Instance.SetFX(myStringParameter.Address, "yes");
        FXManager.Instance.SetFX(myColorParameter.Address, Color.white);

        FXManager.Instance.SetFX(myFloatParameterWithEvent.Address, 99.0f);

        // Here we use the FX system to call an FXMethod and to set the value of an FXProperty.
        string methodAddress = $"/{gameObject.name}/{GetType().Name}/{nameof(MyTestIntMethod)}";
        FXManager.Instance.SetFX(methodAddress, 3);

        //// Not working - DO NOT USE
        //string propertyAddress = $"/{gameObject.name}/{GetType().Name}/{nameof(IntProperty)}";
        //FXManager.Instance.SetFX(propertyAddress, 99);
    }

    // This method is marked with the FXMethod attribute, so it can be triggered by the FX system.
    [FXMethod]
    public void MyTestIntMethod(int i)
    {
        Debug.Log("MyTestIntMethod - value: " + i);
    }

    private void HandleFloatValueChanged(float newValue)
    {
        // Handle the value change here
        Debug.Log($"Float value changed: {newValue}");
    }

    // This method is required by the IFXTriggerable interface. It can be triggered by the FX system.
    public void FXTrigger()
    {
        Debug.Log("FXTrigger Triggered");
    }

}