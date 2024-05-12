using FX.Patterns;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FX
{
    public class FXSceneManager : MonoBehaviour
    {

        FXManager fXManager;
        [HideInInspector]
        public List<string> presets;
        
        public bool exportParameterListOnStart = false;

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

        public delegate void OnPresetRemoved(string newName);
        public event OnPresetRemoved onPresetRemoved;


        private void Awake()
        {
            fXManager = FXManager.Instance;
            presets = new List<string>();
            PopulatePresetsList();
        }

        private void Start()
        {

            FXGroupData g = new FXGroupData();
            g.label = "Default";
            g.isPinned = true;
            g.signalSource = GroupFXController.SignalSource.Default;
            fXManager.CreateGroup(g);

            g = new FXGroupData();
            g.label = "Audio - Low";
            g.isPinned = true;
            g.signalSource = GroupFXController.SignalSource.Audio;
            g.audioFrequency = GroupFXController.AudioFrequency.Low;
            fXManager.CreateGroup(g);

            g = new FXGroupData();
            g.label = "Audio - Mid";
            g.isPinned = true;
            g.signalSource = GroupFXController.SignalSource.Audio;
            g.audioFrequency = GroupFXController.AudioFrequency.Mid;
            fXManager.CreateGroup(g);

            g = new FXGroupData();
            g.label = "Audio - High";
            g.isPinned = true;
            g.signalSource = GroupFXController.SignalSource.Audio;
            g.audioFrequency = GroupFXController.AudioFrequency.High;
            fXManager.CreateGroup(g);

            g = new FXGroupData();
            g.label = "Oscillator - Sine";
            g.isPinned = true;
            g.signalSource = GroupFXController.SignalSource.Pattern;
            g.patternType = GroupFXController.PatternType.Oscillator;
            g.oscillatorType = OscillatorPattern.OscillatorType.Sine;
            fXManager.CreateGroup(g);

            g = new FXGroupData();
            g.label = "Oscillator - Square";
            g.isPinned = true;
            g.signalSource = GroupFXController.SignalSource.Pattern;
            g.patternType = GroupFXController.PatternType.Oscillator;
            g.oscillatorType = OscillatorPattern.OscillatorType.Square;
            fXManager.CreateGroup(g);

            g = new FXGroupData();
            g.label = "Tap";
            g.isPinned = true;
            g.signalSource = GroupFXController.SignalSource.Pattern;
            g.patternType = GroupFXController.PatternType.Tap;
            g.numBeats = 1;
            fXManager.CreateGroup(g);

            g = new FXGroupData();
            g.label = "Arpeggiator";
            g.isPinned = true;
            g.signalSource = GroupFXController.SignalSource.Pattern;
            g.patternType = GroupFXController.PatternType.Arpeggiator;
            g.numBeats = 1;
            fXManager.CreateGroup(g);

            if (exportParameterListOnStart) ExportParameterList();

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
            if (fXManager.LoadPreset(name))
            {
                CurrentPresetName = name;
                return true;
            }
            return false;

        }

        public void SavePreset()
        {
            if (!string.IsNullOrEmpty(CurrentPresetName)) SavePreset(CurrentPresetName);
        }

        public void SavePreset(string name)
        {
            fXManager.SavePreset(name);
            PopulatePresetsList();
        }

        public void ExportParameterList()
        {
            fXManager.SavePreset("ParameterList",true);
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

        public void ResetCurrentScene()
        {
            if (!string.IsNullOrEmpty(currentPresetName)) LoadPreset(currentPresetName);
        }

        public void CreateNewScene()
        {
            GroupFXController[] allGroups = GameObject.FindObjectsOfType<GroupFXController>();

            foreach (var group in allGroups)
            {
                if (!group.isPinned)
                {
                    GameObject.Destroy(group.gameObject);
                }
                else
                {
                    group.ClearFXAdresses();
                }
            }

            fXManager.ResetAllParamsToDefault();
        }


    }
}

