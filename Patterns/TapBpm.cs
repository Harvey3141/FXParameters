using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace FX.Patterns
{
    public class TapBpm : MonoBehaviour
    {
        public int _bpm = 120; // The calculated BPM value

        private List<float> _tapTimes = new List<float>(); // List to store tap times
        private float _lastTapTime = 0; // Time of the last tap

        public event UnityAction<int> OnBpmChangeEvent;
        public event UnityAction OnResetPhase;

        private void Start()
        {
            if (OnBpmChangeEvent != null) OnBpmChangeEvent.Invoke(_bpm);
        }

        public void Tap()
        {
            float currentTime = Time.time; // Get the current time
            if (_lastTapTime != 0) // Check if this is not the first tap
            {
                float timeSinceLastTap = currentTime - _lastTapTime; // Calculate time between taps
                float bpmEstimate = 60 / timeSinceLastTap; // Calculate BPM estimate
                _tapTimes.Add(timeSinceLastTap); // Add time between taps to list
                if (_tapTimes.Count > 4) // Limit list to the last 4 taps
                {
                    _tapTimes.RemoveAt(0);
                }
                _bpm = (int)Mathf.Round(_tapTimes.Count / _tapTimes.Sum() * 60); // Calculate the average BPM of the last 4 taps
            }
            _lastTapTime = currentTime; // Update the last tap time

            if (OnBpmChangeEvent != null) OnBpmChangeEvent.Invoke(_bpm);
        }

        public void ResetPhase()
        {
            if (OnResetPhase != null) OnResetPhase.Invoke();
        }


    }

}

