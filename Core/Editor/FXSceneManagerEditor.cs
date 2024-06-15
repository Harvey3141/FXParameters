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

        private int selectedTagConfigIndex = 0;
        private int selectedTagIndex = 0;

        private string newTagValue = "";

        private string saveAsName = ""; 

        private string filterTagId = "";
        private int selectedFilterTagIndex = 0;
        private List<FX.Scene> filteredScenes;

        private void OnEnable()
        {
            fxSceneManager = (FXSceneManager)target;
            filteredScenes = new List<FX.Scene>();
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

            GUILayout.BeginHorizontal();
            saveAsName = EditorGUILayout.TextField("Save As", saveAsName);
            if (GUILayout.Button("Save As", GUILayout.Width(70)))
            {
                fxSceneManager.SaveCurrentSceneAs(saveAsName);
                saveAsName = ""; 
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.Label("Scenes", EditorStyles.boldLabel);

            if (fxSceneManager.tagConfigurations.Count > 0)
            {
                string[] tagConfigTypes = fxSceneManager.tagConfigurations.Select(tc => tc.type).ToArray();
                List<string> allTagValues = fxSceneManager.tagConfigurations.SelectMany(tc => tc.tags).Select(tag => tag.value).ToList();
                allTagValues.Insert(0, "All Scenes");

                int selectedFilterTagIndexBefore = selectedFilterTagIndex;
                GUILayout.BeginHorizontal();
                selectedFilterTagIndex = EditorGUILayout.Popup(selectedFilterTagIndex, allTagValues.ToArray(), GUILayout.Width(150));

                if (selectedFilterTagIndex == 0)
                {
                    filteredScenes = fxSceneManager.scenes;
                }
                else
                {
                    filterTagId = fxSceneManager.tagConfigurations.SelectMany(tc => tc.tags).ToArray()[selectedFilterTagIndex - 1].id;

                    if (selectedFilterTagIndexBefore != selectedFilterTagIndex)
                    {
                        filteredScenes = fxSceneManager.FilterScenesByTag(filterTagId);
                        Debug.Log($"Filtered {filteredScenes.Count} scenes with tag '{allTagValues[selectedFilterTagIndex]}'.");
                    }
                }
                GUILayout.EndHorizontal();

                DisplayScenes(filteredScenes);

                GUILayout.Space(10);
            }

            GUILayout.Space(20);

            //GUILayout.Label("Scenes", EditorStyles.boldLabel);
            //
            //scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 10));
            //
            //if (fxSceneManager.scenes.Count > 0)
            //{
            //    foreach (Scene scene in fxSceneManager.scenes)
            //    {
            //        if (scene.Name != "ParameterList")
            //        {
            //            GUILayout.BeginHorizontal();
            //
            //            if (GUILayout.Button(scene.Name))
            //            {
            //                LoadScene(scene.Name);
            //            }
            //
            //            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            //            {
            //                RemoveScene(scene.Name);
            //            }
            //
            //            GUILayout.EndHorizontal();
            //        }
            //    }
            //}
            //else
            //{
            //    EditorGUILayout.HelpBox("No scenes found.", MessageType.Info);
            //}
            //
            //EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Export", GUILayout.Width(70)))
            {
                fxSceneManager.ExportParameterList();
            }

            GUILayout.Space(20);

            GUILayout.Label("Scene Tags", EditorStyles.boldLabel);
            if (fxSceneManager.CurrentScene != null && fxSceneManager.CurrentScene.TagIds.Count > 0)
            {
                for (int i = fxSceneManager.CurrentScene.TagIds.Count - 1; i >= 0; i--)
                {
                    var tagId = fxSceneManager.CurrentScene.TagIds[i];
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

            GUILayout.Space(20);

            if (fxSceneManager.tagConfigurations.Count > 0)
            {
                string[] tagConfigTypes = fxSceneManager.tagConfigurations.Select(tc => tc.type).ToArray();

                GUILayout.BeginHorizontal();

                selectedTagConfigIndex = EditorGUILayout.Popup(selectedTagConfigIndex, tagConfigTypes, GUILayout.Width(150));
                var selectedTagConfig = fxSceneManager.tagConfigurations[selectedTagConfigIndex];

                string[] tagValues = selectedTagConfig.tags.Select(tag => tag.value).ToArray();
                selectedTagIndex = EditorGUILayout.Popup(selectedTagIndex, tagValues, GUILayout.Width(200));

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

            GUILayout.Label("Tag Configuration", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            selectedTagConfigIndex = EditorGUILayout.Popup(selectedTagConfigIndex, fxSceneManager.tagConfigurations.Select(tc => tc.type).ToArray(), GUILayout.Width(150));
            GUILayout.Label("value", GUILayout.Width(40));
            newTagValue = EditorGUILayout.TextField(newTagValue, GUILayout.Width(150));

            if (GUILayout.Button("Create Tag", GUILayout.Width(100)))
            {
                var selectedTagConfigType = fxSceneManager.tagConfigurations[selectedTagConfigIndex].type;
                if (!string.IsNullOrEmpty(newTagValue))
                {
                    if (fxSceneManager.AddTagToConfiguration(selectedTagConfigType, newTagValue))
                    {
                        Debug.Log($"Tag '{newTagValue}' added to configuration under type '{selectedTagConfigType}'.");
                        newTagValue = "";
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to add tag '{newTagValue}' to configuration under type '{selectedTagConfigType}'.");
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            selectedTagConfigIndex = EditorGUILayout.Popup(selectedTagConfigIndex, fxSceneManager.tagConfigurations.Select(tc => tc.type).ToArray(), GUILayout.Width(150));
            var selectedRemoveTagConfig = fxSceneManager.tagConfigurations[selectedTagConfigIndex];

            if (selectedRemoveTagConfig.tags.Count > 0)
            {
                selectedTagIndex = EditorGUILayout.Popup(selectedTagIndex, selectedRemoveTagConfig.tags.Select(tag => tag.value).ToArray(), GUILayout.Width(150));
                var selectedTagId = selectedRemoveTagConfig.tags[selectedTagIndex].id;

                if (GUILayout.Button("Delete Tag", GUILayout.Width(100)))
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
        }

        private void SaveScene()
        {
            fxSceneManager.SaveScene();
            fxSceneManager.PopulateScenesList();

            if (selectedFilterTagIndex == 0)
            {
                filteredScenes = fxSceneManager.scenes;
            }
            else
            {
                filteredScenes = fxSceneManager.FilterScenesByTag(filterTagId);
            }

            Repaint();
        }

        private void LoadScene(string sceneName)
        {
            fxSceneManager.LoadScene(sceneName);
        }

        private void RemoveScene(string sceneName)
        {
            fxSceneManager.RemoveScene(sceneName);
            fxSceneManager.PopulateScenesList();

            if (selectedFilterTagIndex == 0)
            {
                filteredScenes = fxSceneManager.scenes;
            }
            else
            {
                filteredScenes = fxSceneManager.FilterScenesByTag(filterTagId);
            }

            Repaint();

        }

        private void DisplayScenes(List<Scene> scenes)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 10));

            if (scenes.Count > 0)
            {
                foreach (Scene scene in scenes)
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
        }
    }
}
