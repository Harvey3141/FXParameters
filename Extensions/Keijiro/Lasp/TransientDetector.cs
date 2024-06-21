using UnityEngine;
using System;


namespace Lasp
{
    public class TransientDetector : MonoBehaviour
    {
        [SerializeField] SpectrumAnalyzer spectrumAnalyzer;
        [SerializeField] float sensitivity = 1.5f; 
        [SerializeField] float threshold = 0.1f;   

        private float[] previousSpectrum;
        private bool transientDetected = false;

        public event Action OnTransient;

        void Start()
        {
            if (spectrumAnalyzer == null)
            {
                Debug.LogError("SpectrumAnalyzer is not assigned!");
                enabled = false;
                return;
            }

            previousSpectrum = new float[spectrumAnalyzer.resolution];
        }

        void Update()
        {
            DetectTransients();
        }

        private void DetectTransients()
        {
            var currentSpectrum = spectrumAnalyzer.spectrumSpan;

            for (int i = 0; i < currentSpectrum.Length; i++)
            {
                float change = Mathf.Abs(currentSpectrum[i] - previousSpectrum[i]);

                if (change > threshold && change > sensitivity * previousSpectrum[i])
                {
                    OnTransientDetected();
                    break;
                }

                previousSpectrum[i] = currentSpectrum[i];
            }
        }

        private void OnTransientDetected()
        {
            if (!transientDetected)
            {
                transientDetected = true;
                OnTransient?.Invoke();

                Invoke("ResetTransientDetection", 0.1f);
            }
        }

        private void ResetTransientDetection()
        {
            transientDetected = false;
        }
    }
}
