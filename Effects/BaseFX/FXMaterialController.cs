using FX;
using System;
using UnityEditor;
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

public class FXMaterialController : FXBase
{
    public FXParameter<int> materialIndex = new FXParameter<int>(0);

    public FXScaledParameter<float> emmissiveIntensity = new FXScaledParameter<float>(0.0f,0.0f,1.0f);

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
        //Default = Resources.Load<Material>("FXParameters/Materials/FXDefault.mat");
        //Emissive = Resources.Load<Material>("FXParameters/Materials/FXEmissive.mat.mat");
        //Concrete = Resources.Load<Material>("FXParameters/Materials/Concrete_UP7178.mat");
        //RustyConcrete = Resources.Load<Material>("FXParameters/Materials/Concrete_UP7417.mat");

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
                    renderer.material = material;
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
        if (Emissive != null)
        {
            Color emissiveColorLDR = Emissive.GetColor("_EmissiveColorLDR");
            Color emissiveColor = new Color(Mathf.GammaToLinearSpace(emissiveColorLDR.r), Mathf.GammaToLinearSpace(emissiveColorLDR.g), Mathf.GammaToLinearSpace(emissiveColorLDR.b));
            Emissive.SetColor("_EmissiveColor", emissiveColor * intensity);
        }
    }
}
