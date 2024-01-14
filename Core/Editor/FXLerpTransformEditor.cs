using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FXLerpTransform))]
public class FXLerpTransformEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FXLerpTransform fxAreaLight = (FXLerpTransform)target;

        EditorGUILayout.LabelField("Start Transform", EditorStyles.boldLabel);
        fxAreaLight.positionA = EditorGUILayout.Vector3Field("Position", fxAreaLight.positionA);
        fxAreaLight.rotationA = EditorGUILayout.Vector3Field("Rotation", fxAreaLight.rotationA);
        fxAreaLight.scaleA = EditorGUILayout.Vector3Field("Scale", fxAreaLight.scaleA);

        if (GUILayout.Button("Set"))
        {
            fxAreaLight.positionA = fxAreaLight.transform.position;
            fxAreaLight.rotationA = fxAreaLight.transform.eulerAngles;
            fxAreaLight.scaleA = fxAreaLight.transform.localScale;
        }

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("End Transform", EditorStyles.boldLabel);
        fxAreaLight.positionB = EditorGUILayout.Vector3Field("Position", fxAreaLight.positionB);
        fxAreaLight.rotationB = EditorGUILayout.Vector3Field("Rotation", fxAreaLight.rotationB);
        fxAreaLight.scaleB = EditorGUILayout.Vector3Field("Scale", fxAreaLight.scaleB);

        if (GUILayout.Button("Set"))
        {
            fxAreaLight.positionB = fxAreaLight.transform.position;
            fxAreaLight.rotationB = fxAreaLight.transform.eulerAngles;
            fxAreaLight.scaleB = fxAreaLight.transform.localScale;
        }
        EditorGUILayout.EndVertical(); // End of bounding box

    }

    private void OnSceneGUI()
    {
        FXLerpTransform fxAreaLight = (FXLerpTransform)target;

        EditorGUI.BeginChangeCheck();
        Vector3 newPositionA = Handles.PositionHandle(fxAreaLight.positionA, Quaternion.Euler(fxAreaLight.rotationA));
        Vector3 newPositionB = Handles.PositionHandle(fxAreaLight.positionB, Quaternion.Euler(fxAreaLight.rotationB));

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(fxAreaLight, "Move Handle");
            fxAreaLight.positionA = newPositionA;
            fxAreaLight.positionB = newPositionB;
        }

        Handles.DrawLine(fxAreaLight.positionA, fxAreaLight.positionB);
    }
}
