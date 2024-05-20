using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace FX
{
    [CustomEditor(typeof(FXSceneManager))]
    public class FXSceneManagerEditor : Editor
    {
        private FXSceneManager fxSceneManager;
        private Vector2 scrollPosition;
        private Vector2 tagScrollPosition;

        private Dictionary<string, string> newTagValues = new Dictionary<string, string>();
        private Dictionary<string, int> selectedTagIndices = new Dictionary<string, int>();

        private int selectedTagConfigIndex = 0;
        private int selectedTagIndex = 0;

        private string newTagType = "";
        private string newTagValue = "";

        private void OnEnable()
        {
            fxSceneManager = (FXSceneManager)target;
        }

        public override void OnInspectorGUI()
        {
            if (!EditorApplication.isPlaying) return;

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

            if (fxSceneManager.CurrentScene == null)
            {
                fxSceneManager.CurrentScene = new Scene(string.Empty);
            }

            fxSceneManager.CurrentScene.Name = EditorGUILayout.TextField("Name", fxSceneManager.CurrentScene.Name);
            EditorGUIUtility.labelWidth = 0;

            if (GUILayout.Button("Save", GUILayout.Width(70)))
            {
                SaveScene();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.Label("Scenes", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 10));

            if (fxSceneManager.scenes.Count > 0)
            {
                foreach (Scene scene in fxSceneManager.scenes)
                {
                    if (scene.Name != "ParameterList")
                    {
                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button(scene.Name))
                        {
                            LoadScene(scene.Name);
                        }

                        if (GUILayout.Button("Remove", GUILayout.Width(70)))
                        {
                            RemoveScene(scene.Name);
                        }

                        GUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No scenes found.", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Export", GUILayout.Width(70)))
            {
                fxSceneManager.ExportParameterList();
            }

            GUILayout.Space(20);

            GUILayout.Label("Manage System Tags", EditorStyles.boldLabel);

            if (fxSceneManager.tagConfigurations.Count > 0)
            {
                string[] tagConfigTypes = fxSceneManager.tagConfigurations.Select(tc => tc.type).ToArray();

                GUILayout.BeginHorizontal();

                // Dropdown to select the tag configuration type
                selectedTagConfigIndex = EditorGUILayout.Popup(selectedTagConfigIndex, tagConfigTypes, GUILayout.Width(150));
                var selectedTagConfig = fxSceneManager.tagConfigurations[selectedTagConfigIndex];

                // Dropdown to select the tag within the selected tag configuration type
                string[] tagValues = selectedTagConfig.tags.Select(tag => tag.value).ToArray();
                selectedTagIndex = EditorGUILayout.Popup(selectedTagIndex, tagValues, GUILayout.Width(200));

                // Add the selected tag to the current scene
                if (GUILayout.Button($"Add Tag to Scene", GUILayout.Width(150)))
                {
                    var selectedTagId = selectedTagConfig.tags[selectedTagIndex].id;
                    if (fxSceneManager.AddTagToScene(fxSceneManager.CurrentScene.Name, selectedTagId))
                    {
                        Debug.Log($"Tag '{selectedTagConfig.tags[selectedTagIndex].value}' added to scene '{fxSceneManager.CurrentScene.Name}' under type '{selectedTagConfig.type}'.");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to add tag '{selectedTagConfig.tags[selectedTagIndex].value}' to scene '{fxSceneManager.CurrentScene.Name}'.");
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
            else
            {
                EditorGUILayout.HelpBox("No tag configurations found.", MessageType.Info);
            }

            GUILayout.Space(20);

            // Add new tag to configuration
            GUILayout.Label("Add New Tag to Configuration", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            newTagType = EditorGUILayout.TextField("Type", newTagType, GUILayout.Width(150));
            newTagValue = EditorGUILayout.TextField("Value", newTagValue, GUILayout.Width(150));

            if (GUILayout.Button("Add Tag", GUILayout.Width(100)))
            {
                if (!string.IsNullOrEmpty(newTagType) && !string.IsNullOrEmpty(newTagValue))
                {
                    if (fxSceneManager.AddTagToConfiguration(newTagType, newTagValue))
                    {
                        Debug.Log($"Tag '{newTagValue}' added to configuration under type '{newTagType}'.");
                        newTagType = "";
                        newTagValue = "";
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to add tag '{newTagValue}' to configuration under type '{newTagType}'.");
                    }
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            // Remove tag from configuration
            GUILayout.Label("Remove Tag from Configuration", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            selectedTagConfigIndex = EditorGUILayout.Popup(selectedTagConfigIndex, fxSceneManager.tagConfigurations.Select(tc => tc.type).ToArray(), GUILayout.Width(150));
            var selectedRemoveTagConfig = fxSceneManager.tagConfigurations[selectedTagConfigIndex];

            if (selectedRemoveTagConfig.tags.Count > 0)
            {
                selectedTagIndex = EditorGUILayout.Popup(selectedTagIndex, selectedRemoveTagConfig.tags.Select(tag => tag.value).ToArray(), GUILayout.Width(150));
                var selectedTagId = selectedRemoveTagConfig.tags[selectedTagIndex].id;

                if (GUILayout.Button("Remove Tag", GUILayout.Width(100)))
                {
                    if (fxSceneManager.RemoveTagFromConfiguration(selectedRemoveTagConfig.type, selectedTagId))
                    {
                        Debug.Log($"Tag '{selectedRemoveTagConfig.tags[selectedTagIndex].value}' removed from configuration under type '{selectedRemoveTagConfig.type}'.");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to remove tag '{selectedRemoveTagConfig.tags[selectedTagIndex].value}' from configuration under type '{selectedRemoveTagConfig.type}'.");
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No tags available for selected configuration.", MessageType.Info);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            // Display current scene tags
            GUILayout.Label("Current Scene Tags", EditorStyles.boldLabel);
            if (fxSceneManager.CurrentScene != null && fxSceneManager.CurrentScene.TagIds.Count > 0)
            {
                foreach (var tagId in fxSceneManager.CurrentScene.TagIds)
                {
                    var tag = fxSceneManager.tagConfigurations.SelectMany(tc => tc.tags).FirstOrDefault(t => t.id == tagId);
                    if (tag != null)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"{tag.value}", GUILayout.Width(200));
                        if (GUILayout.Button("Remove Tag", GUILayout.Width(100)))
                        {
                            if (fxSceneManager.RemoveTagFromScene(fxSceneManager.CurrentScene.Name, tag.id))
                            {
                                Debug.Log($"Tag '{tag.value}' removed from scene '{fxSceneManager.CurrentScene.Name}'.");
                            }
                            else
                            {
                                Debug.LogWarning($"Failed to remove tag '{tag.value}' from scene '{fxSceneManager.CurrentScene.Name}'.");
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No tags found for the current scene.", MessageType.Info);
            }
        }

        private void SaveScene()
        {
            fxSceneManager.SaveScene();
            fxSceneManager.PopulateScenesList();
        }

        private void LoadScene(string sceneName)
        {
            fxSceneManager.LoadScene(sceneName);
        }

        private void RemoveScene(string sceneName)
        {
            fxSceneManager.RemoveScene(sceneName);
        }
    }
}
