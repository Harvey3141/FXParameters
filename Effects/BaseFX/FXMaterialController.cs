using FX;
using System;
using UnityEngine;

public enum MaterialType
{
    DEFAULT,
    EMISSIVE,
    CONCRETE,
    RUSTY_CONCRETE,
    DISSOLVE
}
public enum DissolveType
{
    ONE,
    TWO,
    THREE,
}


public static class MaterialTypeInfo
{
    public static int Count { get { return Enum.GetNames(typeof(MaterialType)).Length; } }
}

public class FXMaterialController : FXGroupObjectController, IFXTriggerable
{
    public FXParameter<MaterialType> materialType = new FXParameter<MaterialType>(MaterialType.DEFAULT);
    public Material Default;
    public Material Emissive;
    public Material Concrete;
    public Material RustyConcrete;
    public Material Disolve;

    public FXParameter<Color> color = new FXParameter<Color>(Color.white);
    public FXScaledParameter<float> dissolveEdgeWidth = new FXScaledParameter<float>(0.0f, -12.0f, 0.0f);
    public FXScaledParameter<float> dissolveOffset = new FXScaledParameter<float>(0.0f, 0.0f, 1.0f);
    public FXParameter<DissolveType> dissolveType = new FXParameter<DissolveType>(DissolveType.ONE);


    protected override void Awake()
    {
        base.Awake();
        materialType.OnValueChanged += SetMaterial;
        triggerValue.OnScaledValueChanged += SetEmissiveIntensityAll;

        dissolveEdgeWidth.OnScaledValueChanged += SetDissolveEdgeWidth;
        dissolveOffset.OnScaledValueChanged += SetDissolveOffset;
        color.OnValueChanged += SetColor;
    }

    protected override void Start()
    {
        SetMaterial(MaterialType.DEFAULT);
    }

    private void Update()
    {
        switch (materialType.Value)
        {
            case MaterialType.DEFAULT:
                break;
            case MaterialType.EMISSIVE:
                break;
            case MaterialType.CONCRETE:
                break;
            case MaterialType.RUSTY_CONCRETE:
                break;
            case MaterialType.DISSOLVE:
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
                SetEmissiveIntensityAll(triggerValue.ScaledValue);
                break;
            case MaterialType.CONCRETE:
                ApplyMaterial(Concrete);
                break;
            case MaterialType.RUSTY_CONCRETE:
                ApplyMaterial(RustyConcrete);
                break;
            case MaterialType.DISSOLVE:
                ApplyMaterial(Disolve);
                SetEmissiveIntensityAll(triggerValue.ScaledValue);
                SetDissolveEdgeWidth(dissolveEdgeWidth.ScaledValue);
                SetDissolveOffset(dissolveOffset.ScaledValue);
                break;
            default:
                break;
        }
        SetColor(color.Value);
    }

    void SetEmissiveIntensityAll(float intensity) { 
        for (int i = 0; i < controlledObjects.Length; i++)
        {
            SetEmissiveIntensityAtIndex(i, intensity);
        }
    }

    void SetEmissiveIntensityAtIndex(int index, float intensity)
    {
        GameObject obj = controlledObjects[index];
        if (obj != null)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                switch (materialType.Value)
                {
                    case MaterialType.EMISSIVE:
                        //Color emissiveColorLDR = renderer.material.GetColor("_EmissiveColorLDR");
                        //Color emissiveColor = new Color(Mathf.GammaToLinearSpace(emissiveColorLDR.r), Mathf.GammaToLinearSpace(emissiveColorLDR.g), Mathf.GammaToLinearSpace(emissiveColorLDR.b));
                        renderer.material.SetColor("_EmissiveColor", color.Value * intensity);
                        
                        break;
                    case MaterialType.DISSOLVE:
                            renderer.material.SetFloat("_EdgeColorIntensity", intensity);                       
                        break;
                    default:
                        break;
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

    void SetEmissiveColor(Color value)
    {
        if (materialType.Value != MaterialType.EMISSIVE) return;

        for (int i = 0; i < controlledObjects.Length; i++)
        {
            GameObject obj = controlledObjects[i];
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color emissiveColorLDR = renderer.material.GetColor("_EmissiveColorLDR");
                    Color emissiveColor = new Color(Mathf.GammaToLinearSpace(value.r), Mathf.GammaToLinearSpace(value.g), Mathf.GammaToLinearSpace(value.b));
                    renderer.material.SetColor("_EmissiveColor", emissiveColor);                    
                }
            }
        }
    }


    void SetDissolvePropertyFloat(string name, float value)
    {
        for (int i = 0; i < controlledObjects.Length; i++)
        {
            GameObject obj = controlledObjects[i];
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (Disolve != null)
                    {
                        renderer.material.SetFloat(name, value);
                    }                   
                }
            }
        }
    }

    void SetDissolvePropertyVector(string name, Vector4 value)
    {
        for (int i = 0; i < controlledObjects.Length; i++)
        {
            GameObject obj = controlledObjects[i];
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (Disolve != null)
                    {
                        renderer.material.SetVector(name, value);
                    }
                }
            }
        }
    }

    void SetDissolvePropertyColor(string name, Color value)
    {
        if (materialType.Value != MaterialType.DISSOLVE) return;

        for (int i = 0; i < controlledObjects.Length; i++)
        {
            GameObject obj = controlledObjects[i];
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {

                    //renderer.material.SetColor(name, value);
                    // Color emissiveColorLDR = renderer.material.GetColor(name);
                    // emissiveColor = new Color(Mathf.GammaToLinearSpace(emissiveColorLDR.r), Mathf.GammaToLinearSpace(emissiveColorLDR.g), Mathf.GammaToLinearSpace(emissiveColorLDR.b));
                    renderer.material.SetColor(name, value * Mathf.GammaToLinearSpace(6));
                    
                }
            }
        }
    }

    void SetColor(Color color)
    {
        SetDissolvePropertyColor("_EdgeColor", color);
        SetEmissiveColor(color);
    }


    void SetDissolveEdgeWidth(float value) {
        SetDissolvePropertyFloat("_DierctionEdgeWidthScale", value);
    }

    void SetDissolveOffset(float value)
    {
        Vector3 direction = new Vector3(-90, 0, 0);
        Vector3 offset = new Vector3(0, 0, value);

        switch (dissolveType.Value) {
            case DissolveType.ONE:
                direction = new Vector3(-90, 0, 0);
                offset = new Vector3(0, 0, value);

                SetDissolvePropertyVector("_DissolveOffest", offset);
                SetDissolvePropertyVector("_DissolveDirection", direction);

                break;
            case DissolveType.TWO:
                direction = new Vector3(90, 0, 0);
                offset = new Vector3(0, 0, value);
                SetDissolvePropertyVector("_DissolveOffest", offset);
                SetDissolvePropertyVector("_DissolveDirection", direction);
                break;
            case DissolveType.THREE:
                direction = new Vector3(180, 0, 0);
                offset = new Vector3(0, value*3.35f, 0);
                SetDissolvePropertyVector("_DissolveOffest", offset);
                SetDissolvePropertyVector("_DissolveDirection", direction);
                break;

        }
       
    }

}
