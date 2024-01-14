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
        private FXScaledParameter<         float> fxScaledParamF;
        private FXScaledParameter<Color>   fxScaledParamC;
        private FXScaledParameter<int>     fxScaledParamI;
        private FXScaledParameter<Vector3> fxScaledParamv3;

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
            float padding = 4f;  // Change this value to adjust the padding
            Rect paddedPosition = new Rect(position.x + padding, position.y + padding, position.width - 2 * padding, position.height - 2 * padding);

            // Calculate the positions for the scaledValue, valueAtZero_, and valueAtOne_ fields
            Rect labelPosition       = new Rect(paddedPosition.x, paddedPosition.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            Rect scaledValuePosition = new Rect(paddedPosition.x + EditorGUIUtility.labelWidth, paddedPosition.y, paddedPosition.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

            // Display the foldout
            property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, "   " + property.displayName, true, EditorStyles.boldLabel);

            float buttonWidth   = 20f; // Width of the button, adjust as needed
            float fieldWidth    = scaledValuePosition.width - buttonWidth - padding;
            Rect fieldPosition  = new Rect(scaledValuePosition.x, scaledValuePosition.y, fieldWidth, scaledValuePosition.height);
            Rect buttonPosition = new Rect(fieldPosition.xMax + padding, scaledValuePosition.y, buttonWidth, scaledValuePosition.height);


            // Display the Scaled Value Property
            EditorGUI.PropertyField(fieldPosition, scaledValueProperty, GUIContent.none);

            if (buttonPosition.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                GenericMenu menu = new GenericMenu();
                string address = "";
                
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
                
                if (foundParam)
                {                   
                    if (fxScaledParamC != null) address = fxScaledParamC.Address;
                    else if (fxScaledParamF != null)  address = fxScaledParamF.Address;
                    else if (fxScaledParamI != null)  address = fxScaledParamI.Address ;
                    else if (fxScaledParamv3 != null) address = fxScaledParamv3.Address;
                }

                FXGroupController[] allFXGroups = GameObject.FindObjectsOfType<FXGroupController>();
                // Reverse order for nice ordered labels in UI
                for (int i = allFXGroups.Length - 1; i >= 0; i--)
                {
                    FXGroupController group = allFXGroups[i];
                    bool existsInGroup = group.ExistsInFxAddress(address);
                    GUIContent menuItemContent = new GUIContent(group.GetLabelBasedOnSignalSource());

                    menu.AddItem(menuItemContent, existsInGroup, () =>
                    {
                        if (existsInGroup)
                        {
                            group.RemoveFxAddress(address);
                        }
                        else
                        {
                            group.AddFxAddress(address);
                        }
                    });
                }
                menu.ShowAsContext();
                Event.current.Use();
            }

            if (GUI.Button(buttonPosition, "G", EditorStyles.miniButton))
            {
                // Left-click action (if needed)
                Debug.Log("Button left-clicked");
            }



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
                    if (fxScaledParamC != null) fxScaledParamC.Value = newValue;
                    else if (fxScaledParamF != null) fxScaledParamF.Value = newValue;
                    else if (fxScaledParamI != null) fxScaledParamI.Value = newValue;
                    else if (fxScaledParamv3 != null) fxScaledParamv3.Value = newValue;
                }

                EditorGUI.PropertyField(valueAtZeroPosition, valueAtZeroProperty, valueAtZeroLabel);
                EditorGUI.PropertyField(valueAtOnePosition , valueAtOneProperty, valueAtOneLabel);
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
}