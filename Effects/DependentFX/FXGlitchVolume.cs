using FX;
using Kino.PostProcessing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Requires - https://github.com/keijiro/Kino
/// </summary>
public class FXGlitchVolume : FXBase
{
    public FXScaledParameter<float> block  = new FXScaledParameter<float>(0, 0, 1.0f,"",false);
    public FXScaledParameter<float> drift  = new FXScaledParameter<float>(0, 0, 1.0f,"",false);
    public FXScaledParameter<float> jitter = new FXScaledParameter<float>(0, 0, 1.0f,"",false);
    public FXScaledParameter<float> jump   = new FXScaledParameter<float>(0, 0, 1.0f,"",false);
    public FXScaledParameter<float> shake  = new FXScaledParameter<float>(0, 0, 1.0f,"",false);

    public FXParameter<bool> blockEnabled = new FXParameter<bool>(false,"",false);

    private Volume volume;
    private Glitch glitchEffect;

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

        block .OnScaledValueChanged   += SetBlock;
        drift .OnScaledValueChanged   += SetDrift;
        jitter.OnScaledValueChanged   += SetJitter;
        jump  .OnScaledValueChanged   += SetJump;
        shake .OnScaledValueChanged   += SetShake;
    }

    private void SetBlock(float value)
    {
        if (glitchEffect != null)
        {
            glitchEffect.block.value = value;
        }
    }

    private void SetDrift(float value)
    {
        if (glitchEffect != null)
        {
            glitchEffect.drift.value = value;
        }
    }

    private void SetJitter(float value)
    {
        if (glitchEffect != null)
        {
            glitchEffect.jitter.value = value;
        }
    }

    private void SetJump(float value)
    {
        if (glitchEffect != null)
        {
            glitchEffect.jump.value = value;
        }
    }
    private void SetShake(float value)
    {
        if (glitchEffect != null)
        {
            glitchEffect.shake.value = value;
        }
    }
}
