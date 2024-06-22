using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FX.Patterns
{
    public class BPMManager : MonoBehaviour
    {
        public float bpm = 120f;
        private float cachedBpm = 0.0f;

        private List<float> tapTimes = new List<float>();
        private float lastTapTime = 0;

        public event Action<float> OnBpmChanged;
        public event Action OnResetPhase;
        public event Action<float> OnBeat;

        private float lastBeatTime;
        [HideInInspector]
        public Color indicatorColor = Color.gray;
        private Coroutine beatIndicatorCoroutine;

        protected void Start()
        {
            OnBpmChanged?.Invoke(bpm);
            lastBeatTime = Time.time;
            OnBeat += HandleBeat;
        }

        public void Tap()
        {
            float currentTime = Time.time;
            if (lastTapTime != 0)
            {
                float timeSinceLastTap = currentTime - lastTapTime;
                float bpmEstimate = 60 / timeSinceLastTap;
                tapTimes.Add(timeSinceLastTap);
                if (tapTimes.Count > 8) // Limit to last 4 taps
                {
                    tapTimes.RemoveAt(0);
                }
                bpm = (tapTimes.Count / tapTimes.Sum() * 60f); // Calculate the average BPM of the last 4 taps
            }
            lastTapTime = currentTime;
        }

        public void ResetPhase()
        {
            OnResetPhase?.Invoke();
            lastBeatTime = Time.time;
        }

        public void DoubleBPM()
        {
            bpm = bpm * 2f;
        }

        public void HalfBPM()
        {
            bpm = bpm * 0.5f;
        }

        private void FixedUpdate()
        {
            float timeSinceLastBeat = Time.time - lastBeatTime;
            if (timeSinceLastBeat >= (60.0f / bpm))
            {
                lastBeatTime = Time.time;
                OnBeat?.Invoke(bpm);
            }

            if (cachedBpm != bpm)
            {
                cachedBpm = bpm;
                OnBpmChanged(bpm);
            }
        }

        private void HandleBeat(float bpm)
        {
            if (beatIndicatorCoroutine != null)
            {
                StopCoroutine(beatIndicatorCoroutine);
            }
            beatIndicatorCoroutine = StartCoroutine(BeatIndicator());
        }

        private IEnumerator BeatIndicator()
        {
            indicatorColor = Color.white;
            yield return new WaitForSeconds(0.1f);
            indicatorColor = Color.gray;
        }
    }
}
