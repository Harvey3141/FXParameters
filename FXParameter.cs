using System;
using System.Reflection;
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
        //EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), $"Address: {addressProperty.stringValue}");

        // Calculate the position for the value field
        Rect valuePosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
        //Rect valuePosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        string propertyName = ObjectNames.NicifyVariableName(property.name);
        GUIContent valueLabel = new GUIContent(propertyName, "Test");


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
                EditorGUI.BeginChangeCheck();
                string newValue = EditorGUI.TextField(valuePosition, valueLabel, valueProperty.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    valueProperty.stringValue = newValue;
                }
                //valueProperty.stringValue = EditorGUI.TextField(valuePosition, valueLabel, valueProperty.stringValue);
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
        // Calculate the number of lines needed based on the property type
        int lines = 1; // Default to one line
    
        switch (property.FindPropertyRelative("value_").propertyType)
        {
            case SerializedPropertyType.Float:
            case SerializedPropertyType.Integer:
            case SerializedPropertyType.Boolean:
            case SerializedPropertyType.String:
            case SerializedPropertyType.Color:
                lines = 2; // Two lines needed for the value field
                break;
        }
    
        return EditorGUIUtility.singleLineHeight * lines + EditorGUIUtility.standardVerticalSpacing;
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

#if UNITY_EDITOR
    [Tooltip("Address:")]
    public T AddressDisplay
    {
        get { return value_; }
        set { }
    }
#endif
}