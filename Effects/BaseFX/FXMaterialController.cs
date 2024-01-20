using FX;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MaterialType
{
    DEFAULT,
    EMISSIVE,
    CONCRETE,
    RUSTY_CONCRETE
}

public enum TriggerPattern
{
    ALL,
    ODD_EVEN,
    SEQUENTIAL,
    RANDOM_SINGLE,
    RANDOM_MULTI
}

public static class MaterialTypeInfo
{
    public static int Count { get { return Enum.GetNames(typeof(MaterialType)).Length; } }
}

public class FXMaterialController : FXBase, IFXTriggerable
{
    public FXParameter<int> materialIndex = new FXParameter<int>(0);

    public FXScaledParameter<float> emmissiveIntensity = new FXScaledParameter<float>(0.0f,0.0f,1.0f);


    public FXScaledParameter<float> triggerDuration = new FXScaledParameter<float>(0.05f, 0.0f, 1.0f);
    private bool isTriggerCoroutineActive = false;
    public TriggerPattern emissivePattern = TriggerPattern.ALL;
    public int triggerSequencialIndex = 0;
    public bool triggerOddEvenState = false;
    private System.Random triggerRandomiser = new System.Random();
    private List<int> triggerRandomIndices = new List<int>();



    public GameObject[] controlledObjects;

    public Material Default;
    public Material Emissive;
    public Material Concrete;
    public Material RustyConcrete;

    protected override void Awake()
    {
        base.Awake();
        materialIndex.OnValueChanged += SetMaterial;
        emmissiveIntensity.OnScaledValueChanged += SetEmissiveIntensity;
    }

    protected override void Start()
    {
        SetMaterial(MaterialType.DEFAULT);
    }

    private void Update()
    {
        switch ((MaterialType)materialIndex.Value)
        {
            case MaterialType.DEFAULT:
                break;
            case MaterialType.EMISSIVE:
                break;
            case MaterialType.CONCRETE:
                break;
            case MaterialType.RUSTY_CONCRETE:
                break;
            default:
                break;
        }
    }

    private bool ApplyMaterial(Material material)
    {
        if (material == null) return false;
        foreach (var obj in controlledObjects)
        {
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material uniqueMaterial = new Material(material);
                    renderer.material = uniqueMaterial;
                }
            }
        }
        return true;
    }


    private void SetMaterial(int index)
    {
        if (index < MaterialTypeInfo.Count) SetMaterial((MaterialType)index);
    }

    public void SetMaterial(MaterialType type)
    {
        switch (type)
        {
            case MaterialType.DEFAULT:
                ApplyMaterial(Default);
                break;
            case MaterialType.EMISSIVE:
                ApplyMaterial(Emissive);
                SetEmissiveIntensity(emmissiveIntensity.ScaledValue);
                break;
            case MaterialType.CONCRETE:
                ApplyMaterial(Concrete);
                break;
            case MaterialType.RUSTY_CONCRETE:
                ApplyMaterial(RustyConcrete);
                break;
            default:
                break;
        }
    }


    public void SetEmissiveIntensity(float intensity)
    {
        
        if ((MaterialType)materialIndex.Value != MaterialType.EMISSIVE) return;

        for (int i = 0; i < controlledObjects.Length; i++)
        {
            GameObject obj = controlledObjects[i];
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && ShouldApply(i))
                {
                    if (Emissive != null)
                    {
                        Color emissiveColorLDR = renderer.material.GetColor("_EmissiveColorLDR");
                        Color emissiveColor = new Color(Mathf.GammaToLinearSpace(emissiveColorLDR.r), Mathf.GammaToLinearSpace(emissiveColorLDR.g), Mathf.GammaToLinearSpace(emissiveColorLDR.b));
                        renderer.material.SetColor("_EmissiveColor", emissiveColor * intensity);
                    }
                }
            }
        }
    }

    [FXMethod]
    public void FXTrigger() {
        if (!isTriggerCoroutineActive) {
            triggerOddEvenState = !triggerOddEvenState;
            triggerSequencialIndex = (triggerSequencialIndex + 1) % controlledObjects.Length;
            GenerateRandomIndices(UnityEngine.Random.Range(0, controlledObjects.Length));
            StartCoroutine(LerpEmissiveIntensity());
        } 
    }

    private IEnumerator LerpEmissiveIntensity()
    {
        isTriggerCoroutineActive = true;
        float halfDuration = triggerDuration.ScaledValue / 2.0f;
        float timer = 0.0f;

        float startIntensity = emmissiveIntensity.ScaledValue;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float intensity = Mathf.Lerp(startIntensity, 1.0f, timer / halfDuration);
            SetEmissiveIntensity(intensity);
            yield return null;
        }

        timer = 0.0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float intensity = Mathf.Lerp(1.0f, 0.0f, timer / halfDuration);
            SetEmissiveIntensity(intensity);
            yield return null;
        }

        isTriggerCoroutineActive = false;
    }

    private bool ShouldApply(int index)
    {
        switch (emissivePattern)
        {
            case TriggerPattern.ALL:
                return true;
            case TriggerPattern.ODD_EVEN:
                return (index % 2 == 0) ? triggerOddEvenState : !triggerOddEvenState;               
            case TriggerPattern.SEQUENTIAL:
                return (index == triggerSequencialIndex);
            case TriggerPattern.RANDOM_SINGLE:
                return triggerRandomiser.Next(controlledObjects.Length) == index;
            case TriggerPattern.RANDOM_MULTI:
                return triggerRandomIndices.Contains(index);
        }
        return false;
    }

    private void GenerateRandomIndices(int count)
    {
        triggerRandomIndices.Clear();
        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, controlledObjects.Length);
            if (!triggerRandomIndices.Contains(randomIndex))
            {
                triggerRandomIndices.Add(randomIndex);
            }
        }
    }
}
