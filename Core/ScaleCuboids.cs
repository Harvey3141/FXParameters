using System.Collections.Generic;
using UnityEngine;

public class ScaleCuboids : MonoBehaviour
{
    public AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
    public float duration = 2f; 

    private float elapsedTime = 0f;

    public List<GameObject> cuboidParents = new List<GameObject>();


    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > duration)
            elapsedTime = 0f; // Loop the animation

        Vector3 currentScale = Vector3.one * scaleCurve.Evaluate(elapsedTime / duration);
        ScaleAllCuboids(currentScale);
    }

    private void ScaleAllCuboids(Vector3 scale)
    {
        foreach (GameObject go in cuboidParents) {

            foreach (Transform child in go.transform)
            {
                child.localScale = scale;
            }
        }
    }
}
