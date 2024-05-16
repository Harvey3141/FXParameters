using UnityEditor;
using UnityEngine;
using System.Linq;

namespace FX
{
    [CustomEditor(typeof(FXSceneManager))]
    public class FXSceneManagerEditor : Editor
    {
        private FXSceneManager fxSceneManager;
        private Vector2 scrollPosition;

        private string newSystemTagName = "";
        private int selectedTagTypeIndex = 0;
        private string tagNameToRemove = "";
        private string tagTypeToRemove = "";
        private Vector2 tagScrollPosition;

        private int selectedTagIndex = 0;

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

            if (fxSceneManager.CurrentSceneName == null)
            {
                fxSceneManager.CurrentSceneName = string.Empty;
            }

            fxSceneManager.CurrentSceneName = EditorGUILayout.TextField("Name", fxSceneManager.CurrentSceneName);
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

            tagScrollPosition = EditorGUILayout.BeginScrollView(tagScrollPosition, GUILayout.Height(150));

            foreach (var tag in fxSceneManager.tagList)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Type: {tag.Type}, Name: {tag.Name}", GUILayout.Width(300));
                if (GUILayout.Button("Remove Tag", GUILayout.Width(100)))
                {
                    if (fxSceneManager.RemoveTagFromSystem(tag.Type, tag.Name))
                    {
                        Debug.Log($"Tag '{tag.Name}' removed from the system.");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to remove tag '{tag.Name}' from the system.");
                    }
                }
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            string[] tagTypes = GetUniqueTagTypes();
            GUILayout.Label("Type", GUILayout.Width(30));
            selectedTagTypeIndex = EditorGUILayout.Popup(selectedTagTypeIndex, tagTypes, GUILayout.Width(100));
            GUILayout.Label("Name", GUILayout.Width(40));
            newSystemTagName = EditorGUILayout.TextField(newSystemTagName, GUILayout.Width(100));
            if (GUILayout.Button("Add Tag to System", GUILayout.Width(150)))
            {
                if (fxSceneManager.AddTagToSystem(tagTypes[selectedTagTypeIndex], newSystemTagName))
                {
                    Debug.Log($"Tag '{newSystemTagName}' added to the system.");
                }
                else
                {
                    Debug.LogWarning($"Tag '{newSystemTagName}' already exists in the system.");
                }
                newSystemTagName = "";
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.Label("Manage Current Scene Tags", EditorStyles.boldLabel);

            string[] tagNames = fxSceneManager.tagList.Select(tag => $"{tag.Type}: {tag.Name}").ToArray();
            GUILayout.BeginHorizontal();
            selectedTagIndex = EditorGUILayout.Popup(selectedTagIndex, tagNames, GUILayout.Width(200));
            if (GUILayout.Button("Add Tag to Scene", GUILayout.Width(150)))
            {
                var selectedTag = fxSceneManager.tagList[selectedTagIndex];
                if (fxSceneManager.AddTagToScene(fxSceneManager.CurrentSceneName, selectedTag.Type, selectedTag.Name))
                {
                    Debug.Log($"Tag '{selectedTag.Name}' added to scene '{fxSceneManager.CurrentSceneName}'.");
                }
                else
                {
                    Debug.LogWarning($"Failed to add tag '{selectedTag.Name}' to scene '{fxSceneManager.CurrentSceneName}'.");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.Label("Current Scene Tags", EditorStyles.boldLabel);
            Scene currentScene = fxSceneManager.scenes.Find(scene => scene.Name == fxSceneManager.CurrentSceneName);
            if (currentScene != null && currentScene.Tags.Count > 0)
            {
                foreach (var tag in currentScene.Tags)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{tag.Type}: {tag.Name}", GUILayout.Width(200));
                    if (GUILayout.Button("Remove Tag", GUILayout.Width(100)))
                    {
                        if (fxSceneManager.RemoveTagFromScene(currentScene.Name, tag.Type, tag.Name))
                        {
                            Debug.Log($"Tag '{tag.Name}' removed from scene '{currentScene.Name}'.");
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to remove tag '{tag.Name}' from scene '{currentScene.Name}'.");
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
            if (string.IsNullOrEmpty(fxSceneManager.CurrentSceneName))
            {
                Debug.LogError("Scene name cannot be empty.");
                return;
            }

            FXManager.Instance.SavePreset(fxSceneManager.CurrentSceneName);
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
            return fxSceneManager.tagList.Select(tag => tag.Type).Distinct().ToArray();
        }
    }
}
