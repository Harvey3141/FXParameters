using UnityEngine;
using FX;

public abstract class FXBase : MonoBehaviour
{
    [SerializeField]
    public string fxAddress = "";
    private bool isQuitting = false;


    protected virtual void Awake()
    {
        fxAddress = this.AddFXElements(fxAddress);
    }

    protected virtual void Start()
    {

    }
    protected virtual void OnDestroy()
    {
        if (!isQuitting)
        {
            this.RemoveFXElements();
        }
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }


}

