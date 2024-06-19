using FX;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;


public class FXLight : FXBaseWithEnabled
{
    private HDAdditionalLightData lightData;

    public enum FXLightType { SPOT, POINT }
    public FXParameter<FXLightType> lightType = new FXParameter<FXLightType>(FXLightType.POINT);

    public FXScaledParameter<float> intensity = new FXScaledParameter<float>(0.0f, 0.0f, 6.0f);   
    public FXParameter<Color> color = new FXParameter<Color>(Color.white);

    public FXScaledParameter<float> spotAngle = new FXScaledParameter<float>(0.5f, 5f, 100f);

    private Light lightComp;

    protected override void Awake()
    {
        base.Awake();
        lightType.OnValueChanged += SetLightType;
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

    void SetLightType(FXLightType lightType)
    {
        switch (lightType)
        {
            case FXLightType.SPOT:
                lightComp.type = LightType.Spot;
                break;
            case FXLightType.POINT:
                lightComp.type = LightType.Point;
                break;
        }
    }

    protected override void OnFXEnabled(bool state)
    {
        lightComp.enabled = state;
    }

    void SetIntensity(float value)
    {
        

        switch (lightType.Value)
        {
            case FXLightType.SPOT:
                lightData.intensity = value;
                break;
            case FXLightType.POINT:
                lightData.intensity = value * 2.0f;
                break;
        }
    }

    void SetLightColour(Color value)
    {
        lightComp.color = value;
    }

    void SetSpotAngle(float value)
    {
        lightComp.spotAngle = value;

    }



}
