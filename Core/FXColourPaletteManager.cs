using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FX
{
    [System.Serializable]
    public class ColourPalette
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<Color> colours;

        public event Action<int, Color> OnColourChanged;

        public ColourPalette(string name, List<Color> paletteColors)
        {
            this.id = Guid.NewGuid().ToString();
            this.name = name;
            colours = paletteColors;
        }

        public void SetColor(int index, Color color)
        {
            if (index >= 0 && index < colours.Count)
            {
                colours[index] = color;
                OnColourChanged?.Invoke(index, color);
            }
        }
    }

    public class FXColourPaletteManager : MonoBehaviour
    {
        public List<ColourPalette> palettes;
        public ColourPalette activePalette;

        public event Action<bool> onUsePaletteManagerChanged;
        public event Action<bool> onUseForceUpdateChanged;
        public event Action<ColourPalette> onPaletteChanged;
        public event Action<string> onActivePaletteChanged;

        private bool usePaletteManager_ = true;
        public bool usePaletteManager
        {
            get => usePaletteManager_;
            set
            {
                if (usePaletteManager_ != value)
                {
                    usePaletteManager_ = value;
                    onUsePaletteManagerChanged?.Invoke(value);
                }
            }
        }

        private bool useForceUpdate_;
        public bool useForceUpdate
        {
            get => useForceUpdate_;
            set
            {
                if (useForceUpdate_ != value)
                {
                    useForceUpdate_ = value;
                    onUseForceUpdateChanged?.Invoke(value);
                    fXManager.UpdateAllColorParametersToPalette(activePalette, useForceUpdate_);
                }
            }
        }

        private FXManager fXManager;


        public static FXColourPaletteManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }

            palettes = LoadGlobalColourPalettes();

            fXManager = FXManager.Instance;
            fXManager.onPresetLoaded += HandleSceneChanged;

            if (palettes != null && palettes.Count > 0)
            {
                SetActivePalette(palettes[0].id);
            }
        }

        public void SetActivePalette(string id)
        {
            var palette = palettes.Find(p => p.id == id);
            if (palette != null)
            {
                if (activePalette != null)
                {
                    activePalette.OnColourChanged -= HandleColorChanged;
                }

                activePalette = palette;
                activePalette.OnColourChanged += HandleColorChanged;
            }

            fXManager.UpdateAllColorParametersToPalette(activePalette, useForceUpdate);
            onActivePaletteChanged?.Invoke(activePalette.id);
        }

        private void HandleColorChanged(int index, Color color)
        {
            fXManager.UpdateColorParametersWithPaletteIndex(index, color, useForceUpdate);
            onPaletteChanged?.Invoke(activePalette);
        }

        private void HandleSceneChanged(string name)
        {
            if (usePaletteManager)
            {
                fXManager.UpdateAllColorParametersToPalette(activePalette, useForceUpdate);
            }
        }


        public int GetActiveColourPaletteSize()
        {
            return activePalette?.colours.Count ?? 0;
        }

        public Color GetColor(int index)
        {
            if (activePalette == null || index < 0 || index >= activePalette.colours.Count)
                return Color.white;

            return activePalette.colours[index];
        }

        public void NewPalette(ColourPalette data)
        {
            NewPalette(data.name, data.colours);
        }

        public void NewPalette(string name, List<Color> colors)
        {
            ColourPalette newPalette = new ColourPalette(name, colors);
            palettes.Add(newPalette);
            SaveGlobalColourPalettes();
        }

        public bool RemovePalette(string id)
        {
            ColourPalette paletteToRemove = palettes.Find(p => p.id == id);
            if (paletteToRemove != null)
            {
                palettes.Remove(paletteToRemove);
                SaveGlobalColourPalettes();
                if (activePalette == paletteToRemove && palettes.Count > 0)
                {
                    SetActivePalette(palettes[0].id);
                }
                return true;
            }
            return false;
        }

        public bool SetPalette(ColourPalette paletteData)
        {
            ColourPalette palette = palettes.Find(p => p.id == paletteData.id);
            if (palette != null)
            {
                palette.name = paletteData.name;
                palette.colours = paletteData.colours;

                if (activePalette.id == palette.id)
                {
                    SetActivePalette(palette.id);
                }

                SaveGlobalColourPalettes();
                onPaletteChanged?.Invoke(palette);

                return true;
            }
            else
            {
                return false;
            }
        }


        private List<ColourPalette> LoadGlobalColourPalettes()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "FX/ColourPalettes.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var settings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> {
                new ColourHandler()
                },
                };
                return JsonConvert.DeserializeObject<List<ColourPalette>>(json,settings);
            }
            return new List<ColourPalette>
            {
                new ColourPalette("White", new List<Color> { Color.white, Color.white}),
                new ColourPalette("Blue Red",new List<Color> {Color.blue, Color.red}),
                new ColourPalette("Green White",new List<Color> {Color.green, Color.white})
            };
        }

        public void SaveGlobalColourPalettes()
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {
                new ColourHandler()
                },
            };

            string path = Path.Combine(Application.streamingAssetsPath, "FX/ColourPalettes.json");
            string json = JsonConvert.SerializeObject(palettes, settings);
            File.WriteAllText(path, json);
        }

    }
}
