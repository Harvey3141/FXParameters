using FX.Patterns;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FX
{
    public interface ITag
    {
        string Name { get; set; }
        object Value { get; }
    }

    public class Tag<T> : ITag
    {
        public string Name { get; set; }
        public T Value { get; set; }

        object ITag.Value => Value;

        public Tag(string name, T value)
        {
            Name = name;
            Value = value;
        }
    }

    public class Scene
    {
        public string Name { get; set; }
        public List<ITag> Tags { get; set; }

        public Scene(string name)
        {
            Name = name;
            Tags = new List<ITag>();
        }

        public bool AddTag(ITag tag)
        {
            if (!Tags.Exists(t => t.Name == tag.Name && t.Value.Equals(tag.Value)))
            {
                Tags.Add(tag);
                return true;
            }
            return false;
        }

        public bool RemoveTag(ITag tag)
        {
            return Tags.Remove(tag);
        }
    }

    public class FXSceneManager : MonoBehaviour
    {
        FXManager fXManager;
        [HideInInspector]
        public List<Scene> scenes;
        [HideInInspector]
        public List<ITag> tagList;

        public bool exportParameterListOnStart = false;

        private Scene currentScene;
        public Scene CurrentScene
        {
            get => currentScene;
            set
            {
                if (currentScene != value)
                {
                    currentScene = value;
                    onCurrentSceneChanged?.Invoke(currentScene);
                }
            }
        }

        public delegate void OnSceneListUpdated(List<Scene> scenes);
        public event OnSceneListUpdated onSceneListUpdated;

        public delegate void OnCurrentSceneChanged(Scene newScene);
        public event OnCurrentSceneChanged onCurrentSceneChanged;

        public delegate void OnSceneRemoved(string sceneName);
        public event OnSceneRemoved onSceneRemoved;

        private void Awake()
        {
            fXManager = FXManager.Instance;
            scenes = new List<Scene>();
            tagList = new List<ITag>
            {
                new Tag<string>("scene-bucket", "test bucket"),
                new Tag<string>("scene-label", "value2"),
                new Tag<bool>("scene-float", true)
            };
            PopulateScenesList();
        }

        public void PopulateScenesList()
        {
            scenes.Clear();
            string scenesFolderPath = Path.Combine(Application.streamingAssetsPath, "FX Scenes");

            if (Directory.Exists(scenesFolderPath))
            {
                DirectoryInfo scenesDirectory = new DirectoryInfo(scenesFolderPath);
                FileInfo[] sceneFiles = scenesDirectory.GetFiles("*.json");

                foreach (FileInfo file in sceneFiles)
                {
                    if (file.Name != "ParameterList")
                    {
                        string sceneName = Path.GetFileNameWithoutExtension(file.Name);
                        // Placeholder: Tags should be loaded from the file
                        Scene scene = new Scene(sceneName);
                        scenes.Add(scene);
                    }
                }
                onSceneListUpdated?.Invoke(scenes);
            }
            else
            {
                Debug.LogError("Scenes folder not found: " + scenesFolderPath);
            }
        }

        public bool LoadScene(string name)
        {
            if (fXManager.LoadPreset(name, out List<ITag> loadedTags))
            {
                CurrentScene = scenes.Find(s => s.Name == name);

                foreach (var tag in loadedTags)
                {
                    AddTagToSystem(tag.Name, tag.Value);
                }

                if (CurrentScene != null)
                {
                    CurrentScene.Tags = loadedTags;
                    return true;
                }
            }
            return false;
        }


        public void SaveScene()
        {
            if (CurrentScene != null)
            {
                SaveScene(CurrentScene);
            }
        }

        public void SaveScene(FX.Scene scene)
        {
            fXManager.SavePreset(scene);
            PopulateScenesList();
        }

        public void ExportParameterList()
        {
            Scene parameterListScene = new Scene("ParameterList");
            fXManager.SavePreset(parameterListScene,true);
        }

        public void RemoveScene(string name)
        {
            string scenesFolderPath = Path.Combine(Application.streamingAssetsPath, "FX Scenes");
            string scenePath = Path.Combine(scenesFolderPath, name + ".json");
            string metaPath = Path.Combine(scenePath + ".meta");

            if (File.Exists(scenePath))
            {
                File.Delete(scenePath);
                PopulateScenesList();

                if (File.Exists(metaPath))
                {
                    File.Delete(metaPath);
                }
                onSceneRemoved?.Invoke(name);
            }
            else
            {
                Debug.LogError("Scene not found: " + name);
            }
        }

        public void ResetCurrentScene()
        {
            if (CurrentScene != null)
            {
                LoadScene(CurrentScene.Name);
            }
        }

        public void CreateNewScene()
        {
            GroupFXController[] allGroups = GameObject.FindObjectsOfType<GroupFXController>();

            foreach (var group in allGroups)
            {
                if (!group.isPinned)
                {
                    Destroy(group.gameObject);
                }
                else
                {
                    group.ClearFXAdresses();
                }
            }

            fXManager.ResetAllParamsToDefault();
        }

        public bool AddTagToSystem<T>(string name, T value)
        {
            if (!tagList.Exists(t => t.Name == name && t.Value.Equals(value)))
            {
                tagList.Add(new Tag<T>(name, value));
                return true;
            }
            return false;
        }

        public bool RemoveTagFromSystem(string name, object value)
        {
            ITag tag = tagList.Find(t => t.Name == name && t.Value.Equals(value));
            if (tag != null)
            {
                foreach (var scene in scenes)
                {
                    scene.RemoveTag(tag);
                }
                return tagList.Remove(tag);
            }
            return false;
        }

        public bool AddTagToScene(string sceneName, string name, object value)
        {
            Debug.Log($"Attempting to add tag to scene: {sceneName}, tag name: {name}, tag value: {value}");

            Scene scene = scenes.Find(s => s.Name == sceneName);
            if (scene == null)
            {
                Debug.LogError($"Scene not found: {sceneName}");
                return false;
            }

            ITag tag = tagList.Find(t => t.Name == name && CompareTagValue(t.Value, value));
            if (tag == null)
            {
                Debug.LogError($"Tag not found: {name} with value {value}");
                return false;
            }

            Debug.Log($"Adding tag {name} with value {value} to scene {sceneName}");
            return scene.AddTag(tag);
        }

        public bool RemoveTagFromScene(string sceneName, string tagName, object value)
        {
            Debug.Log($"Attempting to remove tag from scene: {sceneName}, tag name: {tagName}, tag value: {value}");

            Scene scene = scenes.Find(s => s.Name == sceneName);
            if (scene == null)
            {
                Debug.LogError($"Scene not found: {sceneName}");
                return false;
            }

            ITag tag = tagList.Find(t => t.Name == tagName && CompareTagValue(t.Value, value));
            if (tag == null)
            {
                Debug.LogError($"Tag not found: {tagName} with value {value}");
                return false;
            }

            Debug.Log($"Removing tag {tagName} with value {value} from scene {sceneName}");
            return scene.RemoveTag(tag);
        }

        private bool CompareTagValue(object tagValue, object value)
        {
            Debug.Log($"Comparing values. tagValue type: {tagValue.GetType()}, value type: {value.GetType()}");

            if (tagValue is float tagFloat && value is float inputFloat)
            {
                bool isEqual = Mathf.Abs(tagFloat - inputFloat) < Mathf.Epsilon;
                Debug.Log($"Comparing float values: {tagFloat} and {inputFloat}, equal: {isEqual}");
                return isEqual;
            }
            if (tagValue is string tagString && value is string inputString)
            {
                bool isEqual = tagString.Equals(inputString);
                Debug.Log($"Comparing string values: {tagString} and {inputString}, equal: {isEqual}");
                return isEqual;
            }
            // Add more detailed logging for other types if necessary
            Debug.Log($"tagValue: {tagValue}, value: {value}");
            bool equals = tagValue.Equals(value);
            Debug.Log($"Comparing values: {tagValue} and {value}, equal: {equals}");
            return equals;
        }

    }
}
