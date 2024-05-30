using UnityEngine;
using System.Collections.Generic;
using FX;

public class FXScaler : FXBaseWithEnabled
{
    public enum ScaleType { X, Y, Z, XYZ }

    public FXParameter<ScaleType> scaleType = new FXParameter<ScaleType>(ScaleType.XYZ);  

    private ScaleType previousScaleType;

    public FXScaledParameter<float> scale   = new FXScaledParameter<float>(1.0f, 0.0f, 1.0f);
    public List<Transform> targetTransforms = new List<Transform>();

    public Vector3[] initialScales;

    public bool resetsToSceneDefault = false;

    protected override void Start()
    {
        initialScales = new Vector3[targetTransforms.Count];
        int i = 0;
        foreach (var t in targetTransforms) {
            initialScales[i] = t.localScale;
            i++;
        }
        base.Start();
        previousScaleType = scaleType.Value;
        scaleType.OnValueChanged += OnScaleTypeChanged;
    }

    private void OnScaleTypeChanged(ScaleType obj)
    {
        ResetScale();
        previousScaleType = scaleType.Value;
    }

    private void Update()
    {
        ApplyScaling();
    }

    private void ApplyScaling()
    {
        if (!fxEnabled.Value) return;

        int i = 0;
        foreach (var transform in targetTransforms)
        {
            if (transform != null)
            {
                Vector3 currentScale = transform.localScale;

                switch (scaleType.Value)
                {
                    case ScaleType.X:
                        transform.localScale = new Vector3(scale.ScaledValue * initialScales[i].x, currentScale.y, currentScale.z);
                        break;
                    case ScaleType.Y:
                        transform.localScale = new Vector3(currentScale.x, scale.ScaledValue * initialScales[i].y, currentScale.z);
                        break;
                    case ScaleType.Z:
                        transform.localScale = new Vector3(currentScale.x, currentScale.y, scale.ScaledValue * initialScales[i].z);
                        break;
                    case ScaleType.XYZ:
                        transform.localScale = new Vector3(scale.ScaledValue * initialScales[i].x, scale.ScaledValue * initialScales[i].y, scale.ScaledValue * initialScales[i].z);
                        break;
                }
            }
            i++;
        }
    }

    private void ResetScale()
    {
        int i = 0;
        foreach (var transform in targetTransforms)
        {
            if (transform != null)
            {
                if (initialScales != null) {
                    if (initialScales[i] != null) transform.localScale = initialScales[i];
                }
                //scale.ScaledValue* initialScales[i].y
            }
            i++;
        }
    }

    protected override void OnFXEnabled(bool state)
    {
        ResetScale();
    }
}
