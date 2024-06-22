
using FX;
using IE.RichFX;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Requires - https://github.com/keijiro/Kino
/// </summary>
public class FXPostProcessVolume : FXBase
{
    private Volume volume;
    private Kino.PostProcessing.Glitch glitchEffect;
    private Kino.PostProcessing.Slice sliceEffect;
    private ChromaticAberration chromaticAberrationEffect;
    private LimitlessDistortion1Vol_2 limitlessDistortion1Vol_2;
    private LimitlessDistortion10Vol_2 limitlessDistortion10Vol_2;
    private LimitlessDistortion7Vol_2 limitlessDistortion7Vol_2;

    private Limitless_Distortion_2 limitless_Distortion2;
    private Limitless_Distortion_10 limitless_Distortion10;


    private ScreenGlitch screenGlitchEffect;
    private ChromaLines chromaLinesEffect;
    private DisplaceView displaceViewEffect;
    private ScreenFuzz screenFuzzEffect;
    private ColorEdges colorEdgesEffect;

    public FXParameter<bool> blockEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> block = new FXScaledParameter<float>(0, 0, 1.0f);

    public FXParameter<bool> driftEnabled = new FXParameter<bool>(false);
    public FXScaledParameter<float> drift = new FXScaledParameter<float>(0, 0, 1.0f);

    public FXParameter<bool> jitterEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> jitter = new FXScaledParameter<float>(0, 0, 1.0f);

    public FXParameter<bool> jumpEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> jump = new FXScaledParameter<float>(0, 0, 1.0f);
    
    public FXParameter<bool> shakeEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> shake = new FXScaledParameter<float>(0, 0, 1.0f);

    public FXParameter<bool> sliceEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> slice = new FXScaledParameter<float>(0, 0, 1.0f);

    public FXParameter<bool> chromaticAberrationEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> chromaticAberration = new FXScaledParameter<float>(0, 0, 1.0f);

    public FXParameter<bool> trippyEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> trippy = new FXScaledParameter<float>(0, 0, 1.0f);

    public FXParameter<bool> wobbleEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> wobble = new FXScaledParameter<float>(0, 0, 1.0f);

    public FXParameter<bool> splitRotateEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> splitRotate = new FXScaledParameter<float>(0, 0, 1.0f);

    public FXParameter<bool> wetEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> wetness = new FXScaledParameter<float>(0, 0, 1.0f);

    public FXParameter<bool> screenGlitchEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> screenGlitch = new FXScaledParameter<float>(0, 0, 1.0f);

    public FXParameter<bool> soundWaveEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> soundWave = new FXScaledParameter<float>(0, 0, 1.0f);

    public FXParameter<bool> chromaLinesEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> chromaLines = new FXScaledParameter<float>(0, 0, 1.0f);

    public FXParameter<bool> displaceViewEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> displaceView = new FXScaledParameter<float>(0, 0, 1.0f);

    public FXParameter<bool> screenFuzzEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> screenFuzz = new FXScaledParameter<float>(0, 0, 1.0f);

    public FXParameter<bool> colourEdgesEnabled = new FXParameter<bool>(false, "", true);
    public FXScaledParameter<float> colourEdges = new FXScaledParameter<float>(0, 0, 1.0f);
    public FXParameter<Color> colourEdgeColour = new FXParameter<Color>(Color.white);



    protected override void Awake()
    {
        base.Awake();

        volume = GetComponent<Volume>();
        if (volume == null)
        {
            Debug.LogError("Volume component not found on the GameObject");
        }

        if (!volume.profile.TryGet<Kino.PostProcessing.Glitch>(out glitchEffect))
        {
            Debug.LogError("Glitch effect not found in the Volume profile");
        }

        if (!volume.profile.TryGet<Kino.PostProcessing.Slice>(out sliceEffect))
        {
            Debug.LogError("Slice effect not found in the Volume profile");
        }

        if (!volume.profile.TryGet<ChromaticAberration>(out chromaticAberrationEffect))
        {
            Debug.LogError("Chromatic Aberration effect not found in the Volume profile");
        }

        if (!volume.profile.TryGet<LimitlessDistortion1Vol_2>(out limitlessDistortion1Vol_2))
        {
            Debug.LogError("limitlessDistortion1Vol_2 effect not found in the Volume profile");
        }

        if (!volume.profile.TryGet<LimitlessDistortion10Vol_2>(out limitlessDistortion10Vol_2))
        {
            Debug.LogError("limitlessDistortion10Vol_2 effect not found in the Volume profile");
        }

        if (!volume.profile.TryGet<LimitlessDistortion7Vol_2>(out limitlessDistortion7Vol_2))
        {
            Debug.LogError("limitlessDistortion7Vol_2 effect not found in the Volume profile");
        }

        if (!volume.profile.TryGet<Limitless_Distortion_2>(out limitless_Distortion2))
        {
            Debug.LogError("limitlessDistortion2 effect not found in the Volume profile");
        }

        if (!volume.profile.TryGet<Limitless_Distortion_10>(out limitless_Distortion10))
        {
            Debug.LogError("limitless_Distortion10 effect not found in the Volume profile");
        }

        if (!volume.profile.TryGet<ScreenGlitch>(out screenGlitchEffect))
        {
            Debug.LogError("screenGlitchEffect effect not found in the Volume profile");
        }

        if (!volume.profile.TryGet<ChromaLines>(out chromaLinesEffect))
        {
            Debug.LogError("ChromaLines effect not found in the Volume profile");
        }

        if (!volume.profile.TryGet<DisplaceView>(out displaceViewEffect))
        {
            Debug.LogError("DisplaceView effect not found in the Volume profile");
        }

        if (!volume.profile.TryGet<ScreenFuzz>(out screenFuzzEffect))
        {
            Debug.LogError("ScreenFuzz effect not found in the Volume profile");
        }

        if (!volume.profile.TryGet<ColorEdges>(out colorEdgesEffect))
        {
            Debug.LogError("ColorEdges effect not found in the Volume profile");
        }


        blockEnabled.OnValueChanged               += SetBlockEnabled;
        driftEnabled.OnValueChanged               += SetDriftEnbled;
        jitterEnabled.OnValueChanged              += SetJitterEnbled;
        jumpEnabled.OnValueChanged                += SetJumpEnbled;
        shakeEnabled.OnValueChanged               += SetShakeEnbled;
        sliceEnabled.OnValueChanged               += SetSliceEnabled;
        chromaticAberrationEnabled.OnValueChanged += SetChromaticAberrationEnabled;
        trippyEnabled.OnValueChanged              += SetTrippyEnabled;
        wobbleEnabled.OnValueChanged              += SetWobbleEnabled;
        splitRotateEnabled.OnValueChanged         += SetSplitRotateEnabled;
        wetEnabled.OnValueChanged                 += SetWetEnabled;
        screenGlitchEnabled.OnValueChanged        += SetScreenGlitchEnabled;
        soundWaveEnabled.OnValueChanged           += SetSoundWaveEnabled;
        chromaLinesEnabled.OnValueChanged         += SetChromaLinesEnabled;
        displaceViewEnabled.OnValueChanged        += SetDisplaceViewEnabled;
        screenFuzzEnabled.OnValueChanged          += SetScreenFuzzEnabled;
        colourEdgesEnabled.OnValueChanged          += SetColorEdgesEnabled;



        block.OnScaledValueChanged                += SetBlock;
        drift.OnScaledValueChanged                += SetDrift;
        jitter.OnScaledValueChanged               += SetJitter;
        jump.OnScaledValueChanged                 += SetJump;
        shake.OnScaledValueChanged                += SetShake;
        slice.OnScaledValueChanged                += SetSlice;
        chromaticAberration.OnScaledValueChanged  += SetChromaticAberration;
        trippy.OnScaledValueChanged               += SetTrippy;
        wobble.OnScaledValueChanged               += SetWobble;
        splitRotate.OnScaledValueChanged          += SetSplitRotate;
        wetness.OnScaledValueChanged              += SetWetness;
        screenGlitch.OnScaledValueChanged         += SetScreenGlitch;
        soundWave.OnScaledValueChanged            += SetSoundWave;
        chromaLines.OnScaledValueChanged          += SetChromaLines;
        displaceView.OnScaledValueChanged         += SetDisplaceView;
        screenFuzz.OnScaledValueChanged           += SetScreenFuzz;
        colourEdges.OnScaledValueChanged          += SetColorEdges;
        colourEdgeColour.OnValueChanged           += SetColorEdgesColor;


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

    private void SetTrippyEnabled(bool value)
    {
        if (limitlessDistortion1Vol_2 != null)
        {
            limitlessDistortion1Vol_2.active = value;
        }
    }


    private void SetTrippy(float value)
    {
        if (limitlessDistortion1Vol_2 != null)
        {
            limitlessDistortion1Vol_2.size.value = value;
            limitlessDistortion1Vol_2.strength.value = value * 1.5f;

        }
    }

    private void SetWobbleEnabled(bool value)
    {
        if (limitlessDistortion10Vol_2 != null)
        {
            limitlessDistortion10Vol_2.active = value;
        }
    }

    private void SetWobble(float value)
    {
        if (limitlessDistortion10Vol_2 != null)
        {
            limitlessDistortion10Vol_2.fade.value = value;
        }
    }

    private void SetSplitRotateEnabled(bool value)
    {
        if (limitlessDistortion7Vol_2 != null)
        {
            limitlessDistortion7Vol_2.active = value;
        }
    }

    private void SetSplitRotate(float value)
    {
        if (limitlessDistortion7Vol_2 != null)
        {
            limitlessDistortion7Vol_2.lineWidth.value = value;
        }
    }

    private void SetWetEnabled(bool value)
    {
        if (limitless_Distortion2 != null)
        {
            limitless_Distortion2.active = value;
        }
    }

    private void SetWetness(float value)
    {
        if (limitless_Distortion2 != null)
        {
            limitless_Distortion2.size.value = value;
        }
    }

    private void SetScreenGlitchEnabled(bool value)
    {
        if (screenGlitchEffect != null)
        {
            screenGlitchEffect.active = value;
        }
    }

    private void SetScreenGlitch(float value)
    {
        if (screenGlitchEffect != null)
        {
            screenGlitchEffect.intensity.value = value;
        }
    }

    private void SetSoundWaveEnabled(bool value)
    {
        if (limitless_Distortion10 != null)
        {
            limitless_Distortion10.active = value;
        }
    }

    private void SetSoundWave(float value)
    {
        if (limitless_Distortion10 != null)
        {
            limitless_Distortion10.Intensity.value = value;
        }
    }

    private void SetChromaLines(float value)
    {
        if (chromaLinesEffect != null)
        {
            chromaLinesEffect.intensity.value = value;
        }
    }

    private void SetChromaLinesEnabled(bool value)
    {
        if (chromaLinesEffect != null)
        {
            chromaLinesEffect.active = value;
        }
    }

    private void SetDisplaceView(float value)
    {
        if (displaceViewEffect != null)
        {
            displaceViewEffect.amount.value = new Vector2(value,value);
        }
    }

    private void SetDisplaceViewEnabled(bool value)
    {
        if (displaceViewEffect != null)
        {
            displaceViewEffect.active = value;
        }
    }

    private void SetScreenFuzz(float value)
    {
        if (screenFuzzEffect != null)
        {
            screenFuzzEffect.intensity.value = value;
        }
    }

    private void SetScreenFuzzEnabled(bool value)
    {
        if (screenFuzzEffect != null)
        {
            screenFuzzEffect.active = value;
        }
    }

    private void SetColorEdges(float value)
    {
        if (colorEdgesEffect != null)
        {
            colorEdgesEffect.edgeWidth.value = value;
        }
    }

    private void SetColorEdgesEnabled(bool value)
    {
        if (colorEdgesEffect != null)
        {
            colorEdgesEffect.active = value;
        }
    }

    private void SetColorEdgesColor(Color colour)
    {
        if (colorEdgesEffect != null)
        {
            colorEdgesEffect.edgeColor.value = colour;
        }
    }

}
