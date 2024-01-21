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

public static class MaterialTypeInfo
{
    public static int Count { get { return Enum.GetNames(typeof(MaterialType)).Length; } }
}

public class FXMaterialController : FXGroupObjectController, IFXTriggerable
{
    public FXParameter<int> materialIndex = new FXParameter<int>(0);

    public FXScaledParameter<float> emmissiveIntensity = new FXScaledParameter<float>(0.0f,0.0f,1.0f);

    public Material Default;
    public Material Emissive;
    public Material Concrete;
    public Material RustyConcrete;

    protected override void Awake()
    {
        base.Awake();
        materialIndex.OnValueChanged += SetMaterial;
        emmissiveIntensity.OnScaledValueChanged += SetEmissiveIntensityAll;
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
                SetEmissiveIntensityAll(emmissiveIntensity.ScaledValue);
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

    void SetEmissiveIntensityAll(float intensity) { 
        for (int i = 0; i < controlledObjects.Length; i++)
        {
            SetEmissiveIntensityAtIndex(i, intensity);
        }
    }

    void SetEmissiveIntensityAtIndex(int index, float intensity)
    {
        if ((MaterialType)materialIndex.Value != MaterialType.EMISSIVE) return;

        GameObject obj = controlledObjects[index];
        if (obj != null)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
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

    protected override void SetLerpValueToObject(int index, float value)
    {
        SetEmissiveIntensityAtIndex(index, value);        
    }


    public override void FXTrigger() {
        base.FXTrigger();
    }

}
