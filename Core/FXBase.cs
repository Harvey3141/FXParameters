using UnityEngine;
using FX;

public abstract class FXBase : MonoBehaviour
{
    public FXParameter<bool> fxEnabled = new FXParameter<bool>(false);
    public string fxAddress = "";

    protected virtual void Awake()
    {
        this.AddFXElements(fxAddress);
        fxEnabled.OnValueChanged += OnFXEnabled;
    }

    protected virtual void Start()
    {
        OnFXEnabled(fxEnabled.Value);
    }

    protected virtual void OnFXEnabled(bool state)
    {
        
    }

}

