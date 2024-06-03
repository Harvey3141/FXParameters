using UnityEditor;
using UnityEngine;

namespace FX
{
    [CustomEditor(typeof(FXColourPaletteManager))]
    public class FXColourPaletteManagerEditor : Editor
    {
        private FXColourPaletteManager manager;
        private string[] paletteNames;
        private int selectedPaletteIndex;

        private void OnEnable()
        {
            manager = (FXColourPaletteManager)target;
            UpdatePaletteNames();
            selectedPaletteIndex = manager.palettes.FindIndex(p => p.id == manager.activePalette?.id);
        }

        public override void OnInspectorGUI()
        {
            manager.usePaletteManager = EditorGUILayout.Toggle("Use Palette Manager", manager.usePaletteManager);
            manager.useForceUpdate = EditorGUILayout.Toggle("Use Force Update", manager.useForceUpdate);

            if (manager.palettes != null && manager.palettes.Count > 0)
            {
                UpdatePaletteNames(); 

                int newSelectedIndex = EditorGUILayout.Popup("Active Palette", selectedPaletteIndex, paletteNames);
                if (newSelectedIndex != selectedPaletteIndex)
                {
                    selectedPaletteIndex = newSelectedIndex;
                    SetActivePalette(selectedPaletteIndex);
                }               

                if (manager.activePalette != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Active Palette Colors", EditorStyles.boldLabel);

                    for (int i = 0; i < manager.activePalette.colours.Count; i++)
                    {
                        EditorGUI.BeginChangeCheck();
                        Color newColor = EditorGUILayout.ColorField($"Color {i}", manager.activePalette.colours[i]);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(manager, "Change Color");
                            manager.activePalette.SetColor(i, newColor);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("No palettes available.");
            }

            if (GUILayout.Button("Save"))
            {
                manager.SaveGlobalColourPalettes();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(manager);
            }
        }

        private void UpdatePaletteNames()
        {
            paletteNames = new string[manager.palettes.Count];
            for (int i = 0; i < manager.palettes.Count; i++)
            {
                paletteNames[i] = manager.palettes[i].name;
            }
        }

        private void SetActivePalette(int index)
        {
            if (index >= 0 && index < manager.palettes.Count)
            {
                manager.SetActivePalette(manager.palettes[index].id);
                EditorUtility.SetDirty(manager); 
            }
        }
    }
}
