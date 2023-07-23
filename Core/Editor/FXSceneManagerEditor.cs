using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace FX
{
    [CustomEditor(typeof(FXSceneManager))]
    public class FXSceneManagerEditor : Editor
    {
        private FXSceneManager fxSceneManager;
        private int selectedPresetIndex = 0;

        private void OnEnable()
        {
            fxSceneManager = (FXSceneManager)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);

            // Save Preset Section
            GUILayout.Label("Current Preset", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 60;
            fxSceneManager.currentPresetName = EditorGUILayout.TextField("Name", fxSceneManager.currentPresetName);
            EditorGUIUtility.labelWidth = 0;

            if (GUILayout.Button("Save", GUILayout.Width(70)))
            {
                SavePreset();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            // Load Preset Section
            GUILayout.Label("Load Preset", EditorStyles.boldLabel);
            if (fxSceneManager.presets.Count > 0)
            {
                selectedPresetIndex = EditorGUILayout.Popup("Select Preset", selectedPresetIndex, fxSceneManager.presets.ToArray());
                if (GUILayout.Button("Load", GUILayout.Width(70)))
                {
                    LoadPreset();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No presets found.", MessageType.Info);
            }
        }

        private void SavePreset()
        {
            if (string.IsNullOrEmpty(fxSceneManager.currentPresetName))
            {
                Debug.LogError("Preset name cannot be empty.");
                return;
            }

            FXManager.Instance.SavePreset(fxSceneManager.currentPresetName);
        }

        private void LoadPreset()
        {
            if (selectedPresetIndex < 0 || selectedPresetIndex >= fxSceneManager.presets.Count)
            {
                Debug.LogError("Invalid preset selection.");
                return;
            }

            string selectedPresetName = fxSceneManager.presets[selectedPresetIndex];            
            fxSceneManager.LoadPreset(selectedPresetName);
            fxSceneManager.currentPresetName = selectedPresetName;
        }
    }
}
