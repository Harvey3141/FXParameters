using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FX.Patterns {
    public class TapPattern : PatternBase
    {
        public float triggerSpeed = 100.0f;
        private bool isLerping = false;
        private bool isIncreasing = true;

        [Range(0.0f, 1.0f)]
        [HideInInspector]
        public float previousPlayhead = 0.0f;

        private float beatDuration; // Duration of each beat in seconds
        private float lastBeatTime; 

        public Dictionary<float, bool> triggers;

        public override void Start()
        {
            triggers = new Dictionary<float, bool>();

            AddTriggers(1);

            base.Start();
            beatDuration = 60f / bpm; // Calculate the duration of each beat
            lastBeatTime = Time.time;
        }

        public override void HandleBpmChange(float number)
        {
            base.HandleBpmChange(number);
            beatDuration = 60f / bpm;
        }

        void Update()
        {
            float timeSinceLastBeat = Time.time - lastBeatTime;
            float beatsElapsed = timeSinceLastBeat / beatDuration;
            float barsElapsed = beatsElapsed / numBeats;
            phase += barsElapsed;
            lastBeatTime = Time.time;
            // Wrap the timeline position to keep it within the 0-1 range
            phase %= 1f;

            if (previousPlayhead > phase)
            {
                if (triggers != null) {
                    foreach (float key in triggers.Keys.ToList())
                    {
                        triggers[key] = true;
                    }
                }

            }
            previousPlayhead = phase;


            if (isLerping)
            {
                if (isIncreasing)
                {
                    _currentValue = Mathf.Lerp(_currentValue, 1.0f, Time.deltaTime * triggerSpeed);
                    if (_currentValue >= 0.99f)
                    {
                        isIncreasing = false;
                    }
                }
                else
                {
                    _currentValue = Mathf.Lerp(_currentValue, 0.0f, Time.deltaTime * triggerSpeed);
                    if (_currentValue <= 0.01f)
                    {
                        isIncreasing = true;
                        isLerping = false;
                        _currentValue = 0.0f;
                    }
                }
            }

            foreach (float key in triggers.Keys.ToList())
            {
                if (triggers[key] == true && phase > key)
                {
                    triggers[key] = false;
                    TriggerLerp();
                }
            }
        }


        public void AddTriggerAtCurrentTime()
        {
            if (triggers == null) return;
            triggers.Add(phase, false);
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
            if (!isLerping)
            {
                isIncreasing = true;
                isLerping = true;
            }
            else if (isLerping)
            {
                if (!isIncreasing)
                {
                    isIncreasing = true;
                }
            }
        }
    }

}
