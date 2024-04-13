using UnityEngine;
using System.IO;

public class CameraConfigManager : MonoBehaviour
{
    public Matrix4x4 projectionMatrix;
    public Vector3 position;
    public Quaternion rotation;

    private string settingsPath;

    void Start()
    {
        settingsPath = Path.Combine(Application.streamingAssetsPath, "FX/cameraSettings.json");
        LoadSettings();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SaveSettings();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            LoadSettings();
        }
    }

    void SaveSettings()
    {
        CameraData data = new CameraData
        {
            projectionMatrix = GetComponent<Camera>().projectionMatrix,
            position = transform.position,
            rotation = transform.rotation
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(settingsPath, json);

        Debug.Log("Settings saved to " + settingsPath);
    }

    void LoadSettings()
    {
        if (File.Exists(settingsPath))
        {
            string json = File.ReadAllText(settingsPath);
            CameraData data = JsonUtility.FromJson<CameraData>(json);

            GetComponent<Camera>().projectionMatrix = data.projectionMatrix;
            transform.position = data.position;
            transform.rotation = data.rotation;

            Debug.Log("Settings loaded from " + settingsPath);
        }
        else
        {
            Debug.LogError("No settings file found.");
        }
    }

    [System.Serializable]
    public class CameraData
    {
        public Matrix4x4 projectionMatrix;
        public Vector3 position;
        public Quaternion rotation;
    }
}
