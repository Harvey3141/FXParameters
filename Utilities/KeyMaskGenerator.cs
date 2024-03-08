using UnityEngine;
using System.IO;

public class KeyMaskGenerator : MonoBehaviour
{
    public RenderTexture renderTexture;
    public TextureFormat format = TextureFormat.RGB24;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) SaveRenderTextureToDisk(renderTexture);

    }

    private void SaveRenderTextureToDisk(RenderTexture renderTexture)
    {
        Texture2D texture2D = ConvertToTexture2D(renderTexture);

        byte[] bytes = texture2D.EncodeToPNG();
        Object.Destroy(texture2D);

        string directoryPath = Path.Combine(Application.streamingAssetsPath, "FX"); ;
        string filePath = Path.Combine(directoryPath, gameObject.name + "-mask.png");

        string path = Path.Combine(Application.persistentDataPath, filePath);
        File.WriteAllBytes(path, bytes);

        Debug.Log("Saved RenderTexture to " + path);
    }

    private Texture2D ConvertToTexture2D(RenderTexture renderTexture)
    {
        Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, format, false);
        RenderTexture currentActiveRT = RenderTexture.active;

        RenderTexture.active = renderTexture;

        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentActiveRT;
        return texture2D;
    }
}
