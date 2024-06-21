using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Lasp;
using System;

namespace FX {
    public class AudioManager : MonoBehaviour
    {

        public AudioLevelTracker audioLow;
        public AudioLevelTracker audioMid;
        public AudioLevelTracker audioHigh;
        public TransientDetector transientDetector;

        public event Action onTransient;


        public void Awake()
        {
            transientDetector.OnTransient += OnTransient;
        }

        private void OnTransient() {
            onTransient?.Invoke();

        }

        public float Low { get; set;}
        public float Mid { get; set; }
        public float High { get; set; }

    }
}


