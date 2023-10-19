using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Lasp;

namespace FX {
    public class AudioManager : MonoBehaviour
    {

        public AudioLevelTracker audioLow;
        public AudioLevelTracker audioMid;
        public AudioLevelTracker audioHigh;

        public float Low { get; set;}
        public float Mid { get; set; }
        public float High { get; set; }

    }
}


