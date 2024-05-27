using FX;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class FXEnvironmentControl : FXBase
{
    private Volume volume;
    private Fog fogEffect;

    public FXParameter<bool> fogEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> fogDensity = new FXScaledParameter<float>(0, 0, 1.0f, "", false);

    public GameObject fogPlane;


    protected override void Awake()
    {
        base.Awake();

        volume = GetComponent<Volume>();
        if (volume == null)
        {
            Debug.LogError("Volume component not found on the GameObject");
            return;
        }

        if (!volume.profile.TryGet<Fog>(out fogEffect))
        {
            Debug.LogError("Fog effect not found in the Volume profile");
        }

        fogEnabled.OnValueChanged       += SetFogEnabled;
        fogDensity.OnScaledValueChanged += SetFogDensity;

    }

    private void SetFogEnabled(bool value)
    {
        if (fogEffect != null)
        {
            fogEffect.active = value;
            fogPlane.SetActive(value);
        }
        else fogPlane.SetActive(false);
    }

    private void SetFogDensity(float value)
    {
        if (fogEffect != null)
        {
            fogEffect.meanFreePath.value = fogDensity.ScaledValue;
        }
    }

}
