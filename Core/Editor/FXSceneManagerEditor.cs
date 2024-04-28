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

            if (GUILayout.Button("New Scene..", GUILayout.Width(200)))
            {
                fxSceneManager.CreateNewScene();
            }

            GUILayout.Space(10);

            GUILayout.Label("Current Scene", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 60;
            fxSceneManager.CurrentPresetName = EditorGUILayout.TextField("Name", fxSceneManager.CurrentPresetName);
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
                    string presetName = fxSceneManager.presets[i];
                    if (presetName != "ParameterList") {
                        GUILayout.BeginHorizontal();

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

            }
            else
            {
                EditorGUILayout.HelpBox("No presets found.", MessageType.Info);
            }


            EditorGUILayout.EndScrollView();


            if (GUILayout.Button("Export", GUILayout.Width(70)))
            {
                fxSceneManager.ExportParameterList();
            }

        }

        private void SavePreset()
        {
            if (string.IsNullOrEmpty(fxSceneManager.CurrentPresetName))
            {
                Debug.LogError("Preset name cannot be empty.");
                return;
            }

            FXManager.Instance.SavePreset(fxSceneManager.CurrentPresetName);
            fxSceneManager.PopulatePresetsList(); 
        }

        private void LoadPreset(string presetName)
        {
            bool loadOK = fxSceneManager.LoadPreset(presetName);
            if (loadOK) fxSceneManager.CurrentPresetName = presetName;
        }

        private void RemovePreset(string presetName)
        {
            fxSceneManager.RemovePreset(presetName);
        }

    }
}
