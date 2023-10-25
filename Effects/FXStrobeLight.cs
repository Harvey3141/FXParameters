using UnityEngine;
using FX;

public class FXStrobeLight : FXBase
{
    private Light lightComp;

    public FXScaledParameter<float> strobeFrequency = new FXScaledParameter<float>(1.0f, 0.1f, 10.0f); // Strobes per second
    public FXScaledParameter<float> intensityOn = new FXScaledParameter<float>(1.0f, 0.0f, 6.0f); // Intensity when light is on
    public FXParameter<float> intensityOff = new FXParameter<float>(0.0f); // Intensity when light is off

    private float nextStrobeTime = 0f;
    private bool isStrobeOn = true;

    protected override void Awake()
    {
        base.Awake();

        if (GetComponent<Light>())
        {
            lightComp = GetComponent<Light>();
        }
        else
        {
            lightComp = gameObject.AddComponent<Light>();
        }
    }

    private void Update()
    {

        if (Time.time > nextStrobeTime)
        {
            isStrobeOn = !isStrobeOn;

            if (isStrobeOn)
            {
                lightComp.intensity = intensityOn.ScaledValue;
            }
            else
            {
                lightComp.intensity = intensityOff.Value;
            }

            nextStrobeTime = Time.time + (1f / strobeFrequency.ScaledValue);
        }
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
            lightComp.intensity = intensityOff.Value;
        }
    }
}
