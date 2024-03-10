using UnityEngine;
using FX;

public abstract class FXBase : MonoBehaviour
{
    [SerializeField]
    public string fxAddress = "";

    protected virtual void Awake()
    {
        fxAddress = this.AddFXElements(fxAddress);
    }

    protected virtual void Start()
    {

    }
    protected virtual void OnDestroy()
    {
        this.RemoveFXElements();
    }

}

