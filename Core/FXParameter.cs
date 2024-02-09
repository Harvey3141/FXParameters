using System;
using System.Collections.Generic;
using UnityEngine;

namespace FX
{

    public interface IFXParameter
    {
        object ObjectValue { get; set; }
        string Address { get; set; }
        bool ShouldSave { get; set; }
    }

    [System.Serializable]
    public class FXParameter<T> : IFXParameter
    {
        [SerializeField]
        private string address_;
        [SerializeField]
        private T value_;
        [SerializeField]
        private bool shouldSave_ = true;

        [SerializeField]
        private T minValue_;
        [SerializeField]
        private T maxValue_;
        [SerializeField]
        private bool hasMinValue_ = false;
        [SerializeField]
        private bool hasMaxValue_ = false;

        public event Action<T> OnValueChanged; // Event triggered when the value changes

        public virtual T Value
        {
            get { return value_; }
            set
            {
                T newValue = value;

                if (hasMinValue_ && Comparer<T>.Default.Compare(newValue, minValue_) < 0)
                {
                    newValue = minValue_;
                }
                if (hasMaxValue_ && Comparer<T>.Default.Compare(newValue, maxValue_) > 0)
                {
                    newValue = maxValue_;
                }

                if (!EqualityComparer<T>.Default.Equals(value_, newValue)) 
                {
                    value_ = newValue;
                    OnValueChanged?.Invoke(value_); 
                }
            }
        }

        object IFXParameter.ObjectValue
        {
            get { return Value; }
            set
            {
                if (typeof(T).IsEnum && value is int intValue)
                {
                    Value = (T)Enum.ToObject(typeof(T), intValue);
                }
                else if (value is T tValue)
                {
                    Value = tValue;
                }
                else
                {
                    throw new ArgumentException($"Value must be of type {typeof(T).Name}");
                }
            }
        }

        public string Address
        {
            get { return address_; }
            set { 
                address_ = value; 
            }
        }

        public bool ShouldSave
        {
            get { return shouldSave_; }
            set { shouldSave_ = value; }
        }

        public FXParameter(T value, string address = "", bool shouldSave = true)
        {
            if (typeof(T) == typeof(float) || typeof(T) == typeof(int) || typeof(T) == typeof(bool) || typeof(T) == typeof(string) || typeof(T) == typeof(Color) || typeof(T).IsEnum)
            {
                Value = value;
                ShouldSave = shouldSave;

                if (string.IsNullOrEmpty(address))
                {
                    //throw new ArgumentException("Address must be provided.");
                }
                else
                {
                    address_ = address;
                }
            }
            else
            {
                throw new ArgumentException("FXParameter supports only float, int, bool, string, and Color types.");
            }
        }

        public FXParameter(T value, T minValue, T maxValue, string address = "", bool shouldSave = true) : this(value, address, shouldSave)
        {
            SetMinValue(minValue);
            SetMaxValue(maxValue);
        }

        public bool HasMinValue
        {
            get { return hasMinValue_; }
            
        }

        public bool HasMaxValue
        {
            get { return hasMaxValue_; }
        }

        public void SetMinValue(T minValue)
        {
            minValue_ = minValue;
            hasMinValue_ = true;
            Value = value_; 
        }

        public void SetMaxValue(T maxValue)
        {
            maxValue_ = maxValue;
            hasMaxValue_ = true;
            Value = value_; 
        }

        public T GetMinValue()
        {
            return minValue_;
        }

        public T GetMaxValue()
        {
            return maxValue_;
        }

    }

    [System.Serializable]
    public class FXScaledParameter<T> : FXParameter<float>
    {
        [SerializeField]
        private T valueAtZero_;
        [SerializeField]
        private T valueAtOne_;
        [SerializeField]
        private T scaledValue_;

        public event Action<T> OnScaledValueChanged; // Event triggered when the value changes

        public FXScaledParameter(float value, T valueAtZero, T valueAtOne, string address = "", bool shouldSave = true)
            : base(value, address, shouldSave)
        {
            valueAtZero_ = valueAtZero;
            valueAtOne_ = valueAtOne;
            UpdateScaledValue();
        }

        public override float Value
        {
            get { return base.Value; }
            set
            {
                base.Value = value;
                UpdateScaledValue();
                OnScaledValueChanged?.Invoke(scaledValue_);
            }
        }

        public T ScaledValue
        {
            get { return scaledValue_; }
            private set { scaledValue_ = value; }
        }

        public T ValueAtZero
        {
            get { return valueAtZero_; }
            private set { valueAtZero_ = value; }
        }

        public T ValueAtOne
        {
            get { return valueAtOne_; }
            private set { valueAtOne_ = value; }
        }

        private void UpdateScaledValue()
        {
            if (valueAtZero_ != null && valueAtOne_ != null)
            {
                if (typeof(T) == typeof(Color))
                {
                    Color zeroColor = (Color)Convert.ChangeType(valueAtZero_, typeof(Color));
                    Color oneColor  = (Color)Convert.ChangeType(valueAtOne_, typeof(Color));

                    float r = Mathf.Lerp(zeroColor.r, oneColor.r, Value);
                    float g = Mathf.Lerp(zeroColor.g, oneColor.g, Value);
                    float b = Mathf.Lerp(zeroColor.b, oneColor.b, Value);
                    float a = Mathf.Lerp(zeroColor.a, oneColor.a, Value);

                    scaledValue_ = (T)(object)new Color(r, g, b, a);
                }
                else if (typeof(T) == typeof(float))
                {
                    float zeroValue = (float)Convert.ChangeType(valueAtZero_, typeof(float));
                    float oneValue  = (float)Convert.ChangeType(valueAtOne_, typeof(float));
                    scaledValue_    = (T)(object)Mathf.Lerp(zeroValue, oneValue, Value);
                }
                else if (typeof(T) == typeof(int))
                {
                    float zeroValue   = (float)Convert.ChangeType(valueAtZero_, typeof(int));
                    float oneValue    = (float)Convert.ChangeType(valueAtOne_, typeof(int));
                    float lerpedValue = Mathf.Lerp(zeroValue, oneValue, Value);
                    scaledValue_      = (T)(object)Mathf.RoundToInt(lerpedValue);
                }
                else if (typeof(T) == typeof(Vector3))
                {
                    Vector3 zeroVector = (Vector3)Convert.ChangeType(valueAtZero_, typeof(Vector3));
                    Vector3 oneVector  = (Vector3)Convert.ChangeType(valueAtOne_, typeof(Vector3));

                    float x = Mathf.Lerp(zeroVector.x, oneVector.x, Value);
                    float y = Mathf.Lerp(zeroVector.y, oneVector.y, Value);
                    float z = Mathf.Lerp(zeroVector.z, oneVector.z, Value);

                    scaledValue_ = (T)(object)new Vector3(x, y, z);
                }
                else
                {
                    throw new ArgumentException($"FXScaledParameter does not support scaling for type {typeof(T).Name}");
                }
            }
        }
    }



    [System.Serializable]
    public class FXEnabledParameter : FXParameter<bool>
    {
        public FXEnabledParameter(bool value) : base(value)
        {
        }

        public override bool Value
        {
            get { return base.Value; }
            set
            {
                if (base.Value != value)
                {
                    base.Value = value;
                    OnEnabledChanged(value); 
                }
            }
        }

        private void OnEnabledChanged(bool isEnabled)
        {
            Debug.Log($"Enabled value changed: {isEnabled}");
        }
    }
}

