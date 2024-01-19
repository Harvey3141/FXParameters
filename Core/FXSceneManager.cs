using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FX
{
    public class FXSceneManager : MonoBehaviour
    {
        [HideInInspector]
        public List<string> presets;

        [HideInInspector]
        public string currentPresetName;
        private void Awake()
        {
            presets = new List<string>();
            PopulatePresetsList();
        }

        public void PopulatePresetsList()
        {
            presets.Clear();
            string presetsFolderPath = Path.Combine(Application.streamingAssetsPath, "FX Presets");

            if (Directory.Exists(presetsFolderPath))
            {
                DirectoryInfo presetsDirectory = new DirectoryInfo(presetsFolderPath);
                FileInfo[] presetFiles = presetsDirectory.GetFiles("*.json");

                foreach (FileInfo file in presetFiles)
                {
                    string presetName = Path.GetFileNameWithoutExtension(file.Name);
                    presets.Add(presetName);
                }
            }
            else
            {
                Debug.LogError("Presets folder not found: " + presetsFolderPath);
            }
        }

        public bool LoadPreset(string name)
        {
            return FXManager.Instance.LoadPreset(name);

        }

        public void SavePreset(string name)
        {
            FXManager.Instance.SavePreset(name);
            PopulatePresetsList();

        }

        public void RemovePreset(string name)
        {
            string presetsFolderPath = Path.Combine(Application.streamingAssetsPath, "FX Presets");
            string presetPath = Path.Combine(presetsFolderPath, name + ".json");
            string metaPath = Path.Combine(presetPath + ".meta");


            if (File.Exists(presetPath))
            {
                File.Delete(presetPath);
                PopulatePresetsList();

                if (File.Exists(metaPath))
                {
                    File.Delete(metaPath);
                }
            }
            else
            {
                Debug.LogError("Preset not found: " + name);
            }
        }
    }
}

