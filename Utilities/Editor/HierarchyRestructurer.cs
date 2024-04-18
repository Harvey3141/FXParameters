using UnityEditor;
using UnityEngine;

public class HierarchyRestructurer : EditorWindow
{
    [MenuItem("Tools/Hierarchy Restructurer")]
    public static void ShowWindow()
    {
        GetWindow<HierarchyRestructurer>("Hierarchy Restructurer");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Restructure Selected GameObject"))
        {
            RestructureHierarchy();
        }
    }

    private void RestructureHierarchy()
    {
        GameObject selectedGameObject = Selection.activeGameObject;

        // Check if a GameObject is selected
        if (selectedGameObject != null)
        {
            // Process all children
            ProcessChildObjects(selectedGameObject, selectedGameObject.transform);
        }
        else
        {
            Debug.LogError("No GameObject selected. Please select a GameObject to restructure its hierarchy.");
        }
    }

    private void ProcessChildObjects(GameObject mainParent, Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            // If the child has no children of its own, move it to the main parent
            if (child.childCount == 0)
            {
                child.SetParent(mainParent.transform);
            }
            else
            {
                // If the child has children, recursively process those
                ProcessChildObjects(mainParent, child);
            }
        }
    }
}
