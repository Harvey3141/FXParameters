using UnityEngine;

namespace FX.Patterns
{
    public enum PatternStyle
    {
        Up,
        Down,
        Random
    }

    public class ArpeggiatorPattern : PatternBase
    {
        private int numSteps = 16;
        public int NumSteps
        {
            get { return numSteps; }
            set
            {
                numSteps = value;
                GeneratePattern();
            }
        }

        public PatternStyle style;
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

        public override void HandleBpmChange(int number)
        {
            base.HandleBpmChange(number);
        }

        public override void Start()
        {
            base.Start();
            GeneratePattern();
            nextTriggerTime = Time.time + 60f / _bpm;
        }

        void Update()
        {
            if (Time.time >= nextTriggerTime)
            {
                TriggerFunction();
                nextTriggerTime += 60f / _bpm;
            }
        }

        public override void GeneratePattern()
        {
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
                    for (int i = 0; i < numSteps; i++)
                    {
                        pattern[i] = Random.value < probability;
                        values[i] = currentValue;
                        currentValue -= 1f / numSteps;
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
            int index = Mathf.FloorToInt(Time.time * _bpm / 60f) % numSteps;
            if (pattern[index])
            {
                _currentValue = values[index];

            }
        }
    }

}


