using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FX
{

    [CustomPropertyDrawer(typeof(FXParameter<>), true)]
    public class FXParameterDrawer : PropertyDrawer
    {
        private Dictionary<string, IFXParameter> fxParameterCache = new Dictionary<string, IFXParameter>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            IFXParameter fxParam = GetFXParameterFromProperty(property);

            if (fxParam is FXParameter<float> floatParam)
            {
                float newValue = EditorGUI.FloatField(position, label, floatParam.Value);
                if (newValue != floatParam.Value)
                    floatParam.Value = newValue;
            }
            else if (fxParam is FXParameter<bool> boolParam)
            {
                bool newValue = EditorGUI.Toggle(position, label, boolParam.Value);
                if (newValue != boolParam.Value)
                    boolParam.Value = newValue;                                 
            }
            else if (fxParam is FXParameter<int> intParam)
            {
                int newValue = EditorGUI.IntField(position, label, intParam.Value);
                if (newValue != intParam.Value)
                    intParam.Value = newValue;
            }
            else if (fxParam is FXParameter<string> stringParam)
            {
                string newValue = EditorGUI.TextField(position, label, stringParam.Value);
                if (newValue != stringParam.Value)
                    stringParam.Value = newValue;
            }
            else if (fxParam is FXParameter<Color> colorParam)
            {
                Color newValue = EditorGUI.ColorField(position, label, colorParam.Value);
                if (newValue != colorParam.Value)
                    colorParam.Value = newValue;
            }
            else
            {
                EditorGUI.LabelField(position, label, new GUIContent("Unsupported FXParameter type"));
            }
            EditorGUI.EndProperty();

        }

        private IFXParameter GetFXParameterFromProperty(SerializedProperty property)
        {
            string propertyPath = property.propertyPath;

            if (fxParameterCache.TryGetValue(propertyPath, out var cachedParam))
            {
                return cachedParam;
            }

            object targetObject = property.serializedObject.targetObject;
            IFXParameter fxParam = fieldInfo.GetValue(targetObject) as IFXParameter;
            fxParameterCache[propertyPath] = fxParam;

            return fxParam;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }



    [CustomPropertyDrawer(typeof(FXScaledParameter<>), true)]
    public class FXScaledParameterDrawer : PropertyDrawer
    {
        // TODO:
        // Consider using a Dictionary<Type, object> to store the different FXScaledParameter<T> instances.
        bool foundParam = false;
        private FXScaledParameter<float>    fxScaledParamF;
        private FXScaledParameter<Color>    fxScaledParamC;
        private FXScaledParameter<int>      fxScaledParamI;
        private FXScaledParameter<Vector3>  fxScaledParamv3;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw a box around the property
            EditorGUI.HelpBox(position, "", MessageType.None);

            // Get the scaledValue, valueAtZero_, valueAtOne_, and value SerializedProperties
            SerializedProperty scaledValueProperty = property.FindPropertyRelative("scaledValue_");
            SerializedProperty valueAtZeroProperty = property.FindPropertyRelative("valueAtZero_");
            SerializedProperty valueAtOneProperty  = property.FindPropertyRelative("valueAtOne_");
            SerializedProperty valueProperty       = property.FindPropertyRelative("value_");

            // Add a small padding to the positions to fit inside the box
            float padding       = 4f;  // Change this value to adjust the padding
            Rect paddedPosition = new Rect(position.x + padding, position.y + padding, position.width - 2 * padding, position.height - 2 * padding);

            // Calculate the positions for the scaledValue, valueAtZero_, and valueAtOne_ fields
            Rect labelPosition       = new Rect(paddedPosition.x, paddedPosition.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            Rect scaledValuePosition = new Rect(paddedPosition.x + EditorGUIUtility.labelWidth, paddedPosition.y, paddedPosition.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

            // Display the foldout
            property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, "   " +property.displayName, true, EditorStyles.boldLabel);

            // Display the Scaled Value Property
            EditorGUI.PropertyField(scaledValuePosition, scaledValueProperty, GUIContent.none);

            // Display the other fields only if the property is expanded
            if (property.isExpanded)
            {
                GUIContent valueLabel       = new GUIContent("Value");
                GUIContent valueAtZeroLabel = new GUIContent("Value at zero");
                GUIContent valueAtOneLabel  = new GUIContent("Value at one");

                Rect valuePosition       = new Rect(paddedPosition.x, paddedPosition.y + EditorGUIUtility.singleLineHeight + padding, paddedPosition.width, EditorGUIUtility.singleLineHeight);
                Rect valueAtZeroPosition = new Rect(paddedPosition.x, paddedPosition.y + 2 * (EditorGUIUtility.singleLineHeight + padding), paddedPosition.width, EditorGUIUtility.singleLineHeight);
                Rect valueAtOnePosition  = new Rect(paddedPosition.x, paddedPosition.y + 3 * (EditorGUIUtility.singleLineHeight + padding), paddedPosition.width, EditorGUIUtility.singleLineHeight);

                if (!foundParam)
                {
                    object target = fieldInfo.GetValue(property.serializedObject.targetObject);

                    if (target is FXScaledParameter<Color>)
                    {
                        fxScaledParamC = (FXScaledParameter<Color>)target;
                        foundParam = true;
                    }
                    else if (target is FXScaledParameter<float>)
                    {
                        fxScaledParamF = (FXScaledParameter<float>)target;
                        foundParam = true;
                    }
                    else if (target is FXScaledParameter<int>)
                    {
                        fxScaledParamI = (FXScaledParameter<int>)target;
                        foundParam = true;
                    }
                    else if (target is FXScaledParameter<Vector3>)
                    {
                        fxScaledParamv3 = (FXScaledParameter<Vector3>)target;
                        foundParam = true;
                    }
                }

                float oldValue = valueProperty.floatValue;
                float newValue = EditorGUI.Slider(valuePosition, valueLabel, valueProperty.floatValue, 0f, 1f);

                if (oldValue != newValue && foundParam)
                {
                    if      (fxScaledParamC != null) fxScaledParamC.Value = newValue;
                    else if (fxScaledParamF != null) fxScaledParamF.Value = newValue;
                    else if (fxScaledParamI != null) fxScaledParamI.Value = newValue;
                    else if (fxScaledParamv3 != null) fxScaledParamv3.Value = newValue;
                }

                EditorGUI.PropertyField(valueAtZeroPosition, valueAtZeroProperty, valueAtZeroLabel);
                EditorGUI.PropertyField(valueAtOnePosition, valueAtOneProperty, valueAtOneLabel);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // The height is 1 line if the property is collapsed
            // The height is 4 lines for scaledValue, value, valueAtZero_, and valueAtOne_ plus padding for the box if the property is expanded
            int lineCount = property.isExpanded ? 4 : 1;

            // Multiply the number of lines by the height of a single line, add spacing for each line, and add extra padding for the box.
            return (lineCount * EditorGUIUtility.singleLineHeight) + ((lineCount + 1) * EditorGUIUtility.standardVerticalSpacing) + 2f * 4f; // additional 2f*padding for the top and bottom padding of the box
        }

    }

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

        public event Action<T> OnValueChanged; // Event triggered when the value changes

        public virtual T Value
        {
            get { return value_; }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(value_, value)) // Only trigger event if the value has changed
                {
                    value_ = value;
                    //Debug.Log($"New value set: {value_}");
                    OnValueChanged?.Invoke(value_); // Trigger the event
                }
            }
        }

        // Implement IFXParameter interface
        object IFXParameter.ObjectValue
        {
            get { return Value; }
            set
            {
                if (value is T tValue)
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
            set { address_ = value; }
        }

        public bool ShouldSave
        {
            get { return shouldSave_; }
            set { shouldSave_ = value; }
        }

        public FXParameter(T value, string address = "", bool shouldSave = true)
        {
            if (typeof(T) == typeof(float) || typeof(T) == typeof(int) || typeof(T) == typeof(bool) || typeof(T) == typeof(string) || typeof(T) == typeof(Color))
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
                    OnEnabledChanged(value); // Invoke the custom enabled changed handler
                }
            }
        }

        private void OnEnabledChanged(bool isEnabled)
        {
            // Handle the enabled state change here
            Debug.Log($"Enabled value changed: {isEnabled}");
        }
    }
}

