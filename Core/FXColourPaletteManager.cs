using FX;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColorPalette
{
    public string paletteName;
    public Color[] colors;

    public ColorPalette(string name, params Color[] paletteColors)
    {
        paletteName = name;
        colors = paletteColors;
    }
}

public class FXColourPaletteManager : FXBase
{
    FXParameter<int> activePaletteIndex = new FXParameter<int>(0);


    public List<ColorPalette> palettes;



    public static FXColourPaletteManager Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    protected override void Start()
    {
        base.Start();
        activePaletteIndex.OnValueChanged += SetActivePaletteIndex;
    }

    private void SetActivePaletteIndex(int i)
    {
        if (i >= 0 && i < palettes.Count)
        {
            activePaletteIndex.Value = i;
        }
        else
        {
            Debug.LogWarning("Invalid palette index: " + i);
        }
    }


    //public Color GetColor(int colorIndex)
    //{
    //
    //    return palette.colors[colorIndex];  
    //    
    //    return Color.white;
    //}
    //
    //public ColourPalette GetActiveColourPalette() { return palettes[activePaletteIndex.Value]; } 

}
