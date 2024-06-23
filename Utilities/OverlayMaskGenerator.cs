using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class OverlayMaskGenerator : MonoBehaviour
{
    public Camera cam;
    public RenderTexture renderTexture;
    public TextureFormat format = TextureFormat.RGB24;
    public SetActiveLayer setActiveLayer;
    public Material whiteMat;
    public GameObject wall;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine(GenerateMask());
        }
    }

    private IEnumerator GenerateMask()
    {
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
            cam.targetTexture = renderTexture;
        }
        yield return null;


        Color colourBefore = cam.backgroundColor;
        Material materialBefore = wall.GetComponent<Renderer>().material;
        wall.GetComponent<Renderer>().material = whiteMat;
        cam.backgroundColor = new Color(0.0f, 0.0f, 0.0f);
        setActiveLayer.SetActive(1);
        yield return null;

        SaveRenderTextureToDisk(renderTexture);
        setActiveLayer.ActivateAll();
        cam.backgroundColor = colourBefore;
        cam.targetTexture = null;
        wall.GetComponent<Renderer>().material = materialBefore;
        yield return null; 
    }


    private void SaveRenderTextureToDisk(RenderTexture renderTexture)
    {
        Texture2D texture2D = ConvertToTexture2D(renderTexture);

        byte[] bytes = texture2D.EncodeToPNG();
        Object.Destroy(texture2D);

        string directoryPath = Path.Combine(Application.streamingAssetsPath, "FX"); ;
        string filePath = Path.Combine(directoryPath,"overlay-mask.png");

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
