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
        void ResetToDefaultValue();

    }

    public enum AffectorFunction
    {
        Linear,
        EaseIn,
        EaseOut,
        Randomise
    }

    [System.Serializable]
    public class FXParameterData<T>
    {
        public string key;
        public T value;
        public T defaultValue;
        public T minValue;
        public T maxValue;
        public bool hasMinValue = false;
        public bool hasMaxValue = false;

        // Settings specific to FXScaledParamers 
        public bool isScaled = false;
        public AffectorFunction affector = AffectorFunction.Linear;
        public bool isInverted = false;
    }


    [System.Serializable]
    public class FXEnumParameterData : FXParameterData<int>
    {
        public List<string> availableNames = new List<string>();
    }


    [System.Serializable]
    public class FXParameter<T> : IFXParameter
    {
        [SerializeField]
        protected string address_;
        [SerializeField]
        private T value_;
        private T defaultValue_;

        [SerializeField]
        private bool shouldSave_ = true;


        private T minValue_;
        private T maxValue_;
        private bool hasMinValue_ = false;
        private bool hasMaxValue_ = false;

        public event Action<T> OnValueChanged;

        protected FXManager fxManager_;

        public virtual T Value
        {
            get { return value_; }
            set
            {
                T newValue = value;

                // TODO - fix this 
                //if (hasMinValue_ && Comparer<T>.Default.Compare(newValue, minValue_) < 0)
                //{
                //    newValue = minValue_;
                //}
                //if (hasMaxValue_ && Comparer<T>.Default.Compare(newValue, maxValue_) > 0)
                //{
                //    newValue = maxValue_;
                //}

                if (!EqualityComparer<T>.Default.Equals(value_, newValue))
                {
                    value_ = newValue;
                    OnValueChanged?.Invoke(value_);
                    fxManager_.OnParameterValueChanged(address_, value);
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
            set
            {
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
            fxManager_ = FXManager.Instance;
            if (typeof(T) == typeof(float) || typeof(T) == typeof(int) || typeof(T) == typeof(bool) || typeof(T) == typeof(string) || typeof(T) == typeof(Color) || typeof(T).IsEnum)
            {
                Value = value;
                defaultValue_ = value;
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
        }

        public void SetMaxValue(T maxValue)
        {
            maxValue_ = maxValue;
            hasMaxValue_ = true;
        }

        public T GetMinValue()
        {
            return minValue_;
        }

        public T GetMaxValue()
        {
            return maxValue_;
        }

        public void ResetToDefaultValue()
        {
            Value = defaultValue_;
        }

        public T GetDefaultValue()
        {
            return defaultValue_;
        }

        public virtual FXParameterData<T> GetData()
        {
            return new FXParameterData<T>
            {
                key          = this.Address,
                value        = this.Value,
                defaultValue = this.GetDefaultValue(),
                minValue     = this.GetMinValue(),
                maxValue     = this.GetMaxValue(),
                hasMinValue  = this.HasMinValue,
                hasMaxValue  = this.HasMaxValue,
                isScaled     = false
            };
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

        [SerializeField]
        private AffectorFunction affectorFunction_ = AffectorFunction.Linear;

        bool invertValue_ = false;

        public event Action<T> OnScaledValueChanged;

        public FXScaledParameter(float value, T valueAtZero, T valueAtOne, string address = "", bool shouldSave = true)
            : base(value, 0.0f, 1.0f, address, shouldSave)
        {
            valueAtZero_ = valueAtZero;
            valueAtOne_ = valueAtOne;
            base.SetMinValue(0.0f);
            base.SetMaxValue(1.0f);

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
                fxManager_.OnParameterValueChanged(address_, value);
            }
        }

        public AffectorFunction AffectorFunction
        {
            get => affectorFunction_;
            set
            {
                affectorFunction_ = value;
                fxManager_.OnParameterAffertorChanged(address_, affectorFunction_);

                Value = Value;
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
            set { valueAtZero_ = value; }
        }

        public T ValueAtOne
        {
            get { return valueAtOne_; }
            set { valueAtOne_ = value; }
        }

        public bool InvertValue
        {
            get { return invertValue_; }
            set { 
                invertValue_ = value;
                Value = Value;
            }
        }

        private void UpdateScaledValue()
        {
            if (valueAtZero_ != null && valueAtOne_ != null)
            {
                float affectedValue = (invertValue_ ? (1.0f - Mathf.Clamp01(Value)) : Mathf.Clamp01(Value));

                switch (affectorFunction_)
                {
                    case AffectorFunction.Linear:
                        break;
                    case AffectorFunction.EaseIn:
                        affectedValue = Mathf.Pow(affectedValue, 2);
                        break;
                    case AffectorFunction.EaseOut:
                        affectedValue = Mathf.Sqrt(affectedValue);
                        break;
                    case AffectorFunction.Randomise:
                        affectedValue = UnityEngine.Random.Range(0f, 1f);
                        break;
                }

                if (typeof(T) == typeof(Color))
                {
                    Color zeroColor = (Color)Convert.ChangeType(valueAtZero_, typeof(Color));
                    Color oneColor  = (Color)Convert.ChangeType(valueAtOne_, typeof(Color));

                    float r = Mathf.Lerp(zeroColor.r, oneColor.r, affectedValue);
                    float g = Mathf.Lerp(zeroColor.g, oneColor.g, affectedValue);
                    float b = Mathf.Lerp(zeroColor.b, oneColor.b, affectedValue);
                    float a = Mathf.Lerp(zeroColor.a, oneColor.a, affectedValue);

                    scaledValue_ = (T)(object)new Color(r, g, b, a);
                }
                else if (typeof(T) == typeof(float))
                {
                    float zeroValue = (float)Convert.ChangeType(valueAtZero_, typeof(float));
                    float oneValue  = (float)Convert.ChangeType(valueAtOne_, typeof(float));
                    scaledValue_    = (T)(object)Mathf.Lerp(zeroValue, oneValue, affectedValue);
                }
                else if (typeof(T) == typeof(int))
                {
                    float zeroValue   = (float)Convert.ChangeType(valueAtZero_, typeof(int));
                    float oneValue    = (float)Convert.ChangeType(valueAtOne_, typeof(int));
                    float lerpedValue = Mathf.Lerp(zeroValue, oneValue, affectedValue);
                    scaledValue_      = (T)(object)Mathf.RoundToInt(lerpedValue);
                }
                else if (typeof(T) == typeof(Vector3))
                {
                    Vector3 zeroVector = (Vector3)Convert.ChangeType(valueAtZero_, typeof(Vector3));
                    Vector3 oneVector  = (Vector3)Convert.ChangeType(valueAtOne_, typeof(Vector3));

                    float x = Mathf.Lerp(zeroVector.x, oneVector.x, affectedValue);
                    float y = Mathf.Lerp(zeroVector.y, oneVector.y, affectedValue);
                    float z = Mathf.Lerp(zeroVector.z, oneVector.z, affectedValue);

                    scaledValue_ = (T)(object)new Vector3(x, y, z);
                }
                else
                {
                    throw new ArgumentException($"FXScaledParameter does not support scaling for type {typeof(T).Name}");
                }
            }
        }

        public override FXParameterData<float> GetData()
        {
            return new FXParameterData<float>
            {
                key          = this.Address,
                value        = this.Value, 
                defaultValue = this.GetDefaultValue(), 
                minValue     = this.GetMinValue(),
                maxValue     = this.GetMaxValue(),
                hasMinValue  = this.HasMinValue,
                hasMaxValue  = this.HasMaxValue,
                affector     = this.AffectorFunction,
                isInverted   = this.InvertValue,
                isScaled     = true
            };

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

