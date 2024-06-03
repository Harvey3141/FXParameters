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
    
        public FXParameterDrawer()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }
    
        ~FXParameterDrawer()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        }
    
        private void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingEditMode
                || state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredEditMode)
            {
                fxParameterCache.Clear();
            }
        }
    
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
    
            IFXParameter fxParam = GetFXParameterFromProperty(property);

            Type parameterType = fxParam.GetType();


            if (fxParam is FXParameter<float> floatParam)
            {
                float newValue = EditorGUI.FloatField(position, label, floatParam.Value);
                if (newValue != floatParam.Value)
                    floatParam.Value = newValue;
            }
            else if (fxParam is FXParameter<bool> boolParam)
            {
                if (fxParam.Address.Contains("fxEnabled"))
                {
                    EditorGUI.PrefixLabel(position, label);

                    Rect buttonRect = new Rect(position.x, position.y, position.width, position.height);

                    GUI.backgroundColor = boolParam.Value ? Color.green : Color.red;

                    string buttonText = boolParam.Value ? "FX Enabled" : "FX Disabled";

                    if (GUI.Button(buttonRect, buttonText))
                    {
                        boolParam.Value = !boolParam.Value;
                    }
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    bool newValue = EditorGUI.Toggle(position, label, boolParam.Value);
                    if (newValue != boolParam.Value) boolParam.Value = newValue;
                }
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
                Rect colorFieldRect = new Rect(position.x, position.y, position.width - 60, position.height);
                Rect buttonRect = new Rect(position.x + position.width - 55, position.y, 25, position.height);
                Rect toggleRect = new Rect(position.x + position.width - 30, position.y, 25, position.height);

                Color newValue = EditorGUI.ColorField(colorFieldRect, label, colorParam.Value);
                if (newValue != colorParam.Value)
                    colorParam.Value = newValue;

                if (GUI.Button(buttonRect, "P", EditorStyles.miniButton))
                {
                    GenericMenu menu = new GenericMenu();
                    FXColourPaletteManager colourPaletteManager = FXColourPaletteManager.Instance;
                    if (colourPaletteManager != null && colourPaletteManager.activePalette != null)
                    {
                        for (int i = 0; i < colourPaletteManager.activePalette.colours.Count; i++)
                        {
                            Color col = colourPaletteManager.activePalette.colours[i];
                            int capturedIndex = i;
                            Texture2D colorTexture = CreateColorTexture(col);
                            GUIContent content = new GUIContent($"Color {i}", colorTexture);
                            bool selected = colorParam.GlobalColourPaletteIndex == capturedIndex;

                            menu.AddItem(content, selected, () =>
                            {
                                colorParam.GlobalColourPaletteIndex = capturedIndex;
                            });
                        }
                    }
                    menu.ShowAsContext();
                }

                colorParam.UseGlobalColourPalette = GUI.Toggle(toggleRect, colorParam.UseGlobalColourPalette, "");
            }


            else
            {
                // TODO - this should also be cached to prevent runtime reflection
                Type valueType = parameterType.IsGenericType ? parameterType.GetGenericArguments()[0] : null;

                if (valueType != null && valueType.IsEnum)
                {
                    object enumValue = fxParam.ObjectValue;
                    EditorGUI.BeginChangeCheck();
                    Enum selectedEnum = EditorGUI.EnumPopup(position, label, (Enum)enumValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        fxParam.ObjectValue = selectedEnum;
                    }
                }
                else
                {
                    EditorGUI.LabelField(position, label, new GUIContent("Unsupported FXParameter type"));

                }
            }
            EditorGUI.EndProperty();  
        }


        private Texture2D CreateColorTexture(Color color)
        {
            Texture2D texture = new Texture2D(16, 16);
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.green;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
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
        //  using a Dictionary<Type, object> to store the different FXScaledParameter<T> instances.
        bool foundParam = false;
        private FXScaledParameter<         float> fxScaledParamF;
        private FXScaledParameter<Color>   fxScaledParamC;
        private FXScaledParameter<int>     fxScaledParamI;
        private FXScaledParameter<Vector3> fxScaledParamv3;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.HelpBox(position, "", MessageType.None);

            SerializedProperty scaledValueProperty = property.FindPropertyRelative("scaledValue_");
            SerializedProperty valueAtZeroProperty = property.FindPropertyRelative("valueAtZero_");
            SerializedProperty valueAtOneProperty  = property.FindPropertyRelative("valueAtOne_");
            SerializedProperty valueProperty       = property.FindPropertyRelative("value_");

            // Add a small padding to the positions to fit inside the box
            float padding = 4f;  
            Rect paddedPosition = new Rect(position.x + padding, position.y + padding, position.width - 2 * padding, position.height - 2 * padding);

            Rect labelPosition       = new Rect(paddedPosition.x, paddedPosition.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            Rect scaledValuePosition = new Rect(paddedPosition.x + EditorGUIUtility.labelWidth, paddedPosition.y, paddedPosition.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

            property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, "   " + property.displayName, true, EditorStyles.boldLabel);

            float buttonWidth   = 20f;
            float fieldWidth = scaledValuePosition.width - 2 * buttonWidth - 2 * padding;
            Rect fieldPosition  = new Rect(scaledValuePosition.x, scaledValuePosition.y, fieldWidth, scaledValuePosition.height);
            Rect buttonPosition = new Rect(fieldPosition.xMax + padding, scaledValuePosition.y, buttonWidth, scaledValuePosition.height);

            Rect buttonPositionAffector = new Rect(buttonPosition.xMax + padding, scaledValuePosition.y, buttonWidth, scaledValuePosition.height);

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

                GroupFXController[] allFXGroups = GameObject.FindObjectsOfType<GroupFXController>();
                // Reverse order for nice ordered labels in UI
                for (int i = allFXGroups.Length - 1; i >= 0; i--)
                {
                    GroupFXController group = allFXGroups[i];
                    bool existsInGroup = group.ExistsInFxAddress(address);
                    GUIContent menuItemContent = new GUIContent(group.GetLabelBasedOnSignalSource());

                    menu.AddItem(menuItemContent, existsInGroup, () =>
                    {
                        if (existsInGroup)
                        {
                            group.RemoveFXParam(address);
                        }
                        else
                        {
                            group.AddFXParam(address);
                        }
                    });
                }
                menu.ShowAsContext();
                Event.current.Use();
            }

            if (GUI.Button(buttonPosition, "G", EditorStyles.miniButton))
            {
                Debug.Log("Button left-clicked");
            }

            if (GUI.Button(buttonPositionAffector, "E", EditorStyles.miniButton))
            {
                GenericMenu menu = new GenericMenu();

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

                AffectorFunction currentAffectorFunction = GetAffectorFunction();

                foreach (AffectorFunction func in Enum.GetValues(typeof(AffectorFunction)))
                {
                    bool isSelected = currentAffectorFunction == func;
                    menu.AddItem(new GUIContent(func.ToString()), isSelected, () =>
                    {
                        SetAffectorFunction(func);
                    });
                }

                menu.ShowAsContext();
            }

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

            return (lineCount * EditorGUIUtility.singleLineHeight) + ((lineCount + 1) * EditorGUIUtility.standardVerticalSpacing) + 2f * 4f; // additional 2f*padding for the top and bottom padding of the box
        }

        // Helper method to get the current AffectorFunction from the correctly typed FXScaledParameter instance
        private AffectorFunction GetAffectorFunction()
        {
            if (fxScaledParamC != null) return fxScaledParamC.AffectorFunction;
            if (fxScaledParamF != null) return fxScaledParamF.AffectorFunction;
            if (fxScaledParamI != null) return fxScaledParamI.AffectorFunction;
            if (fxScaledParamv3 != null) return fxScaledParamv3.AffectorFunction;
            return default(AffectorFunction); // Default or some error handling
        }

        // Helper method to set the AffectorFunction on the correctly typed FXScaledParameter instance
        private void SetAffectorFunction(AffectorFunction func)
        {
            if (fxScaledParamC != null) fxScaledParamC.AffectorFunction = func;
            else if (fxScaledParamF != null) fxScaledParamF.AffectorFunction = func;
            else if (fxScaledParamI != null) fxScaledParamI.AffectorFunction = func;
            else if (fxScaledParamv3 != null) fxScaledParamv3.AffectorFunction = func;

            // Ensure changes are saved, might require marking the object as dirty etc., depending on your setup
            // EditorUtility.SetDirty(targetObject); if working within the editor
        }

    }
}