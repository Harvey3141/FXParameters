using UnityEngine;
using System.Collections.Generic;
using FX;
using UnityEngine.Rendering.HighDefinition;

public class FXScaler : FXBaseWithEnabled
{
    public enum ScaleType { X, Y, Z, XYZ }

    [SerializeField]
    private ScaleType _scaleType = ScaleType.XYZ;
    private ScaleType _previousScaleType;

    public ScaleType scaleType
    {
        get => _scaleType;
        set
        {
            _scaleType = value;
        }
    }

    public FXScaledParameter<float> scale   = new FXScaledParameter<float>(1.0f, 0.0f, 1.0f);
    public List<Transform> targetTransforms = new List<Transform>();

    protected override void Start()
    {
        base.Start();
        _previousScaleType = scaleType;
    }

    private void Update()
    {
        ApplyScaling();
    }

    private void ApplyScaling()
    {
        if (!fxEnabled.Value) return;

        foreach (var transform in targetTransforms)
        {
            if (transform != null)
            {
                Vector3 currentScale = transform.localScale;

                switch (scaleType)
                {
                    case ScaleType.X:
                        transform.localScale = new Vector3(scale.ScaledValue, currentScale.y, currentScale.z);
                        break;
                    case ScaleType.Y:
                        transform.localScale = new Vector3(currentScale.x, scale.ScaledValue, currentScale.z);
                        break;
                    case ScaleType.Z:
                        transform.localScale = new Vector3(currentScale.x, currentScale.y, scale.ScaledValue);
                        break;
                    case ScaleType.XYZ:
                        transform.localScale = new Vector3(scale.ScaledValue, scale.ScaledValue, scale.ScaledValue);
                        break;
                }
            }
        }
    }

    private void OnValidate()
    {
        if (_scaleType != _previousScaleType)
        {
            ResetScale();
            _previousScaleType = _scaleType;
        }
    }

    private void ResetScale()
    {
        foreach (var transform in targetTransforms)
        {
            if (transform != null)
            {
                transform.localScale = Vector3.one;
            }
        }
    }

    protected override void OnFXEnabled(bool state)
    {
        ResetScale();
    }
}
