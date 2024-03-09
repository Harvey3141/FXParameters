using UnityEngine;
using UnityEditor;
using MathGeoLib;

public class BoundingBoxCubeGenerator : EditorWindow
{
    private GameObject referenceObject;
    private GameObject generatedCube;

    [MenuItem("Tools/Bounding Box Cube Generator")]
    public static void ShowWindow()
    {
        GetWindow<BoundingBoxCubeGenerator>("Bounding Box Cube Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Generate Cube from Oriented Bounding Box", EditorStyles.boldLabel);
        referenceObject = (GameObject)EditorGUILayout.ObjectField("Reference Object", referenceObject, typeof(GameObject), true);

        if (GUILayout.Button("Generate Cube"))
        {
            GenerateOrientedBoundingBoxCube();
        }

        if (generatedCube != null)
        {
            EditorGUILayout.LabelField("Generated Cube:", generatedCube.name);
        }
    }

    private void GenerateOrientedBoundingBoxCube()
    {
        if (referenceObject != null)
        {
            MeshFilter meshFilter = referenceObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                Vector3[] vertices = meshFilter.mesh.vertices;
                OrientedBoundingBox obb = OrientedBoundingBox.OptimalEnclosing(vertices);

                // Transform vertices to world space
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = referenceObject.transform.TransformPoint(vertices[i]);
                }

                // Recalculate OBB for transformed vertices
                obb = OrientedBoundingBox.OptimalEnclosing(vertices);

                if (generatedCube != null)
                {
                    DestroyImmediate(generatedCube);
                }

                generatedCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                generatedCube.transform.position = obb.Center;
                generatedCube.transform.localScale = obb.Extent * 2; // Scale
                generatedCube.transform.rotation = Quaternion.LookRotation(obb.Axis1, obb.Axis2);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "The reference object does not have a MeshFilter component.", "OK");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Reference object is not set.", "OK");
        }
    }
}
