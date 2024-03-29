using FX;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FXAreaLight : FXBaseWithEnabled
{
    private HDAdditionalLightData lightData;
    public FXScaledParameter<float> intensity = new FXScaledParameter<float>(1.0f, 0.0f, 100.0f);
    public FXParameter<Color> color           = new FXParameter<Color>(Color.white);

    private Light lightComp;

    protected override void Awake()
    {
        base.Awake();
        intensity.OnScaledValueChanged += SetIntensity;
        color.OnValueChanged += SetLightColor;

        lightComp = gameObject.GetComponent<Light>();
        if (lightComp == null)
        {
            lightComp = gameObject.AddComponent<Light>();
            lightComp.type = LightType.Area;
        }

        lightData = gameObject.GetComponent<HDAdditionalLightData>();
        if (lightData == null)
        {
            lightData = gameObject.AddComponent<HDAdditionalLightData>();
        }
    }

    protected override void OnFXEnabled(bool state)
    {
        lightComp.enabled = state;
    }

    void SetIntensity(float value)
    {
        lightData.intensity = value;
    }

    void SetLightColor(Color newColor)
    {
        color.Value = newColor;
        lightData.color = newColor;
    }
}
