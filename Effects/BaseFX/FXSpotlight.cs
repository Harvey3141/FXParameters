using FX;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;


public class FXSpotlight : FXBaseWithEnabled
{
    private HDAdditionalLightData lightData;

    public FXScaledParameter<float> intensity = new FXScaledParameter<float>(0.0f, 0.0f, 6.0f);
    public FXScaledParameter<float> spotAngle = new FXScaledParameter<float>(0.5f, 5f, 100f);
    public FXParameter<Color> color = new FXParameter<Color>(Color.white);

    private Light lightComp;
    private Material projectorMaterial;

    protected override void Awake()
    {
        base.Awake();
        intensity.OnScaledValueChanged += SetIntensity;
        spotAngle.OnScaledValueChanged += SetSpotAngle;
        color.OnValueChanged += SetLightColour;

        if (GetComponent<Light>()) lightComp = GetComponent<Light>();
        else
        {
            lightComp = gameObject.AddComponent<Light>();
            lightComp.type = LightType.Spot;
        }
        lightData = gameObject.GetComponent<HDAdditionalLightData>();
    }

   
    private void Update()
    {

    }
    protected override void OnFXEnabled(bool state)
    {
        lightComp.enabled = state;
    }

    void SetIntensity(float value)
    {
        lightData.intensity = value;
    }

    void SetSpotAngle(float value)
    {
        lightComp.spotAngle = value;

    }

    void SetLightColour(Color value)
    {
        lightComp.color = value;
    }

    

    void SetLightHue(float value)
    {
        value = Mathf.Clamp(value, 0.0f, 1.0f);
        HSBColor tmpCol = new HSBColor(color.Value);
        tmpCol.h = value;
        tmpCol.s = value == 0 ? 0 : 1;
        color.Value = tmpCol.ToColor();
    }



}
