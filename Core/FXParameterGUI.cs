using UnityEngine;
using FX;
using System.Linq;
using System.Collections.Generic;

public class FXParameterGUI : MonoBehaviour
{
    private Vector2 scrollPosition;
    public Vector2 panelSize;

    private Dictionary<string, Dictionary<string, List<(string address, object item)>>> groupedParams;


    GUIStyle groupLabelStyle;
    GUIStyle paramLabelStyle;
    GUIStyle scrollViewStyle;

    private void Start()
    {
        groupedParams = GroupParametersByBaseAddress();
        scrollViewStyle = new GUIStyle();
        scrollViewStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 0.8f));

    }

    private Dictionary<string, Dictionary<string, List<(string address, object item)>>> GroupParametersByBaseAddress()
    {
        return FXManager.fxItemsByAddress_
            .Where(x => x.Value.type == FXManager.FXItemInfoType.Parameter)
            .GroupBy(x => GetBaseAddress(x.Key))
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(x => GetSubgroupAddress(x.Key))
                    .ToDictionary(
                        sg => sg.Key,
                        sg => sg.Select(x => (x.Key, x.Value.item)).ToList()
                    )
            );
    }

    private string GetBaseAddress(string fullAddress)
    {
        var parts = fullAddress.Split('/');
        return parts.Length > 2 ? "/" + parts[1] : fullAddress;
    }

    private string GetSubgroupAddress(string fullAddress)
    {
        var parts = fullAddress.Split('/');
        return parts.Length > 3 ? "/" + parts[2] : "";
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }


    private void OnGUI()
    {
        if (groupLabelStyle == null)
        {
            groupLabelStyle = new GUIStyle(GUI.skin.label);
            groupLabelStyle.fontSize = 25;
            groupLabelStyle.fontStyle = FontStyle.Bold;
        }

        if (paramLabelStyle == null)
        {
            paramLabelStyle = new GUIStyle(GUI.skin.label);
            paramLabelStyle.fontSize = 20;
        }


        GUI.Box(new Rect(0, 0, panelSize.x, panelSize.y), GUIContent.none, scrollViewStyle);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(panelSize.x), GUILayout.Height(panelSize.y));

        foreach (var group in groupedParams)
        {
            GUILayout.Label(group.Key.Substring(1), groupLabelStyle);

            foreach (var subgroup in group.Value)
            {
                GUILayout.Label(subgroup.Key, paramLabelStyle);

                foreach (var item in subgroup.Value)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("  " + item.address, paramLabelStyle);
                    if (item.item is FXScaledParameter<float> floatParam)
                    {
                        floatParam.Value = GUILayout.HorizontalSlider(floatParam.Value, 0.0f, 1.0f);
                    }
                    else if (item.item is FXParameter<bool> boolParam)
                    {
                        boolParam.Value = GUILayout.Toggle(boolParam.Value, "");
                    }
                    GUILayout.EndHorizontal();
                }
            }
                    
        }

        GUILayout.EndScrollView();
    }
}
