using FX;
using UnityEngine;

public class FXStrobeLight : FXBaseWithEnabled, IFXTriggerable
{
    private Light lightComp;
    public FXScaledParameter<float> strobeFrequency = new FXScaledParameter<float>(1.0f, 0.1f, 20.0f);
    public FXScaledParameter<float> intensityOn     = new FXScaledParameter<float>(1.0f, 0.0f, 6.0f);
    public float intensityOff = 0.0f;
    public FXScaledParameter<float> fadeSpeedOn     = new FXScaledParameter<float>(0.05f, 1.0f, 100.0f);
    public FXScaledParameter<float> fadeSpeedOff    = new FXScaledParameter<float>(0.05f, 1.0f, 20.0f);

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

    private void FixedUpdate()
    {
        float fadeSpeed = isStrobeOn ? fadeSpeedOn.ScaledValue : fadeSpeedOff.ScaledValue;

        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, fadeSpeed * Time.deltaTime);
        lightComp.intensity = currentIntensity;

        if (isStrobeOn && Mathf.Abs(currentIntensity - intensityOn.ScaledValue) < 0.01f)
        {
            isStrobeOn = false;
            targetIntensity = intensityOff;
        }

        if (strobeFrequency.ScaledValue > 0)
        {
            timeSinceLastTrigger += Time.deltaTime;

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
            currentIntensity = intensityOff;
            lightComp.intensity = currentIntensity;
        }
    }
}
