using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[VolumeComponentMenu("FX/FXWarper")]
public class FXPostProcessWarper : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    public TextureParameter warpTexture = new TextureParameter(null);
    public TextureParameter blendTexture = new TextureParameter(null);

    private Material material;
    private const string kShaderName = "Hidden/FX/Warp";
    public Texture2D warpTex;

    public bool IsActive()
    {
        return true;
        bool isActive = warpTexture.value != null && blendTexture.value != null;
        Debug.Log($"IsActive called: Warp Texture is null: {warpTexture.value == null}, Blend Texture is null: {blendTexture.value == null}, IsActive: {isActive}");
        return isActive;
    }

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find(kShaderName) != null)
        {
            material = new Material(Shader.Find(kShaderName));
            Debug.Log($"Shader {kShaderName} found and material created.");
        }
        else
        {
            Debug.LogError($"Unable to find shader '{kShaderName}'. The post-process effect will not be applied.");
            return;
        }

        // Load textures at runtime
        LoadAndSetTextures();
    }

    private void LoadAndSetTextures()
    {
        string programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        string warpPath = Path.Combine(programDataPath, "Igloo Vision\\IglooCoreEngine\\warping\\Screen-warp32.bmp");
        string blendPath = Path.Combine(programDataPath, "Igloo Vision\\IglooCoreEngine\\warping\\Screen-edgeBlend.png");

        int tpWidth = 0;
        int tpHeight = 0;

        using (FileStream fs = new FileStream(warpPath, FileMode.Open, FileAccess.Read))
        {
            byte[] header = new byte[54];
            fs.Read(header, 0, 54);
            tpWidth = BitConverter.ToInt32(header, 18);
            tpHeight = BitConverter.ToInt32(header, 22);
        }

        warpTex = LoadFloatTexture(warpPath, tpWidth, tpHeight, 54);
        warpTexture.value = warpTex;
        blendTexture.value = LoadTexture(blendPath);

        // Mark the textures as overridden
        warpTexture.overrideState = true;
        blendTexture.overrideState = true;

        Debug.Log($"Warp Texture set: {warpTexture.value != null}");
        Debug.Log($"Blend Texture set: {blendTexture.value != null}");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        Debug.Log("Render method called");

        if (!IsActive())
        {
            Debug.Log("Render method early exit because IsActive() returned false");
            return;
        }

        

        if (warpTexture.value != null)
        {
            material.SetTexture("_TextureWarp", warpTexture.value);
        }

        if (blendTexture.value != null)
        {
            material.SetTexture("_TextureBlend", blendTexture.value);
        }

        material.SetTexture("_MainTex", source);

        HDUtils.DrawFullScreen(cmd, material, destination, null, 0);
    }

    public static Texture2D LoadFloatTexture(string path, int w, int h, int ignoreBytes = 0)
    {
        Debug.Log("<b>[Igloo]</b> About to attempt loading float texture, path: " + path + " ,w: " + w + " h: " + h);
        int texWidth = w;
        int texHeight = h;
        Texture2D tex = null;

        if (!File.Exists(path))
        {
            Debug.LogWarning("<b>[Igloo]</b> Cannot find Igloo Warp Image at: " + path);
            return tex;
        }

        byte[] sArray = File.ReadAllBytes(path);

        float[] dArray = new float[(sArray.Length - ignoreBytes) / 4];

        Debug.Log("sArray size: " + sArray.Length + "  dArray size: " + dArray.Length);

        Buffer.BlockCopy(sArray, ignoreBytes, dArray, 0, sArray.Length - ignoreBytes);

        int bArrayPos = 0;
        int pixelPos = 0;

        Color[] pixels = new Color[texWidth * texHeight];

        for (int y = 0; y < texHeight; y++)
        {
            for (int x = 0; x < texWidth; x++)
            {
                float r = dArray[bArrayPos];
                float g = 1 - dArray[bArrayPos + 1];
                float b = dArray[bArrayPos + 2];
                float a = dArray[bArrayPos + 3];

                Color pixel = new Color(r, g, b, a);

                pixels[pixelPos] = pixel;
                bArrayPos += 4;
                pixelPos++;
            }
        }

        tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBAFloat, false);
        tex.SetPixels(pixels);
        tex.Apply();

        return tex;
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(material);
    }

    private Texture2D LoadTexture(string path)
    {
        byte[] fileData;
        Texture2D tex = null;
        if (File.Exists(path))
        {
            fileData = File.ReadAllBytes(path);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }
        return tex;
    }
}
