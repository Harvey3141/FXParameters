using UnityEngine;
using UnityEditor;
using MathGeoLib;


public class GridGeneratorWindow : EditorWindow
{
    private GameObject referenceObject;
    private Vector3Int subdivisions = new Vector3Int(5, 5, 5);
    private Vector3 boundingBoxSize;
    private Vector3 boundingBoxCenter;
    Vector3 axis1;
    Vector3 axis2;

    [MenuItem("Tools/Grid Generator")]
    public static void ShowWindow()
    {
        GetWindow<GridGeneratorWindow>("Grid Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Generator Settings", EditorStyles.boldLabel);

        referenceObject = (GameObject)EditorGUILayout.ObjectField("Reference Object", referenceObject, typeof(GameObject), true);

        if (referenceObject != null)
        {
            CalculateOBB();
            EditorGUILayout.LabelField("Bounding Box Size", boundingBoxSize.ToString());
        }
        else
        {
            boundingBoxSize = EditorGUILayout.Vector3Field("Manual Bounding Box Size", boundingBoxSize);
        }

        subdivisions = EditorGUILayout.Vector3IntField("Subdivisions", subdivisions);

        if (GUILayout.Button("Generate Grid"))
        {
            GenerateGrid();
        }
    }

    private void CalculateOBB()
    {
        MeshFilter meshFilter = referenceObject.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Vector3[] vertices = meshFilter.mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = referenceObject.transform.TransformPoint(vertices[i]);
            }

            OrientedBoundingBox obb = OrientedBoundingBox.OptimalEnclosing(vertices);
            boundingBoxSize = obb.Extent * 2;
            boundingBoxCenter = obb.Center;
            axis1 = obb.Axis1;
            axis2 = obb.Axis2;

        }
        else
        {
            Debug.LogError("Reference object does not have a MeshFilter component.");
        }
    }

    private void GenerateGrid()
    {
        GameObject gridParent = new GameObject("GridParent");
        gridParent.transform.position = boundingBoxCenter;
        gridParent.transform.rotation = Quaternion.LookRotation(axis1, axis2); 

        Vector3 cuboidSize = new Vector3(boundingBoxSize.x / subdivisions.x, boundingBoxSize.y / subdivisions.y, boundingBoxSize.z / subdivisions.z);

        for (int x = 0; x < subdivisions.x; x++)
        {
            for (int y = 0; y < subdivisions.y; y++)
            {
                for (int z = 0; z < subdivisions.z; z++)
                {
                    GameObject cuboid = new GameObject($"Cuboid_{x}_{y}_{z}");
                    cuboid.transform.parent = gridParent.transform;
                    cuboid.transform.localPosition = new Vector3(x * cuboidSize.x, y * cuboidSize.y, z * cuboidSize.z) - boundingBoxSize / 2 + cuboidSize / 2;
                    cuboid.transform.localRotation = Quaternion.identity;

                    MeshRenderer meshRenderer = cuboid.AddComponent<MeshRenderer>();
                    meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

                    MeshFilter meshFilter = cuboid.AddComponent<MeshFilter>();
                    meshFilter.mesh = CreateCubeMesh(cuboidSize);
                }
            }
        }
    }



    private Mesh CreateCubeMesh(Vector3 size)
    {
        Mesh mesh = new Mesh();

        Vector3 halfSize = size * 0.5f;
        Vector3[] vertices = new Vector3[]
        {
        // Bottom
        new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
        new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
        new Vector3(halfSize.x, -halfSize.y, halfSize.z),
        new Vector3(-halfSize.x, -halfSize.y, halfSize.z),

 new Vector3(-halfSize.x, -halfSize.y, -halfSize.z), // Bottom Back
        new Vector3(-halfSize.x, halfSize.y, -halfSize.z),  // Top Back
        new Vector3(-halfSize.x, halfSize.y, halfSize.z),   // Top Front
        new Vector3(-halfSize.x, -halfSize.y, halfSize.z),  // Bottom Front

        // Front
        new Vector3(-halfSize.x, -halfSize.y, halfSize.z),
        new Vector3(halfSize.x, -halfSize.y, halfSize.z),
        new Vector3(halfSize.x, halfSize.y, halfSize.z),
        new Vector3(-halfSize.x, halfSize.y, halfSize.z),

        // Back
        new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
        new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
        new Vector3(halfSize.x, halfSize.y, -halfSize.z),
        new Vector3(-halfSize.x, halfSize.y, -halfSize.z),

        // Right
        new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
        new Vector3(halfSize.x, halfSize.y, -halfSize.z),
        new Vector3(halfSize.x, halfSize.y, halfSize.z),
        new Vector3(halfSize.x, -halfSize.y, halfSize.z),

        new Vector3(-halfSize.x, halfSize.y, halfSize.z), // Top Left - Front
        new Vector3(halfSize.x, halfSize.y, halfSize.z),  // Top Right - Front
        new Vector3(halfSize.x, halfSize.y, -halfSize.z), // Top Right - Back
        new Vector3(-halfSize.x, halfSize.y, -halfSize.z) // Top Left - Back

    };

        Vector3[] normals = new Vector3[]
        {
            Vector3.down, Vector3.down, Vector3.down, Vector3.down, // Bottom
            Vector3.left, Vector3.left, Vector3.left, Vector3.left, // Left
            Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward, // Front
            Vector3.back, Vector3.back, Vector3.back, Vector3.back, // Back
            Vector3.right, Vector3.right, Vector3.right, Vector3.right, // Right
            Vector3.up, Vector3.up, Vector3.up, Vector3.up // Top
        };

        Vector2[] uvs = new Vector2[24];
        for (int i = 0; i < 6; i++)
        {
            uvs[i * 4] = new Vector2(0, 0);
            uvs[i * 4 + 1] = new Vector2(1, 0);
            uvs[i * 4 + 2] = new Vector2(1, 1);
            uvs[i * 4 + 3] = new Vector2(0, 1);
        }

        int[] triangles = new int[]
        {
        // Correcting the winding order
        0, 1, 2, 0, 2, 3, // Bottom
        4, 6, 5, 4, 7, 6, // Left
        8, 9, 10, 8, 10, 11, // Front
        15, 14, 13, 15, 13, 12, // Back
        16, 17, 18, 16, 18, 19, // Right
        20, 21, 22, 20, 22, 23  // Top
        };

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

}
