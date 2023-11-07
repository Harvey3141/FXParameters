using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FX.Patterns {
    public class TapPattern : PatternBase
    {
        public float _triggerSpeed = 100.0f;
        private bool _isLerping = false;
        private bool _isIncreasing = true;

        [Range(0.0f, 1.0f)]
        [HideInInspector]
        public float _previousPlayhead = 0.0f;

        private float beatDuration; // Duration of each beat in seconds
        private float lastBeatTime; // Time at which the last beat occurred

        public Dictionary<float, bool> triggers;

        public override void Start()
        {
            triggers = new Dictionary<float, bool>();

            base.Start();
            beatDuration = 60f / _bpm; // Calculate the duration of each beat
            lastBeatTime = Time.time;
        }

        public override void HandleBpmChange(int number)
        {
            base.HandleBpmChange(number);
            beatDuration = 60f / _bpm;
        }

        void Update()
        {
            float timeSinceLastBeat = Time.time - lastBeatTime;
            float beatsElapsed = timeSinceLastBeat / beatDuration;
            float barsElapsed = beatsElapsed / _numBeats;
            _phase += barsElapsed;
            lastBeatTime = Time.time;
            // Wrap the timeline position to keep it within the 0-1 range
            _phase %= 1f;

            if (_previousPlayhead > _phase)
            {
                foreach (float key in triggers.Keys.ToList())
                {
                    triggers[key] = true;
                }
            }
            _previousPlayhead = _phase;


            if (_isLerping)
            {
                if (_isIncreasing)
                {
                    _currentValue = Mathf.Lerp(_currentValue, 1.0f, Time.deltaTime * _triggerSpeed);
                    if (_currentValue >= 0.99f)
                    {
                        _isIncreasing = false;
                    }
                }
                else
                {
                    _currentValue = Mathf.Lerp(_currentValue, 0.0f, Time.deltaTime * _triggerSpeed);
                    if (_currentValue <= 0.01f)
                    {
                        _isIncreasing = true;
                        _isLerping = false;
                        _currentValue = 0.0f;
                    }
                }
            }

            foreach (float key in triggers.Keys.ToList())
            {
                if (triggers[key] == true && _phase > key)
                {
                    triggers[key] = false;
                    TriggerLerp();
                }
            }
        }


        public void AddTriggerAtCurrentTime()
        {
            if (triggers == null) return;
            triggers.Add(_phase, false);
            TriggerLerp();
        }

        public void AddTriggers(int n)
        {
            if (triggers == null) return;
            for (int i = 0; i < n; i++)
            {
                triggers.Add((1.0f / n) * i, false);
            }
        }
        public void ClearTriggers()
        {
            if (triggers == null) return;
            triggers.Clear();
        }

        public void TriggerLerp()
        {
            base.Trigger();
            if (!_isLerping)
            {
                _isIncreasing = true;
                _isLerping = true;
            }
            else if (_isLerping)
            {
                if (!_isIncreasing)
                {
                    _isIncreasing = true;
                }
            }
        }
    }

}
