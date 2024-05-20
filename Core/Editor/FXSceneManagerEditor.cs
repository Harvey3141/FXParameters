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

                // Determine the type of the first tag in the group
                string tagType = group.First().Value.GetType().Name.ToLower();

                // Create the appropriate input field based on the tag type
                switch (tagType)
                {
                    case "string":
                        newTagValues[group.Key] = EditorGUILayout.TextField(newTagValues[group.Key], GUILayout.Width(100));
                        break;
                    case "int32":
                        if (int.TryParse(newTagValues[group.Key], out int intValue))
                        {
                            newTagValues[group.Key] = EditorGUILayout.IntField(intValue, GUILayout.Width(100)).ToString();
                        }
                        else
                        {
                            newTagValues[group.Key] = EditorGUILayout.IntField(0, GUILayout.Width(100)).ToString();
                        }
                        break;
                    case "single": // float type
                        if (float.TryParse(newTagValues[group.Key], out float floatValue))
                        {
                            newTagValues[group.Key] = EditorGUILayout.FloatField(floatValue, GUILayout.Width(100)).ToString();
                        }
                        else
                        {
                            newTagValues[group.Key] = EditorGUILayout.FloatField(0f, GUILayout.Width(100)).ToString();
                        }
                        break;
                    case "boolean":
                        if (bool.TryParse(newTagValues[group.Key], out bool boolValue))
                        {
                            newTagValues[group.Key] = EditorGUILayout.Toggle(boolValue, GUILayout.Width(100)).ToString();
                        }
                        else
                        {
                            newTagValues[group.Key] = EditorGUILayout.Toggle(false, GUILayout.Width(100)).ToString();
                        }
                        break;
                    default:
                        newTagValues[group.Key] = EditorGUILayout.TextField(newTagValues[group.Key], GUILayout.Width(100));
                        break;
                }

                if (GUILayout.Button("Add", GUILayout.Width(70)))
                {
                    bool success = false;
                    switch (tagType)
                    {
                        case "string":
                            success = fxSceneManager.AddTagToSystem(group.Key, newTagValues[group.Key]);
                            break;
                        case "int32":
                            if (int.TryParse(newTagValues[group.Key], out int intValue))
                            {
                                success = fxSceneManager.AddTagToSystem(group.Key, intValue);
                            }
                            break;
                        case "single":
                            if (float.TryParse(newTagValues[group.Key], out float floatValue))
                            {
                                success = fxSceneManager.AddTagToSystem(group.Key, floatValue);
                            }
                            break;
                        case "boolean":
                            if (bool.TryParse(newTagValues[group.Key], out bool boolValue))
                            {
                                success = fxSceneManager.AddTagToSystem(group.Key, boolValue);
                            }
                            break;
                        default:
                            success = fxSceneManager.AddTagToSystem(group.Key, newTagValues[group.Key]);
                            break;
                    }

                    if (success)
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
                    var tag = group.First();
                    bool success = false;

                    // Add the tag to the scene based on its type
                    if (tag.Value is string)
                    {
                        success = fxSceneManager.AddTagToScene(fxSceneManager.CurrentScene.Name, group.Key, selectedTagValue);
                    }
                    else if (tag.Value is int)
                    {
                        if (int.TryParse(selectedTagValue, out int intValue))
                        {
                            success = fxSceneManager.AddTagToScene(fxSceneManager.CurrentScene.Name, group.Key, intValue);
                        }
                    }
                    else if (tag.Value is float)
                    {
                        if (float.TryParse(selectedTagValue, out float floatValue))
                        {
                            success = fxSceneManager.AddTagToScene(fxSceneManager.CurrentScene.Name, group.Key, floatValue);
                        }
                    }
                    else if (tag.Value is bool)
                    {
                        if (bool.TryParse(selectedTagValue, out bool boolValue))
                        {
                            success = fxSceneManager.AddTagToScene(fxSceneManager.CurrentScene.Name, group.Key, boolValue);
                        }
                    }

                    if (success)
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
