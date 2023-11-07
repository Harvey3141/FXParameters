using FX;
using UnityEngine;

public class FXStrobeLight : FXBase, IFXTriggerable
{
    private Light lightComp;
    public FXScaledParameter<float> strobeFrequency = new FXScaledParameter<float>(1.0f, 0.1f, 20.0f);
    public FXScaledParameter<float> intensityOn = new FXScaledParameter<float>(1.0f, 0.0f, 6.0f);
    public FXParameter<float> intensityOff = new FXParameter<float>(0.0f);
    public FXScaledParameter<float> fadeSpeedOn = new FXScaledParameter<float>(0.05f, 1.0f, 100.0f);
    public FXScaledParameter<float> fadeSpeedOff = new FXScaledParameter<float>(0.05f, 1.0f, 20.0f);

    private float timeSinceLastTrigger = 0f;
    private bool isStrobeOn = false;

    private float targetIntensity;
    private float currentIntensity;

    protected override void Awake()
    {
        base.Awake();

        if (GetComponent<Light>())
        {
            lightComp = GetComponent<Light>();
            currentIntensity = lightComp.intensity;
        }
        else
        {
            lightComp = gameObject.AddComponent<Light>();
            currentIntensity = 0f;
        }
    }

    private void Update()
    {
        float fadeSpeed = isStrobeOn ? fadeSpeedOn.ScaledValue : fadeSpeedOff.ScaledValue;

        // Lerp to the target intensity
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, fadeSpeed * Time.deltaTime);
        lightComp.intensity = currentIntensity;

        if (isStrobeOn && Mathf.Abs(currentIntensity - intensityOn.ScaledValue) < 0.01f)
        {
            // Once the light has reached the target on intensity, start fading off
            isStrobeOn = false;
            targetIntensity = intensityOff.Value;
        }

        if (strobeFrequency.ScaledValue > 0)
        {
            timeSinceLastTrigger += Time.deltaTime;

            // If time since last trigger exceeds the strobe interval, reset timer and trigger
            if (timeSinceLastTrigger > 1f / strobeFrequency.ScaledValue)
            {
                timeSinceLastTrigger = 0;
                FXTrigger();
            }
        }
    }

    [FXMethod]
    public void FXTrigger()
    {
        // Start strobing on
        isStrobeOn = true;
        targetIntensity = intensityOn.ScaledValue;
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
            currentIntensity = intensityOff.Value;
            lightComp.intensity = currentIntensity;
        }
    }
}
