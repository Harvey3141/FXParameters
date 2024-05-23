using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FX;
using UnityEditor;

public class FXLerpTransform : FXBase, IFXTriggerable
{
    public FXScaledParameter<float> lerpProgress = new FXScaledParameter<float>(0.0f,0.0f,1.0f);
    public FXParameter<bool> reverse = new FXParameter<bool>(false);

    [HideInInspector]
    public Vector3 positionA;
    [HideInInspector]
    public Vector3 rotationA;
    [HideInInspector]
    public Vector3 scaleA;

    [HideInInspector]
    public Vector3 positionB;
    [HideInInspector]
    public Vector3 rotationB;
    [HideInInspector]
    public Vector3 scaleB;

    protected override void Awake()
    {
        base.Awake();

    }

    protected override void Start()
    {
        base.Start();
        lerpProgress.OnValueChanged += SetLerpProgress;
    }


    void SetLerpProgress(float val) 
    { 
        Vector3 targetPosA = reverse.Value ? positionA : positionB;
        Vector3 targetPosB = reverse.Value ? positionB : positionA;
        transform.position   = Vector3.Lerp(targetPosA, targetPosB, val);
        transform.rotation   = Quaternion.Euler(Vector3.Lerp(rotationA, rotationB, val));
        transform.localScale = Vector3.Lerp(scaleA, scaleB, val);
    }

    [FXMethod]
    public void FXTrigger() 
    {
        SetLerpProgress(Random.value);
    }

}
