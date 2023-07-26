using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace FX
{
    [CustomPropertyDrawer(typeof(FXParameter<>))]
    public class FXParameterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get the value and address SerializedProperties
            SerializedProperty valueProperty    = property.FindPropertyRelative("value_");
            SerializedProperty addressProperty  = property.FindPropertyRelative("address_");

            // Display the address as a label
            //EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), $"Address: {addressProperty.stringValue}");

            // Calculate the position for the value field
            Rect valuePosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect addressPosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);


            string propertyName = ObjectNames.NicifyVariableName(property.name);
            GUIContent valueLabel = new GUIContent(propertyName, addressProperty.stringValue);

            // Display the value field based on the type of the FXParameter
            switch (valueProperty.propertyType)
            {
                case SerializedPropertyType.Float:
                    valueProperty.floatValue = EditorGUI.FloatField(valuePosition, valueLabel, valueProperty.floatValue);
                    break;
                case SerializedPropertyType.Integer:
                    valueProperty.intValue = EditorGUI.IntField(valuePosition, valueLabel, valueProperty.intValue);
                    break;
                case SerializedPropertyType.Boolean:
                    valueProperty.boolValue = EditorGUI.Toggle(valuePosition, valueLabel, valueProperty.boolValue);
                    break;
                case SerializedPropertyType.String:
                    valueProperty.stringValue = EditorGUI.TextField(valuePosition, valueLabel, valueProperty.stringValue);
                    break;
                case SerializedPropertyType.Color:
                    valueProperty.colorValue = EditorGUI.ColorField(valuePosition, valueLabel, valueProperty.colorValue);
                    break;
                default:
                    EditorGUI.LabelField(valuePosition, "Value type not supported");
                    break;
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // The default height is 1 line.
            int lineCount = 1;

            // When the property is expanded and Show Addresses is enabled, we display two fields, hence 2 lines.
            if (property.isExpanded)
            {
                lineCount = 2;
            }

            // Multiply the number of lines by the height of a single line and add a small padding.
            return lineCount * EditorGUIUtility.singleLineHeight + (lineCount - 1) * EditorGUIUtility.standardVerticalSpacing;
        }

    }

    [CustomPropertyDrawer(typeof(FXScaledParameter<>))]
    public class FXScaledParameterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw a box around the property
            EditorGUI.HelpBox(position, "", MessageType.None);

            // Get the scaledValue, valueAtZero_, and valueAtOne_ SerializedProperties
            SerializedProperty scaledValueProperty = property.FindPropertyRelative("scaledValue_");
            SerializedProperty valueAtZeroProperty = property.FindPropertyRelative("valueAtZero_");
            SerializedProperty valueAtOneProperty = property.FindPropertyRelative("valueAtOne_");

            // Add a small padding to the positions to fit inside the box
            float padding = 4f;  // Change this value to adjust the padding
            Rect paddedPosition = new Rect(position.x + padding, position.y + padding, position.width - 2 * padding, position.height - 2 * padding);

            // Calculate the positions for the scaledValue, valueAtZero_, and valueAtOne_ fields
            Rect scaledValuePosition = new Rect(paddedPosition.x, paddedPosition.y, paddedPosition.width, EditorGUIUtility.singleLineHeight);

            // Create the GUIContent for each field
            GUIContent valueAtZeroLabel = new GUIContent("Value At Zero");
            GUIContent valueAtOneLabel = new GUIContent("Value At One");

            // Display the label for the property in bold and make it a foldout
            property.isExpanded = EditorGUI.Foldout(scaledValuePosition, property.isExpanded, property.displayName, true, EditorStyles.boldLabel);

            // Offset the position of the scaledValueProperty to not overlap with the label
            scaledValuePosition = new Rect(scaledValuePosition.x + EditorGUIUtility.labelWidth, scaledValuePosition.y, scaledValuePosition.width - EditorGUIUtility.labelWidth, scaledValuePosition.height);


            // Display the fields
            EditorGUI.PropertyField(scaledValuePosition, scaledValueProperty, GUIContent.none);  // Display the property without a label

            // Display the other fields only if the property is expanded
            if (property.isExpanded)
            {
                Rect valueAtZeroPosition = new Rect(paddedPosition.x, paddedPosition.y + EditorGUIUtility.singleLineHeight + padding, paddedPosition.width, EditorGUIUtility.singleLineHeight);
                Rect valueAtOnePosition = new Rect(paddedPosition.x, paddedPosition.y + 2 * (EditorGUIUtility.singleLineHeight + padding), paddedPosition.width, EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(valueAtZeroPosition, valueAtZeroProperty, valueAtZeroLabel);
                EditorGUI.PropertyField(valueAtOnePosition, valueAtOneProperty, valueAtOneLabel);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // The height is 1 line if the property is collapsed
            // The height is 3 lines for scaledValue, valueAtZero_, and valueAtOne_ plus padding for the box if the property is expanded
            int lineCount = property.isExpanded ? 3 : 1;

            // Multiply the number of lines by the height of a single line, add spacing for each line, and add extra padding for the box.
            return (lineCount * EditorGUIUtility.singleLineHeight) + ((lineCount + 1) * EditorGUIUtility.standardVerticalSpacing) + 2f * 4f; // additional 2f*padding for the top and bottom padding of the box
        }
    }







    [CustomPropertyDrawer(typeof(FXEnabledParameter))]
    public class FXEnabledParameterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get the value SerializedProperty
            SerializedProperty valueProperty = property.FindPropertyRelative("value_");

            // Display the value field
            switch (valueProperty.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    valueProperty.boolValue = EditorGUI.Toggle(position, label, valueProperty.boolValue);
                    break;
                default:
                    EditorGUI.LabelField(position, "Value type not supported");
                    break;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
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
        //private bool valueAt0 = true;

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
                    Color oneColor = (Color)Convert.ChangeType(valueAtOne_, typeof(Color));

                    float r = Mathf.Lerp(zeroColor.r, oneColor.r, Value);
                    float g = Mathf.Lerp(zeroColor.g, oneColor.g, Value);
                    float b = Mathf.Lerp(zeroColor.b, oneColor.b, Value);
                    float a = Mathf.Lerp(zeroColor.a, oneColor.a, Value);

                    scaledValue_ = (T)(object)new Color(r, g, b, a);
                }
                else if (typeof(T) == typeof(float))
                {
                    float zeroValue = (float)Convert.ChangeType(valueAtZero_, typeof(float));
                    float oneValue = (float)Convert.ChangeType(valueAtOne_, typeof(float));
                    scaledValue_ = (T)(object)Mathf.Lerp(zeroValue, oneValue, Value);
                }
                else if (typeof(T) == typeof(int))
                {
                    float zeroValue = (float)Convert.ChangeType(valueAtZero_, typeof(int));
                    float oneValue = (float)Convert.ChangeType(valueAtOne_, typeof(int));
                    float lerpedValue = Mathf.Lerp(zeroValue, oneValue, Value);
                    scaledValue_ = (T)(object)Mathf.RoundToInt(lerpedValue);
                }
                else if (typeof(T) == typeof(Vector3))
                {
                    Vector3 zeroVector = (Vector3)Convert.ChangeType(valueAtZero_, typeof(Vector3));
                    Vector3 oneVector = (Vector3)Convert.ChangeType(valueAtOne_, typeof(Vector3));

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

