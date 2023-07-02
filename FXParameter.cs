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

            //property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            //if (property.isExpanded)
            //{
            //    // Indent the address field to make it clear it's a child field
            //    EditorGUI.indentLevel++;
            //    EditorGUI.PropertyField(addressPosition, addressProperty);
            //    EditorGUI.indentLevel--;
            //}


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
    }

    [System.Serializable]
    public class FXParameter<T> : IFXParameter
    {
        [SerializeField]
        private string address_;
        [SerializeField]
        private T value_;

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

        public FXParameter(T value, string address = "")
        {
            if (typeof(T) == typeof(float) || typeof(T) == typeof(int) || typeof(T) == typeof(bool) || typeof(T) == typeof(string) || typeof(T) == typeof(Color))
            {
                Value = value;

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

