using System.Reflection;
using UnityEngine;
using FX;
using System;



/// <summary>
/// This class demonstrates the integration of FXManager with MonoBehaviour by implementing the IFXTriggerable interface.
/// It showcases methods that can be triggered by the FX system and holds a state that can be controlled by the same system.
/// </summary>
public class FXExample : MonoBehaviour, IFXTriggerable
{
    /// <summary>
    /// FXParameters are used to expose parameters to the FX system. They can be manipulated through the FX system,
    /// and their current values are stored and retrieved via the FX system.
    /// </summary>
    public FXParameter<float> myFloatParameter = new FXParameter<float>(0.0f);
    public FXParameter<int> myIntParameter = new FXParameter<int>(1);
    public FXParameter<bool> myBoolParameter = new FXParameter<bool>(false);
    public FXParameter<string> myStringParameter = new FXParameter<string>("no");
    public FXParameter<Color> myColorParameter = new FXParameter<Color>(Color.black);

    public enum TestType
    {
        One,
        Two,
        Three,
        Four
    }

    public FXParameter<TestType> myEnumParameter = new FXParameter<TestType>(TestType.One);


    public FXScaledParameter<Color> colorParam = new FXScaledParameter<Color>(0.5f, Color.red, Color.blue);
    public FXScaledParameter<float> floatParam = new FXScaledParameter<float>(0.5f, 0.0f, 10.0f);

    /// <summary>
    /// FXParameter with event handler that gets triggered on value change.
    /// </summary>
    public FXParameter<float> myFloatParameterWithEvent = new FXParameter<float>(0.0f);

    /// <summary>
    /// FXEnabledParameter represents the enabled state of the FX system.
    /// </summary>
    public FXEnabledParameter fxEnabled = new FXEnabledParameter(true);

    private void Start()
    {
        // Adds all FXElements (FXParameters, FXProperties, FXMethods) of this MonoBehaviour to the FX system.
        this.AddFXElements("example");

        // Register event handlers for FXParameters and FXEnabledParameter
        myFloatParameterWithEvent.OnValueChanged += HandleFloatValueChanged;
        fxEnabled.OnValueChanged += FXOnEnabled;

        myEnumParameter.OnValueChanged += HandleEnumValueChanged;

        // Setting the values of the FXParameters through the FX system.
       //FXManager.Instance.SetFX(myFloatParameter.Address, 0.6f);
       //FXManager.Instance.SetFX(myIntParameter.Address, 5);
       //FXManager.Instance.SetFX(myBoolParameter.Address, true);
       //FXManager.Instance.SetFX(myStringParameter.Address, "yes");
       //FXManager.Instance.SetFX(myColorParameter.Address, Color.white);
       //FXManager.Instance.SetFX(myFloatParameterWithEvent.Address, 99.0f);
        FXManager.Instance.SetFX(myEnumParameter.Address, 2, true);



        // Triggering an FXMethod and setting the value of an FXProperty using the FX system.
        string methodAddress = $"/example/{nameof(MyTestIntMethod)}";
        FXManager.Instance.SetFX(methodAddress, 3, true);

        //FXManager.Instance.SetFX(fxEnabled.Address, false);
    }

    private void OnDestroy()
    {
        this.RemoveFXElements();
    }

    /// <summary>
    /// FXMethod that can be triggered by the FX system. Takes an integer as parameter.
    /// </summary>
    [FXMethod]
    public void MyTestIntMethod(int i)
    {
        Debug.Log("MyTestIntMethod - value: " + i);
    }

    /// <summary>
    /// FXMethod that can be triggered by the FX system. Takes a string as parameter.
    /// </summary>
    [FXMethod]
    public void MyTestStringMethod(string s)
    {
        Debug.Log("MyTestStringMethod - value: " + s);
    }

    /// <summary>
    /// Handles the value change event of the FXEnabledParameter.
    /// </summary>
    public void FXOnEnabled(bool value)
    {
        Debug.Log($"Enabled value changed: {value}");
    }

    /// <summary>
    /// Handles the value change event of the myFloatParameterWithEvent.
    /// </summary>
    private void HandleFloatValueChanged(float newValue)
    {
        Debug.Log($"Float value changed: {newValue}");
    }

    /// <summary>
    /// Handles the value change event of the myEnumParameterWithEvent.
    /// </summary>
    private void HandleEnumValueChanged(TestType newValue)
    {
        Debug.Log($"Enum value changed: {newValue}");
    }

    /// <summary>
    /// Required by the IFXTriggerable interface. Can be triggered by the FX system.
    /// </summary>
    [FXMethod]
    public void FXTrigger()
    {
        Debug.Log("FXTrigger Triggered");
    }
}
