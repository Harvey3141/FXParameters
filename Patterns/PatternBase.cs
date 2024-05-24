using System;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;

namespace FX.Patterns
{
    public class PatternBase : MonoBehaviour
    {
        [Range(0.0f, 1.0f)]
        [HideInInspector]
        public float _currentValue;
        protected float bpm = 120f;
        [HideInInspector]
        public float phase = 0f;

        [HideInInspector]
        public float numBeats = 1;

        public event Action OnTrigger;
        public float NumBeats
        {
            get { return numBeats; }
            set
            {
                if (numBeats != value)
                {
                    numBeats = value;
                    NotifyPropertyChanged();
                }
                GeneratePattern();
            }
        }

        public void Trigger()
        {
            OnTrigger.Invoke();
        }

        public delegate void PropertyChanged();
        public event PropertyChanged OnPropertyChanged;

        public virtual void HandleBpmChange(float value) { bpm = value; }
        private void HandleResetPhase() { phase = 0; }


        public virtual void Start()
        {
            BPMManager tapBpm = FindObjectOfType<BPMManager>();
            tapBpm.OnBpmChanged += HandleBpmChange;
            tapBpm.OnResetPhase += HandleResetPhase;

        }

        public virtual void GeneratePattern() { }

        protected void NotifyPropertyChanged()
        {
            OnPropertyChanged?.Invoke();
        }

    }

}
