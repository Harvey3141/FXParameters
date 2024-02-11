using FX;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class ScaleCuboids : FXBaseWithEnabled
{
    public AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
    public int BPM = 120;

    public FXParameter<int> beatsPerCycle = new FXParameter<int>(4,1,16);
    private float duration; 

    private float elapsedTime = 0f;

    public List<GameObject> cuboidParents = new List<GameObject>();


    protected override void Start()
    {
        base.Start();
        UpdateDurationBasedOnBPM();

    }

    void Update()
    {
        if (!fxEnabled.Value) return;
        UpdateDurationBasedOnBPM(); 

        elapsedTime += Time.deltaTime;
        if (elapsedTime > duration)
            elapsedTime = 0f; 

        Vector3 currentScale = Vector3.one * scaleCurve.Evaluate(elapsedTime / duration);
        ScaleAllCuboids(currentScale);
    }

    protected override void OnFXEnabled(bool state)
    {
        if (!state) ScaleAllCuboids(Vector3.one);
    }

    private void ScaleAllCuboids(Vector3 scale)
    {
        foreach (GameObject go in cuboidParents)
        {
            foreach (Transform child in go.transform)
            {
                child.localScale = scale;
            }
        }
    }

    private void UpdateDurationBasedOnBPM()
    {
        duration = 60f / BPM*beatsPerCycle.Value; 
    }
}
