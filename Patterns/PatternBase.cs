using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PatternBase : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    [HideInInspector]
    public float _currentValue;
    protected int _bpm = 120;
    [HideInInspector]
    public float _phase = 0f;

    [HideInInspector]
    public int _numBeats = 1;
    public int NumBeats
    {
        get { return _numBeats; }
        set
        {
            _numBeats = value;
            GeneratePattern();
        }
    }



    public virtual void HandleBpmChange(int number) { _bpm = number; }
    private void HandleResetPhase() { _phase = 0; }


    public virtual void Start()
    {
        TapBpm tapBpm = FindObjectOfType<TapBpm>();
        tapBpm.OnBpmChangeEvent += HandleBpmChange;
        tapBpm.OnResetPhase += HandleResetPhase;
    }

    public virtual void GeneratePattern() { }


}