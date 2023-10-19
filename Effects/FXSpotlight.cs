using UnityEngine;
using System.Collections;
using System;
using FX;

public class FXSpotlight : FXBase
{
    public FXScaledParameter<float> intensity = new FXScaledParameter<float>(0.0f, 0.0f, 6.0f);
    public FXScaledParameter<float> spotAngle = new FXScaledParameter<float>(0.5f, 5f, 100f);
    public FXParameter<Color> color = new FXParameter<Color>(Color.white);
    public FXParameter<bool> projectorOn = new FXParameter<bool>(false);

    private Light lightComp;
    private Projector projector;
    private Material projectorMaterial;

    protected override void Awake()
    {
        base.Awake();
        intensity.OnScaledValueChanged += SetIntensity;
        spotAngle.OnScaledValueChanged += SetSpotAngle;
        projectorOn.OnValueChanged += SetProjectorEnabled;
        color.OnValueChanged += SetLightColour;


        if (GetComponent<Light>()) lightComp = GetComponent<Light>();
        else
        {
            lightComp = gameObject.AddComponent<Light>();
            lightComp.type = LightType.Spot;
        }

        if (GetComponent<Projector>()) projector = GetComponent<Projector>();
        else
        {
            projector = gameObject.AddComponent<Projector>();
        }
        projectorMaterial = new Material(Resources.Load("FXWireframeProjector", typeof(Material)) as Material);
        projector.enabled = false;
        projector.material = projectorMaterial;

    }


    private void Update()
    {
        if (projector.isActiveAndEnabled)
        {
            projector.fieldOfView = spotAngle.ScaledValue;
            lightComp.color = Color.black;
        }
        else lightComp.color = color.Value;

    }
    protected override void OnFXEnabled(bool state)
    {
        if (state)
        {
            lightComp.enabled = true;
            SetProjectorEnabled(projectorOn.Value);
        }
        else if (!state)
        {
            lightComp.enabled = false;
            projector.enabled = false;
        }
    }

    void SetIntensity(float value)
    {
        lightComp.intensity = value;
        projector.material.SetFloat("_V_WIRE_EmissionStrength", value * 5);
    }

    void SetSpotAngle(float value)
    {
        lightComp.spotAngle = value;

    }

    void SetLightHue(float value)
    {
        value = Mathf.Clamp(value, 0.0f, 1.0f);
        HSBColor tmpCol = new HSBColor(color.Value);
        tmpCol.h = value;
        tmpCol.s = value == 0 ? 0 : 1;
        SetLightColour(tmpCol.ToColor());
    }


    void SetLightColour(Color colour)
    {
        color.Value = colour;
        lightComp.color = colour;
        projector.material.SetColor("_V_WIRE_Color", colour);
    }

    void SetProjectorEnabled(bool value)
    {
        if (!fxEnabled.Value) return;
        projector.enabled = value;
        projector.material.SetFloat("_V_WIRE_EmissionStrength", intensity.ScaledValue * 5);
    }
}
