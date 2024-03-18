using FX;
using Kino.PostProcessing;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Requires - https://github.com/keijiro/Kino
/// </summary>
public class FXGlitchVolume : FXBase
{
    public FXScaledParameter<float> block               = new FXScaledParameter<float>(0, 0, 1.0f,"",false);
    public FXScaledParameter<float> drift               = new FXScaledParameter<float>(0, 0, 1.0f,"",false);
    public FXScaledParameter<float> jitter              = new FXScaledParameter<float>(0, 0, 1.0f,"",false);
    public FXScaledParameter<float> jump                = new FXScaledParameter<float>(0, 0, 1.0f,"",false);
    public FXScaledParameter<float> shake               = new FXScaledParameter<float>(0, 0, 1.0f,"",false);
    public FXScaledParameter<float> slice               = new FXScaledParameter<float>(0, 0, 1.0f, "", false);
    public FXScaledParameter<float> chromaticAberration = new FXScaledParameter<float>(0, 0, 1.0f, "", false);


    public FXParameter<bool> blockEnabled  = new FXParameter<bool>(false,"", false);
    public FXParameter<bool> driftEnabled  = new FXParameter<bool>(false,"", false);
    public FXParameter<bool> jitterEnabled = new FXParameter<bool>(false,"", false);
    public FXParameter<bool> jumpEnabled   = new FXParameter<bool>(false,"", false);
    public FXParameter<bool> shakeEnabled  = new FXParameter<bool>(false,"", false);
    public FXParameter<bool> sliceEnaled   = new FXParameter<bool>(false, "", false);

    public FXParameter<bool> chromaticAberrationEnabled = new FXParameter<bool>(false, "", false);




    private Volume volume;
    private Glitch glitchEffect;
    private Slice sliceEffect;
    private ChromaticAberration chromaticAberrationEffect;

    protected override void Awake()
    {
        base.Awake();

        volume = GetComponent<Volume>();
        if (volume == null)
        {
            Debug.LogError("Volume component not found on the GameObject");
        }

        if (!volume.profile.TryGet<Glitch>(out glitchEffect))
        {
            Debug.LogError("Glitch effect not found in the Volume profile");
        }

        if (!volume.profile.TryGet<Slice>(out sliceEffect))
        {
            Debug.LogError("Slice effect not found in the Volume profile");
        }

        if (!volume.profile.TryGet<ChromaticAberration>(out chromaticAberrationEffect))
        {
            Debug.LogError("Chromatic Aberration effect not found in the Volume profile");
        }

        block .OnScaledValueChanged              += SetBlock;
        drift .OnScaledValueChanged              += SetDrift;
        jitter.OnScaledValueChanged              += SetJitter;
        jump  .OnScaledValueChanged              += SetJump;
        shake .OnScaledValueChanged              += SetShake;
        slice.OnScaledValueChanged               += SetSlice;
        chromaticAberration.OnScaledValueChanged += SetChromaticAberration;

        blockEnabled .OnValueChanged              += SetBlockEnabled;
        driftEnabled .OnValueChanged              += SetDriftEnbled;
        jitterEnabled.OnValueChanged              += SetJitterEnbled;
        jumpEnabled  .OnValueChanged              += SetJumpEnbled;
        shakeEnabled .OnValueChanged              += SetShakeEnbled;
        sliceEnaled.OnValueChanged                += SetSliceEnabled;
        chromaticAberrationEnabled.OnValueChanged += SetChromaticAberrationEnabled;

    }

    private void SetBlock(float value)
    {
        if (glitchEffect != null)
        {
            glitchEffect.block.value = value;
        }
    }

    private void SetBlockEnabled(bool value)
    {
        if (glitchEffect != null)
        {
            glitchEffect.block.overrideState = value;
        }
    }

    private void SetDrift(float value)
    {
        if (glitchEffect != null)
        {
            glitchEffect.drift.value = value;
        }
    }

    private void SetDriftEnbled(bool value)
    {
        if (glitchEffect != null)
        {
            glitchEffect.drift.overrideState = value;
        }
    }

    private void SetJitter(float value)
    {
        if (glitchEffect != null)
        {
            glitchEffect.jitter.value = value;
        }
    }

    private void SetJitterEnbled(bool value)
    {
        if (glitchEffect != null)
        {
            glitchEffect.jitter.overrideState = value;
        }
    }

    private void SetJump(float value)
    {
        if (glitchEffect != null)
        {
            glitchEffect.jump.value = value;
        }
    }

    private void SetJumpEnbled(bool value)
    {
        if (glitchEffect != null)
        {
            glitchEffect.jump.overrideState = value;
        }
    }
    private void SetShake(float value)
    {
        if (glitchEffect != null)
        {
            glitchEffect.shake.value = value;
        }
    }
    private void SetShakeEnbled(bool value)
    {
        if (glitchEffect != null)
        {
            glitchEffect.shake.overrideState = value;
        }
    }

    private void SetSlice(float value)
    {
        if (sliceEffect != null)
        {
            sliceEffect.displacement.value = value;
        }
    }
    private void SetSliceEnabled(bool value)
    {
        if (sliceEffect != null)
        {
            sliceEffect.active = value;
        }
    }

    private void SetChromaticAberration(float value)
    {
        if (chromaticAberrationEffect != null)
        {
            chromaticAberrationEffect.intensity.value = value;
        }
    }
    private void SetChromaticAberrationEnabled(bool value)
    {
        if (chromaticAberrationEffect != null)
        {
            chromaticAberrationEffect.active = value;
        }
    }
}
