using UnityEditor;
using UnityEngine;

namespace FX
{
    [CustomEditor(typeof(FXSceneManager))]
    public class FXSceneManagerEditor : Editor
    {
        private FXSceneManager fxSceneManager;
        private Vector2 scrollPosition;

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
            GUILayout.Label("Presets", EditorStyles.boldLabel);

            // Display the presets in a scrollable list box
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 10));

            if (fxSceneManager.presets.Count > 0)
            {
                for (int i = 0; i < fxSceneManager.presets.Count; i++)
                {
                    GUILayout.BeginHorizontal();

                    string presetName = fxSceneManager.presets[i];

                    if (GUILayout.Button(presetName))
                    {
                        LoadPreset(presetName);
                    }

                    // Add a remove button for each preset
                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    {
                        RemovePreset(presetName);
                    }

                    GUILayout.EndHorizontal();
                }

            }
            else
            {
                EditorGUILayout.HelpBox("No presets found.", MessageType.Info);
            }

            if (GUILayout.Button("Export Parameter List", GUILayout.Width(70)))
            {
                fxSceneManager.ExportParameterList();
            }

            EditorGUILayout.EndScrollView();


        }

        private void SavePreset()
        {
            if (string.IsNullOrEmpty(fxSceneManager.currentPresetName))
            {
                Debug.LogError("Preset name cannot be empty.");
                return;
            }

            FXManager.Instance.SavePreset(fxSceneManager.currentPresetName);
            fxSceneManager.PopulatePresetsList(); // Refresh the list after saving
        }

        private void LoadPreset(string presetName)
        {
            fxSceneManager.LoadPreset(presetName);
            fxSceneManager.currentPresetName = presetName;
        }

        private void RemovePreset(string presetName)
        {
            fxSceneManager.RemovePreset(presetName);
        }
    }
}
