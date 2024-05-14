using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace FX.Patterns
{
    public class TapBpm : FXBase
    {
        public int _bpm = 120; 

        private List<float> _tapTimes = new List<float>();
        private float _lastTapTime = 0; 

        public event UnityAction<int> OnBpmChangeEvent;
        public event UnityAction OnResetPhase;

        private float lastBeatTime; 

        // TODO
        // - change to c# events
        // - bpm identifier in UI and via OSC
        // - condition for tapping after a delay 

        protected override void Start()
        {
            base.Start();
            if (OnBpmChangeEvent != null) OnBpmChangeEvent.Invoke(_bpm);

            lastBeatTime = Time.time;
        }

        [FXMethod]
        public void Tap()
        {
            float currentTime = Time.time; 
            if (_lastTapTime != 0) 
            {
                float timeSinceLastTap = currentTime - _lastTapTime;
                float bpmEstimate = 60 / timeSinceLastTap; 
                _tapTimes.Add(timeSinceLastTap);
                if (_tapTimes.Count > 8) // Limit list to the last 4 taps
                {
                    _tapTimes.RemoveAt(0);
                }
                _bpm = (int)Mathf.Round(_tapTimes.Count / _tapTimes.Sum() * 60); // Calculate the average BPM of the last 4 taps
            }
            _lastTapTime = currentTime; 

            if (OnBpmChangeEvent != null) OnBpmChangeEvent.Invoke(_bpm);
        }

        [FXMethod]
        public void ResetPhase()
        {
            if (OnResetPhase != null) OnResetPhase.Invoke();
            lastBeatTime = Time.time;
        }

        private void Update()
        {
            float timeSinceLastBeat = Time.time - lastBeatTime;
            if (timeSinceLastBeat >= (60.0f / _bpm)) {
                lastBeatTime = Time.time;
                //Debug.Log("YESH");
            }
        }
    }

}

