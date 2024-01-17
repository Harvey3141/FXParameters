using UnityEngine;

public enum MaterialType
{
    Default,
    Lit,
    Wireframe
}

public class FXMaterialController : FXBase
{
    // Array of GameObjects to control
    public GameObject[] controlledObjects;

    public Material FXDefault;
    public Material FXLit;
    public Material FXWireframe;

    private void ApplyMaterial(Material material)
    {
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
    }

    public void SwitchMaterial(MaterialType type)
    {
        switch (type)
        {
            case MaterialType.Default:
                ApplyMaterial(FXDefault);
                break;
            case MaterialType.Lit:
                ApplyMaterial(FXLit);
                break;
            case MaterialType.Wireframe:
                ApplyMaterial(FXWireframe);
                break;
        }
    }

    protected override void OnFXEnabled(bool state)
    {

    }
}
