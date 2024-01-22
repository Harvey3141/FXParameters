using UnityEngine;

public class ScaleCuboids : MonoBehaviour
{
    public AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
    public float duration = 5f; // Duration of the animation in seconds

    private float elapsedTime = 0f;

    void Update()
    {
        // Update the elapsed time
        elapsedTime += Time.deltaTime;
        if (elapsedTime > duration)
            elapsedTime = 0f; // Loop the animation

        // Evaluate the scale from the animation curve
        Vector3 currentScale = Vector3.one * scaleCurve.Evaluate(elapsedTime / duration);
        ScaleAllCuboids(currentScale);
    }

    private void ScaleAllCuboids(Vector3 scale)
    {
        foreach (Transform child in transform)
        {
            //if (child.gameObject.CompareTag("Cuboid")) // Ensure the object is tagged as "Cuboid"
            //{
                child.localScale = scale;
            //}
        }
    }
}
