using UnityEngine;
using UnityEditor;

public class GridGeneratorWindow : EditorWindow
{
    private GameObject referenceObject;
    private Vector3Int subdivisions = new Vector3Int(5, 5, 5);
    private Vector3 boundingBoxSize;
    private Vector3 boundingBoxCenter;

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
            Renderer rend = referenceObject.GetComponent<Renderer>();
            if (rend != null)
            {
                boundingBoxSize = rend.bounds.size;
                boundingBoxCenter = rend.bounds.center;
                EditorGUILayout.LabelField("Bounding Box Size", boundingBoxSize.ToString());
            }
            else
            {
                EditorGUILayout.HelpBox("The selected GameObject does not have a Renderer component.", MessageType.Warning);
            }
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

    private void GenerateGrid()
    {
        GameObject gridParent = new GameObject("GridParent");
        gridParent.transform.position = boundingBoxCenter; // Set the position to the center of the bounding box

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

            // Left
            new Vector3(-halfSize.x, -halfSize.y, halfSize.z),
            new Vector3(-halfSize.x, halfSize.y, halfSize.z),
            new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
            new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),

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

            // Top
            new Vector3(-halfSize.x, halfSize.y, halfSize.z),
            new Vector3(halfSize.x, halfSize.y, halfSize.z),
            new Vector3(halfSize.x, halfSize.y, -halfSize.z),
            new Vector3(-halfSize.x, halfSize.y, -halfSize.z)
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
            // Bottom
            3, 1, 0, 3, 2, 1, 
            // Left
            4, 5, 6, 4, 6, 7, 
            // Front
            8, 9, 10, 8, 10, 11, 
            // Back
            12, 15, 14, 12, 14, 13, 
            // Right
            16, 17, 18, 16, 18, 19, 
            // Top
            20, 21, 22, 20, 22, 23
        };

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}
