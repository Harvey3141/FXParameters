using System.Collections.Generic;
using UnityEngine;

namespace FX.Patterns
{
    public class OscillatorPattern : PatternBase
    {

        [HideInInspector]
        public List<float> _pattern = new List<float>();
        float _beatDuration;
        float _frequency;
        public enum OscillatorType
        {
            Sine,
            Square,
            Triangle,
            Sawtooth
        }


        private OscillatorType _oscillatorType = OscillatorType.Sine;
        public OscillatorType Oscillator
        {
            get { return _oscillatorType; }
            set { _oscillatorType = value; GeneratePattern(); }
        }

        public override void Start()
        {
            base.Start();
            GeneratePattern();

        }

        public override void GeneratePattern()
        {
            _beatDuration = 60f / _bpm;
            _frequency = 1f / (_beatDuration * _numBeats);
            int steps = 20;
            _pattern = new List<float>();
            for (int i = 0; i < steps; i++)
            {
                float phase = (float)i / steps;
                _pattern.Add(GetCurrentValue(_oscillatorType, phase));
            }
        }

        private void Update()
        {
            _phase += Time.deltaTime * _frequency;
            _phase %= 1f;
            _currentValue = GetCurrentValue(_oscillatorType, _phase);
        }

        public float GetCurrentValue(OscillatorType oscillatorType, float phase)
        {
            float currentValue = 0f;
            switch (oscillatorType)
            {
                case OscillatorType.Sine:
                    float sinValue = (Mathf.Sin((phase * 2.0f * Mathf.PI) - (Mathf.PI * 0.5f)) + 1f) / 2f;
                    currentValue = sinValue;
                    break;
                case OscillatorType.Square:
                    float squareValue = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * phase));
                    currentValue = (squareValue + 1f) / 2f;
                    break;
                case OscillatorType.Triangle:
                    float triangleValue = Mathf.Abs(2f * (phase - Mathf.Floor(0.5f + phase)));
                    currentValue = triangleValue;
                    break;
                case OscillatorType.Sawtooth:
                    float sawtoothValue = phase - Mathf.Floor(phase);
                    currentValue = sawtoothValue;
                    break;
                default:
                    Debug.LogError("Invalid oscillator type!");
                    break;
            }
            return currentValue;
        }

        public float Map(float value, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return (value - inputMin) * (outputMax - outputMin) / (inputMax - inputMin) + outputMin;
        }

        public override void HandleBpmChange(int number)
        {
            base.HandleBpmChange(number);
            _beatDuration = 60f / _bpm;
        }

    }
}

