using FX;
using UnityEngine;
using UnityEngine.Rendering;

public class FXOutput : FXBase
{
    private Volume volume;
    private FXPostProcessCompositor fXPostProcess;
    public FXScaledParameter<float> brightness = new FXScaledParameter<float>(1.0f, 0, 1.0f, "", false);


    protected override void Awake()
    {
        base.Awake();

        volume = GetComponent<Volume>();
        if (volume == null)
        {
            Debug.LogError("Volume component not found on the GameObject");
            return;
        }

        if (!volume.profile.TryGet<FXPostProcessCompositor>(out fXPostProcess))
        {
            Debug.LogError("Fog effect not found in the Volume profile");
        }

        brightness.OnScaledValueChanged += SetBrightness;

    }

    private void SetBrightness(float value)
    {
        if (fXPostProcess != null)
        {
            fXPostProcess.brightness.value = value;
        }
    }

}
