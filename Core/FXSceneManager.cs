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
        private string currentPresetName;
        public string CurrentPresetName
        {
            get => currentPresetName;
            set
            {
                if (currentPresetName != value)
                {
                    currentPresetName = value;
                    if (onCurrentPresetNameChanged != null) onCurrentPresetNameChanged?.Invoke(currentPresetName);
                }
            }
        }


        public delegate void OnPresetListUpdated(List<string> presets);
        public event OnPresetListUpdated onPresetListUpdated;

        public delegate void OnCurrentPresetNameChanged(string newName);
        public event OnCurrentPresetNameChanged onCurrentPresetNameChanged;


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
                    if (file.Name != "ParameterList") {
                        string presetName = Path.GetFileNameWithoutExtension(file.Name);
                        presets.Add(presetName);
                    }
                }
                if (onPresetListUpdated != null) onPresetListUpdated.Invoke(presets);
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

        public void SavePreset()
        {
            if (!string.IsNullOrEmpty(CurrentPresetName)) SavePreset(CurrentPresetName);
        }

        public void SavePreset(string name)
        {
            FXManager.Instance.SavePreset(name);
            PopulatePresetsList();
        }

        public void ExportParameterList()
        {
            FXManager.Instance.SavePreset("ParameterList",true);
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

