using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[VolumeComponentMenu("Custom/FXCompositor")]
public class FXPostProcessCompositor : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    public TextureParameter textureA   = new TextureParameter(null);
    public TextureParameter textureB   = new TextureParameter(null);
    public TextureParameter textureKey = new TextureParameter(null);


    private Material material;
    private const string kShaderName = "Hidden/FX/CameraCompositor";

    public bool IsActive() => textureA.value != null && textureB.value != null;

    public override CustomPostProcessInjectionPoint injectionPoint =>
        CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find(kShaderName) != null)
            material = new Material(Shader.Find(kShaderName));
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. The post-process effect will not be applied.");
    }

    // Render the effect
    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (!IsActive()) {
            return;

        }

        if (textureA.value != null)
        {
            material.SetTexture("_TextureA", textureA.value);
        }
        if (textureB.value != null)
        {
            material.SetTexture("_TextureB", textureB.value);
        }
        if (textureKey.value != null)
        {
            material.SetTexture("_TextureKey", textureKey.value);
        }
        HDUtils.DrawFullScreen(cmd, material, destination, null, 0);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(material);
    }
}
