using UnityEngine;
using FX;
using UnityEngine.Rendering.HighDefinition;
using System;


public class FXDirectionalLight : FXBaseWithEnabled, IFXTriggerable
{
    private HDAdditionalLightData lightData; 

    public FXScaledParameter<float> intensity = new FXScaledParameter<float>(0.0f, 0.0f, 2.0f);
    public FXParameter<Color> color = new FXParameter<Color>(Color.white);

    public FXParameter<float> rotationX = new FXParameter<float>(0.0f, 0.0f, 360f);
    public FXParameter<float> rotationY = new FXParameter<float>(0.0f, 0.0f, 360f);
    public FXScaledParameter<float> rotationSpeedX = new FXScaledParameter<float>(0.0f,0.0f,100.0f);
    public FXScaledParameter<float> rotationSpeedY = new FXScaledParameter<float>(0.0f, 0.0f, 100.0f);

    private Light lightComp;
    private Vector3 initRot;
    public float clampX = 360f;
    public float clampY = 360f;

    public Vector3[] rotationSet; 

    protected override void Awake()
    {
        base.Awake();

        initRot = transform.localEulerAngles;

        intensity.OnScaledValueChanged += SetIntensity;
        color.OnValueChanged += SetLightColour;

        if (GetComponent<Light>()) lightComp = GetComponent<Light>();
        else
        {
            lightComp = gameObject.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            

        }
        lightData = gameObject.GetComponent<HDAdditionalLightData>();

    }

    private void Update()
    {
        if (rotationSpeedX.ScaledValue != 0 || rotationSpeedY.ScaledValue != 0) {
            rotationX.Value += rotationSpeedX.ScaledValue;
            if (rotationX.Value > clampX) rotationX.Value = 0;
            rotationY.Value += rotationSpeedY.ScaledValue;
            if (rotationY.Value > clampY) rotationY.Value = 0;
        }

        transform.localEulerAngles = new Vector3(rotationX.Value, rotationY.Value, 0.0f);
        
        lightComp.color = color.Value;
    }

    protected override void OnFXEnabled(bool state)
    {
        if (state)
        {
            lightComp.enabled = true;
        }
        else
        {
            lightComp.enabled = false;
        }
    }

    void SetIntensity(float value)
    {
        if (lightData) lightData.intensity = value;
    }

    void SetLightColour(Color colour)
    {
        lightComp.color = colour;
    }

    [FXMethod]
    public void FXTrigger() {


        int index = UnityEngine.Random.Range(0, rotationSet.Length-1);

        Vector3 currentRotation = transform.rotation.eulerAngles;
        currentRotation.x = rotationSet[index].x;
        currentRotation.y = rotationSet[index].y;
        currentRotation.z = rotationSet[index].z;

        rotationX.Value = rotationSet[index].x;
        rotationY.Value = rotationSet[index].y;

    }
}
