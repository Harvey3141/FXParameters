using System;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(FXParameter<>))]
public class FXParameterDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Get the value and address SerializedProperties
        SerializedProperty valueProperty = property.FindPropertyRelative("value_");
        SerializedProperty addressProperty = property.FindPropertyRelative("address_");

        // Display the address as a label
        EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), $"Address: {addressProperty.stringValue}");

        // Calculate the position for the value field
        Rect valuePosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);

        string propertyName = ObjectNames.NicifyVariableName(property.name);
        // Display the value field based on the type of the FXParameter
        switch (valueProperty.propertyType)
        {
            case SerializedPropertyType.Float:
                valueProperty.floatValue = EditorGUI.FloatField(valuePosition, propertyName, valueProperty.floatValue);
                break;
            case SerializedPropertyType.Integer:
                valueProperty.intValue = EditorGUI.IntField(valuePosition, "Value", valueProperty.intValue);
                break;
            case SerializedPropertyType.Boolean:
                valueProperty.boolValue = EditorGUI.Toggle(valuePosition, "Value", valueProperty.boolValue);
                break;
            case SerializedPropertyType.String:
                valueProperty.stringValue = EditorGUI.TextField(valuePosition, "Value", valueProperty.stringValue);
                break;
            case SerializedPropertyType.Color:
                valueProperty.colorValue = EditorGUI.ColorField(valuePosition, "Value", valueProperty.colorValue);
                break;
            default:
                EditorGUI.LabelField(valuePosition, "Value type not supported");
                break;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}

[System.Serializable]
public class FXParameter<T>
{
    [SerializeField]
    private string address_;
    [SerializeField]
    private T value_;

    public T Value
    {
        get { return value_; }
        set
        {
            value_ = value;
            Debug.Log($"New value set: {value_}");
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