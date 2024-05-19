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

            // Ensure CurrentScene is initialized
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

            // Load Scene Section
            GUILayout.Label("Scenes", EditorStyles.boldLabel);

            // Display the scenes in a scrollable list box
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 10));

            if (fxSceneManager.scenes.Count > 0)
            {
                for (int i = 0; i < fxSceneManager.scenes.Count; i++)
                {
                    Scene scene = fxSceneManager.scenes[i];
                    if (scene.Name != "ParameterList")
                    {
                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button(scene.Name))
                        {
                            LoadScene(scene.Name);
                        }

                        // Add a remove button for each scene
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

            // Manage System Tags Section
            GUILayout.Label("Manage System Tags", EditorStyles.boldLabel);

            var groupedTags = fxSceneManager.tagList.GroupBy(tag => tag.Name).ToList();

            foreach (var group in groupedTags)
            {
                GUILayout.Label($"Type: {group.Key}", EditorStyles.boldLabel);
                tagScrollPosition = EditorGUILayout.BeginScrollView(tagScrollPosition, GUILayout.Height(50));

                foreach (var tag in group)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Value: {tag.Value}", GUILayout.Width(200));
                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    {
                        if (fxSceneManager.RemoveTagFromSystem(tag.Name, tag.Value))
                        {
                            Debug.Log($"Tag '{tag.Value}' removed from the system.");
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to remove tag '{tag.Value}' from the system.");
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
                GUILayout.Space(10);

                // Add new tag for this type
                if (!newTagValues.ContainsKey(group.Key))
                {
                    newTagValues[group.Key] = "";
                }
                GUILayout.BeginHorizontal();
                GUILayout.Label("Value", GUILayout.Width(40));
                newTagValues[group.Key] = EditorGUILayout.TextField(newTagValues[group.Key], GUILayout.Width(100));
                if (GUILayout.Button("Add", GUILayout.Width(70)))
                {
                    if (fxSceneManager.AddTagToSystem(group.Key, newTagValues[group.Key]))
                    {
                        Debug.Log($"Tag '{newTagValues[group.Key]}' added to the system under type '{group.Key}'.");
                        newTagValues[group.Key] = "";
                    }
                    else
                    {
                        Debug.LogWarning($"Tag '{newTagValues[group.Key]}' already exists in the system under type '{group.Key}'.");
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            GUILayout.Space(20);

            // Add tag to the current scene
            GUILayout.Label("Current Scene Tags", EditorStyles.boldLabel);

            foreach (var group in groupedTags)
            {
                GUILayout.Label($"Add {group.Key} Tags to Scene", EditorStyles.boldLabel);

                if (!selectedTagIndices.ContainsKey(group.Key))
                {
                    selectedTagIndices[group.Key] = 0;
                }

                string[] tagValues = group.Select(tag => tag.Value.ToString()).ToArray();
                GUILayout.BeginHorizontal();
                selectedTagIndices[group.Key] = EditorGUILayout.Popup(selectedTagIndices[group.Key], tagValues, GUILayout.Width(200));
                if (GUILayout.Button($"Add {group.Key} Tag to Scene", GUILayout.Width(150)))
                {
                    var selectedTagValue = tagValues[selectedTagIndices[group.Key]];
                    if (fxSceneManager.AddTagToScene(fxSceneManager.CurrentScene.Name, group.Key, selectedTagValue))
                    {
                        Debug.Log($"Tag '{selectedTagValue}' added to scene '{fxSceneManager.CurrentScene.Name}' under type '{group.Key}'.");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to add tag '{selectedTagValue}' to scene '{fxSceneManager.CurrentScene.Name}'.");
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            GUILayout.Space(10);

            // Display current scene tags
            GUILayout.Label("Current Scene Tags", EditorStyles.boldLabel);
            if (fxSceneManager.CurrentScene != null && fxSceneManager.CurrentScene.Tags.Count > 0)
            {
                foreach (var tag in fxSceneManager.CurrentScene.Tags)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{tag.Name}: {tag.Value}", GUILayout.Width(200));
                    if (GUILayout.Button("Remove Tag", GUILayout.Width(100)))
                    {
                        if (fxSceneManager.RemoveTagFromScene(fxSceneManager.CurrentScene.Name, tag.Name, tag.Value))
                        {
                            Debug.Log($"Tag '{tag.Value}' removed from scene '{fxSceneManager.CurrentScene.Name}'.");
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to remove tag '{tag.Value}' from scene '{fxSceneManager.CurrentScene.Name}'.");
                        }
                    }
                    GUILayout.EndHorizontal();
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

        private string[] GetUniqueTagTypes()
        {
            return fxSceneManager.tagList.Select(tag => tag.Name).Distinct().ToArray();
        }
    }
}
