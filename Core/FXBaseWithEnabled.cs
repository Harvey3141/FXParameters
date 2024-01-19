using FX;
using UnityEngine;

public class FXBaseWithEnabled : FXBase
{
    public FXParameter<bool> fxEnabled = new FXParameter<bool>(false);

    protected virtual void OnFXEnabled(bool state)
    {

    }

    protected virtual void Awake()
    {
        base.Awake();
        fxEnabled.OnValueChanged += OnFXEnabled;
    }

    protected virtual void Start()
    {
        OnFXEnabled(fxEnabled.Value);
    }
}
