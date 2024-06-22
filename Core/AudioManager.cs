using UnityEngine;
using Lasp;
using System;

namespace FX
{
    public class AudioManager : MonoBehaviour
    {
        public AudioLevelTracker audioLow;
        public AudioLevelTracker audioMid;
        public AudioLevelTracker audioHigh;
        public TransientDetector transientDetector;

        public event Action onTransient;

        private bool isLerping = false;
        private bool isIncreasing = true;

        public void Awake()
        {
            transientDetector.OnTransient += OnTransient;
        }

        private void OnTransient()
        {
            onTransient?.Invoke();
            TriggerLerp();
        }

        private void Update()
        {
            if (isLerping)
            {
                if (isIncreasing)
                {
                    TransientLevel = Mathf.Lerp(TransientLevel, 1.0f, Time.deltaTime * triggerSpeed);
                    if (TransientLevel >= 0.99f)
                    {
                        isIncreasing = false;
                    }
                }
                else
                {
                    TransientLevel = Mathf.Lerp(TransientLevel, 0.0f, Time.deltaTime * triggerSpeed);
                    if (TransientLevel <= 0.01f)
                    {
                        isIncreasing = true;
                        isLerping = false;
                        TransientLevel = 0.0f;
                    }
                }
            }
        }

        private void TriggerLerp()
        {
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

        public float Low { get; set; }
        public float Mid { get; set; }
        public float High { get; set; }

        public float TransientLevel { get; set; }

        public float triggerSpeed = 9.0f; 
    }
}
