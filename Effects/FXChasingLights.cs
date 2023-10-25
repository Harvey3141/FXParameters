using UnityEngine;
using FX;

public class FXChasingLights : FXBase, IFXTriggerable
{
    public Light[] lightsArray;
    public enum LightPattern { SineWave, EveryOther, MiddleOut }
    public LightPattern currentPattern = LightPattern.SineWave;

    public FXParameter<float> spanProportion = new FXParameter<float>(0.5f);
    public FXScaledParameter<float> chaseSpeed = new FXScaledParameter<float>(1.0f, 0.1f, 10.0f);
    public FXParameter<bool> forwardDirection = new FXParameter<bool>(true);
    public FXScaledParameter<float> fadeSpeed = new FXScaledParameter<float>(0.05f, 1.0f, 100.0f);
    public FXScaledParameter<float> targetIntensity = new FXScaledParameter<float>(0.5f, 0.0f, 2.0f);


    private int currentLeadIndex = 0;
    private float timeSinceLastTrigger = 0; 
    private float[] targetIntensities; 
    private float[] currentIntensities; 

    protected override void Awake()
    {
        base.Awake();
        chaseSpeed.OnValueChanged += SetChaseSpeed;

        if (lightsArray.Length == 0)
        {
            lightsArray = GetComponentsInChildren<Light>();
        }

        targetIntensities = new float[lightsArray.Length];
        currentIntensities = new float[lightsArray.Length];
        for (int i = 0; i < lightsArray.Length; i++)
        {
            currentIntensities[i] = lightsArray[i].intensity;
            targetIntensities[i] = 0; 
        }
    }

    private void Update()
    {
        // If chaseSpeed is more than 0, update timer and check if we should trigger
        if (chaseSpeed.ScaledValue > 0)
        {
            timeSinceLastTrigger += Time.deltaTime;

            // If time since last trigger exceeds the chase interval, reset timer and trigger
            if (timeSinceLastTrigger > 1f / chaseSpeed.ScaledValue)
            {
                timeSinceLastTrigger = 0;
                FXTrigger();
            }
        }

        UpdateIntensities();

    }

    private void UpdateIntensities()
    {
        for (int i = 0; i < lightsArray.Length; i++)
        {
            if (currentIntensities[i] != targetIntensities[i])
            {
                currentIntensities[i] = Mathf.Lerp(currentIntensities[i], targetIntensities[i], fadeSpeed.ScaledValue * Time.deltaTime);
                lightsArray[i].intensity = currentIntensities[i];

                // If close enough to target, set it directly
                if (Mathf.Abs(currentIntensities[i] - targetIntensities[i]) < 0.01f)
                {
                    lightsArray[i].intensity = targetIntensities[i];
                    currentIntensities[i] = targetIntensities[i];
                    // Ensure value resets to 0
                    targetIntensities[i] = 0.0f;
                }
            }
        }
    }


    [FXMethod]
    public void FXTrigger()
    {
        if (!fxEnabled.Value) return;

        // Apply pattern logic
        switch (currentPattern)
        {
            case LightPattern.SineWave:
                float frequency = 2 * Mathf.PI / lightsArray.Length;
                for (int i = 0; i < lightsArray.Length; i++)
                {
                    float sinValue = 0.5f * Mathf.Sin(frequency * (i + currentLeadIndex)) + 0.5f;
                    targetIntensities[i] = sinValue > spanProportion.Value ? targetIntensity.ScaledValue : 0f;
                }
                break;


            case LightPattern.EveryOther:
                for (int i = 0; i < lightsArray.Length; i++)
                {
                    if (i % 2 == currentLeadIndex % 2)
                    {
                        targetIntensities[i] = targetIntensity.ScaledValue;
                    }
                    else
                    {
                        targetIntensities[i] = 0f;
                    }
                }
                break;


            case LightPattern.MiddleOut:
                int middle = lightsArray.Length / 2;
                int offset = currentLeadIndex % middle;
                for (int i = 0; i < lightsArray.Length; i++)
                {
                    if (i == middle + offset || i == middle - offset)
                    {
                        targetIntensities[i] = targetIntensity.ScaledValue;
                    }
                    else
                    {
                        targetIntensities[i] = 0f;
                    }
                }
                break;

        }

        // Update lead index
        if (forwardDirection.Value)
        {
            currentLeadIndex++;
            if (currentLeadIndex >= lightsArray.Length) currentLeadIndex = 0;
        }
        else
        {
            currentLeadIndex--;
            if (currentLeadIndex < 0) currentLeadIndex = lightsArray.Length - 1;
        }
    }


    private void SetChaseSpeed(float value) 
    {
        if (value == 0.0f) 
        {
            for (int i = 0; i < lightsArray.Length; i++)
            {
                targetIntensities[i] = 0.0f;
            }
        }
    }

    protected override void OnFXEnabled(bool state)
    {
        foreach (Light light in lightsArray)
        {
            light.enabled = state;
        }
    }
}
