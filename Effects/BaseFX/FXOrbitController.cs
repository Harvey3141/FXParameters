using UnityEngine;

public class FXOrbitController : MonoBehaviour
{
    public Transform targetPoint; // The point around which the objects will orbit
    public GameObject[] orbitingObjects; // Array of objects to orbit around the target point
    public float orbitRadius = 5.0f; // Radius of the orbit
    public float orbitSpeed = 30.0f; // Speed at which the objects will orbit (in degrees per second)
    public Vector3 orbitAxis = Vector3.up; // Axis around which the objects will orbit

    private Vector3[] startPositions;

    void Start()
    {
        if (!targetPoint)
        {
            Debug.LogError("Target Point not set for FXOrbitController!");
            enabled = false;
            return;
        }

        // Store initial positions of objects at a distance of orbitRadius from the target point
        startPositions = new Vector3[orbitingObjects.Length];
        for (int i = 0; i < orbitingObjects.Length; i++)
        {
            Vector3 offset = (orbitingObjects[i].transform.position - targetPoint.position).normalized * orbitRadius;
            startPositions[i] = targetPoint.position + offset;
            orbitingObjects[i].transform.position = startPositions[i];
            orbitingObjects[i].transform.LookAt(targetPoint);
        }
    }

    void Update()
    {
        for (int i = 0; i < orbitingObjects.Length; i++)
        {
            // Calculate the new position based on orbit for each object
            orbitingObjects[i].transform.RotateAround(targetPoint.position, orbitAxis, orbitSpeed * Time.deltaTime);
            orbitingObjects[i].transform.LookAt(targetPoint);
        }
    }
}
