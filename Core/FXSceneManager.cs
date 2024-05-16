using FX.Patterns;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FX
{
    public class Tag
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public Tag(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}



namespace FX
{
    public class Scene
    {
        public string Name { get; set; }
        public List<Tag> Tags { get; private set; }

        public Scene(string name)
        {
            Name = name;
            Tags = new List<Tag>();
        }

        public bool AddTag(Tag tag)
        {
            if (!Tags.Contains(tag))
            {
                Tags.Add(tag);
                return true;
            }
            return false;
        }

        public bool RemoveTag(Tag tag)
        {
            return Tags.Remove(tag);
        }
    }
}


namespace FX
{
    public class FXSceneManager : MonoBehaviour
    {
        FXManager fXManager;
        [HideInInspector]
        public List<Scene> scenes;
        [HideInInspector]
        public List<Tag> tagList;

        public bool exportParameterListOnStart = false;

        [HideInInspector]
        private string currentSceneName;
        public string CurrentSceneName
        {
            get => currentSceneName;
            set
            {
                if (currentSceneName != value)
                {
                    currentSceneName = value;
                    onCurrentSceneNameChanged?.Invoke(currentSceneName);
                }
            }
        }

        public delegate void OnSceneListUpdated(List<Scene> scenes);
        public event OnSceneListUpdated onSceneListUpdated;

        public delegate void OnCurrentSceneNameChanged(string newName);
        public event OnCurrentSceneNameChanged onCurrentSceneNameChanged;

        public delegate void OnSceneRemoved(string newName);
        public event OnSceneRemoved onSceneRemoved;

        private void Awake()
        {
            fXManager = FXManager.Instance;
            scenes = new List<Scene>();
            tagList = new List<Tag> { new Tag("scene-bucket", "test"), new Tag("scene-label", "test") };
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
            if (fXManager.LoadPreset(name))
            {
                CurrentSceneName = name;
                return true;
            }
            return false;
        }

        public void SaveScene()
        {
            if (!string.IsNullOrEmpty(CurrentSceneName)) SaveScene(CurrentSceneName);
        }

        public void SaveScene(string name)
        {
            fXManager.SavePreset(name);
            PopulateScenesList();
        }

        public void ExportParameterList()
        {
            fXManager.SavePreset("ParameterList", true);
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
            if (!string.IsNullOrEmpty(currentSceneName)) LoadScene(currentSceneName);
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

        public bool AddTagToSystem(string type, string name)
        {
            if (!tagList.Exists(t => t.Name == name && t.Type == type))
            {
                tagList.Add(new Tag(type, name));
                return true;
            }
            return false;
        }

        public bool RemoveTagFromSystem(string type, string name)
        {
            Tag tag = tagList.Find(t => t.Name == name && t.Type == type);
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


        public bool AddTagToScene(string sceneName, string tagType, string tagName)
        {
            Scene scene = scenes.Find(s => s.Name == sceneName);
            Tag tag = tagList.Find(t => t.Type == tagType && t.Name == tagName);
            if (scene != null && tag != null)
            {
                return scene.AddTag(tag);
            }
            return false;
        }

        public bool RemoveTagFromScene(string sceneName, string tagType, string tagName)
        {
            Scene scene = scenes.Find(s => s.Name == sceneName);
            Tag tag = tagList.Find(t => t.Type == tagType && t.Name == tagName);
            if (scene != null && tag != null)
            {
                return scene.RemoveTag(tag);
            }
            return false;
        }

    }
}
