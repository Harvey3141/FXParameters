using UnityEngine;

namespace FX.Patterns
{
    public class ArpeggiatorPattern : PatternBase
    {
        private int numSteps = 4;
        public int NumSteps
        {
            get { return numSteps; }
            set
            {
                if (numSteps != value)
                {
                    numSteps = Mathf.Max(1, value);  
                    GeneratePattern();
                    NotifyPropertyChanged();
                }
            }
        }

        public enum PatternStyle
        {
            Up,
            Down,
            Random
        }

        private PatternStyle style;

        public PatternStyle Style
        {
            get { return style; }
            set
            {
                if (style != value)
                {
                    style = value;
                    GeneratePattern();
                    NotifyPropertyChanged();
                }
            }
        }

        [Range(0.0f, 1.0f)]
        private float probability = 1.0f;
        public float Probability
        {
            get { return probability; }
            set
            {
                probability = Mathf.Clamp(value, 0, 1);
                GeneratePattern();
            }
        }

        private bool[] pattern;
        private float[] values;
        private float nextTriggerTime;
        private float currentValue;
        private int currentStep = 0;

        public override void HandleBpmChange(float number)
        {
            base.HandleBpmChange(number);
            UpdateTriggerInterval();
        }

        public override void Start()
        {
            base.Start();
            GeneratePattern();
            UpdateTriggerInterval();
        }

        void Update()
        {
            if (Time.time >= nextTriggerTime)
            {
                TriggerFunction();
                nextTriggerTime += GetTriggerInterval();
            }
        }

        private void UpdateTriggerInterval()
        {
            nextTriggerTime = Time.time + GetTriggerInterval();
        }

        private float GetTriggerInterval()
        {
            return (60f / bpm) * (numBeats / (float)numSteps);
        }

        public override void GeneratePattern()
        {
            currentStep = 0; 
            pattern = new bool[numSteps];
            values = new float[numSteps];
            currentValue = 0f;
            switch (style)
            {
                case PatternStyle.Up:
                    for (int i = 0; i < numSteps; i++)
                    {
                        pattern[i] = Random.value < probability;
                        values[i] = currentValue;
                        currentValue += 1f / numSteps;
                    }
                    break;
                case PatternStyle.Down:
                    currentValue = 1f; 
                    for (int i = 0; i < numSteps; i++)
                    {
                        pattern[i] = Random.value < probability;
                        values[i] = currentValue;
                        currentValue -= 1f / numSteps;
                        if (currentValue < 0f)
                        {
                            currentValue = 0f; 
                        }
                    }
                    break;
                case PatternStyle.Random:
                    for (int i = 0; i < numSteps; i++)
                    {
                        pattern[i] = Random.value < probability;
                        values[i] = Random.value;
                    }
                    break;
            }
        }

        void TriggerFunction()
        {
            if (pattern.Length > 0 && currentStep < pattern.Length)
            {
                if (pattern[currentStep])
                {
                    _currentValue = values[currentStep];
                }
                currentStep = (currentStep + 1) % numSteps;
            }
        }
    }
}
