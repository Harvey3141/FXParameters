using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static FX.GroupFXController;

namespace FX
{
    [CustomEditor(typeof(GroupFXController))]
    public class GroupFXControllerEditor : Editor
    {
        private ReorderableList reorderableListTrigger;
        private ReorderableList affectorListReorderable;
        private SerializedProperty affectorListProperty;

        private string[] paramPopupValues = new string[] { };
        private string[] triggerParamPopupValues = new string[] { };


        private SerializedProperty fxTriggerAddressesProperty;
        private SerializedProperty valueProperty;
        private SerializedProperty labelProperty;
        private SerializedProperty addressProperty;
        private SerializedProperty fxTypeProperty;
        private SerializedProperty signalSourcePropery;
        private SerializedProperty patternTypeProperty;
        private SerializedProperty frequencyProperty;


        private bool isIndicatorOn = false;
        private float indicatorDuration = 0.05f;
        private float timeSinceTriggered = 0f;

        GroupFXController controller;

        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        private void OnEnable()
        {
            FXManager.Instance.OnFXListChaned += OnFXListChaned;

            controller = (GroupFXController)target;
            controller.OnFXTriggered += HandleFXTriggered;
            valueProperty             = serializedObject.FindProperty("value");
            addressProperty           = serializedObject.FindProperty("address");
            labelProperty             = serializedObject.FindProperty("label");
            fxTypeProperty            = serializedObject.FindProperty("fxType");
            signalSourcePropery       = serializedObject.FindProperty("signalSource");
            patternTypeProperty       = serializedObject.FindProperty("patternType");
            frequencyProperty         = serializedObject.FindProperty("audioFrequency");

            fxTriggerAddressesProperty = serializedObject.FindProperty("fxTriggerAddresses");
            affectorListProperty       = serializedObject.FindProperty("fxParameterControllers");


            reorderableListTrigger = new ReorderableList(serializedObject, fxTriggerAddressesProperty, true, true, true, true);

            affectorListReorderable = new ReorderableList(serializedObject, affectorListProperty, true, true, true, true)
            {
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty item = affectorListProperty.GetArrayElementAtIndex(index);
                    SerializedProperty fxAddressProperty = item.FindPropertyRelative("key");

                    rect.y += 2;
                    rect.height = EditorGUIUtility.singleLineHeight;

                    if (paramPopupValues != null && paramPopupValues.Length > 0)
                    {
                        int selectedParamIndex = Array.IndexOf(paramPopupValues, fxAddressProperty.stringValue);
                        selectedParamIndex = Mathf.Max(selectedParamIndex, 0); // Ensure the index is at least 0
                        selectedParamIndex = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "FX Address", selectedParamIndex, paramPopupValues);

                        if (selectedParamIndex >= 0 && selectedParamIndex < paramPopupValues.Length)
                        {
                            fxAddressProperty.stringValue = paramPopupValues[selectedParamIndex];
                        }
                    }
                    else
                    {
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "FX Address", "No options available");
                    }

                    rect.y += EditorGUIUtility.singleLineHeight + 2;

                    EditorGUI.BeginChangeCheck(); 

                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), item.FindPropertyRelative("affectorType"), new GUIContent("Affector Function"));
                    rect.y += EditorGUIUtility.singleLineHeight + 2;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), item.FindPropertyRelative("invert"), new GUIContent("Invert"));
                    rect.y += EditorGUIUtility.singleLineHeight + 2;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), item.FindPropertyRelative("enabled"), new GUIContent("Enabled"));
                    if (EditorGUI.EndChangeCheck()) 
                    {
                                         
                    }
                },
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "FX Controllers");
                },
                elementHeightCallback = (int index) =>
                {
                   // return EditorGUIUtility.singleLineHeight * 3 + 6;
                    return EditorGUIUtility.singleLineHeight * 4 + 2 * 3; 

                }
            };

            affectorListReorderable.onRemoveCallback = OnParamRemoved;
            affectorListReorderable.onAddCallback = OnParamAdded;

            reorderableListTrigger.drawElementCallback = DrawListItems2;
            reorderableListTrigger.elementHeightCallback = (index) => EditorGUIUtility.singleLineHeight * 1.5f;
            reorderableListTrigger.drawHeaderCallback = DrawHeader2;
        }


        private void OnParamAdded(ReorderableList l)
        {
            var groupFXController = target as GroupFXController;
            groupFXController.AddFXParam("/");
            serializedObject.Update();
            int newIndex = groupFXController.fxParameterControllers.Count - 1;
            l.index = newIndex;
            serializedObject.ApplyModifiedProperties();

        }


        private void OnParamRemoved(ReorderableList l)
        {
            if (l.index >= 0 && l.index < l.serializedProperty.arraySize)
            {
                SerializedProperty itemProperty = l.serializedProperty.GetArrayElementAtIndex(l.index);

                string removedAddress = itemProperty.FindPropertyRelative("key").stringValue;

                GroupFXController groupFXController = (GroupFXController)l.serializedProperty.serializedObject.targetObject;
                groupFXController.RemoveFXParam("/" + removedAddress);

                l.serializedProperty.DeleteArrayElementAtIndex(l.index);
                l.serializedProperty.serializedObject.ApplyModifiedProperties();

            }
        }

        private void OnDisable()
        {
            FXManager.Instance.OnFXListChaned -= OnFXListChaned;
            controller.OnFXTriggered -= HandleFXTriggered;
        }

        private void OnFXListChaned() {

            UpdateParamPopupValues();
        }


        private void HandleFXTriggered()
        {
            isIndicatorOn = true;
            timeSinceTriggered = 0f;
        }


        private void DrawHeader2(Rect rect)
        {
            EditorGUI.LabelField(rect, "FX Triggers");
        }


        private void DrawListItems2(Rect rect, int index, bool isActive, bool isFocused)
        {

            SerializedProperty element = reorderableListTrigger.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 2;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            EditorGUI.EndDisabledGroup();


            if (Application.isPlaying)
            {
                int selectedParamIndex = Mathf.Max(Array.IndexOf(triggerParamPopupValues, element.stringValue), 0);
                selectedParamIndex = EditorGUI.Popup(new Rect(rect.width + rect.x - 90, rect.y, 90, EditorGUIUtility.singleLineHeight), selectedParamIndex, triggerParamPopupValues);

                if (selectedParamIndex >= 0 && selectedParamIndex < triggerParamPopupValues.Length)
                {
                    element.stringValue = triggerParamPopupValues[selectedParamIndex];
                }
                else
                {
                    // Log a warning or handle the situation appropriately
                    Debug.LogWarning($"selectedParamIndex ({selectedParamIndex}) out of bounds. Array length: {triggerParamPopupValues.Length}");
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (paramPopupValues.Length == 0) 
            {
                UpdateParamPopupValues();
            }

            if (controller.presetLoaded)
            {
                controller.presetLoaded = false;
            }

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(addressProperty, new GUIContent("Address"));
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(labelProperty, new GUIContent("Label"));
            GUILayout.Space(10);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(signalSourcePropery);
            GUILayout.Space(10);
            SignalSource currentSignalSource = (SignalSource)signalSourcePropery.enumValueIndex;
            switch (currentSignalSource) {
                case SignalSource.Default:
                    controller.SetPatternType(PatternType.None);
                    break;
                case SignalSource.Pattern:
                    EditorGUILayout.PropertyField(patternTypeProperty);
                    PatternType currentPatternType = (PatternType)patternTypeProperty.enumValueIndex;
                    if (currentPatternType != controller.patternType)
                    {
                        controller.SetPatternType(currentPatternType);
                        serializedObject.Update();
                    }
                    break;
                case SignalSource.Audio:
                    controller.SetPatternType(PatternType.None);
                    EditorGUILayout.PropertyField(frequencyProperty);
                    break;
            }
            EditorGUILayout.LabelField("Connections");

            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                affectorListReorderable.DoLayoutList();
                GUILayout.Space(10);
                float value = EditorGUILayout.Slider(new GUIContent("Value"), controller.value.Value, 0, 1);
                if (EditorGUI.EndChangeCheck())
                {
                    controller.value.Value = value;
                    controller.SetValue(value);
                }

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Remap Input"); 

                float min = controller.value.ValueAtZero;
                float max = controller.value.ValueAtOne;
                EditorGUILayout.MinMaxSlider(ref min, ref max, 0.0f, 1.0f);
                EditorGUILayout.EndHorizontal(); 

                if (EditorGUI.EndChangeCheck())
                {
                    controller.value.ValueAtZero = min;
                    controller.value.ValueAtOne = max;
                }
            }

            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                reorderableListTrigger.DoLayoutList();
                GUILayout.Space(10); 
                if (GUILayout.Button("Trigger"))
                {
                    controller.FXTrigger();
                }
                Rect lightRect = GUILayoutUtility.GetRect(20, 20);
                EditorGUI.DrawRect(lightRect, isIndicatorOn ? Color.white : Color.gray);
            }


            if (isIndicatorOn)
            {
                timeSinceTriggered += Time.deltaTime;
                if (timeSinceTriggered >= indicatorDuration)
                {
                    isIndicatorOn = false;
                    Repaint();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateParamPopupValues()
        {
            List<string> addressList = new List<string>();
            List<string> addressListTriggers = new List<string>();

            foreach (var kvp in FXManager.fxItemsByAddress_)
            {
                if (kvp.Value.type == FXManager.FXItemInfoType.Method)
                {
                    MethodInfo method = kvp.Value.item as MethodInfo;
                    ParameterInfo[] parameters = method.GetParameters();

                    if (parameters.Length == 0)
                    {
                        string address = kvp.Key;

                        // Remove the leading "/" character from each string.
                        if (address.StartsWith("/"))
                        {
                            address = address.Substring(1);
                        }

                        addressListTriggers.Add(address);
                    }
                }
            }
            if (addressListTriggers.Count > 0)
            {
                triggerParamPopupValues = addressListTriggers.ToArray();
            }

            foreach (var kvp in FXManager.fxItemsByAddress_)
            {
                if (kvp.Value.type == FXManager.FXItemInfoType.Parameter || kvp.Value.type == FXManager.FXItemInfoType.ScaledParameter)
                {
                    if (kvp.Value.item is FXParameter<float> ||
                        kvp.Value.item is FXParameter<bool> ||
                        kvp.Value.item is FXParameter<int>)
                    {
                        string address = kvp.Key;

                        // Remove the leading "/" character from each string.
                        if (address.StartsWith("/"))
                        {
                            address = address.Substring(1);
                        }

                        if (!address.ToUpper().StartsWith("GROUP/"))
                        {
                            addressList.Add(address);
                        }
                       
                    }
                }
                else if (kvp.Value.type == FXManager.FXItemInfoType.Method)
                {
                    MethodInfo method = kvp.Value.item as MethodInfo;
                    ParameterInfo[] parameters = method.GetParameters();

                    // Check if the method has one parameter and it is of type float, int, or bool.
                    if (parameters.Length == 1 &&
                        (parameters[0].ParameterType == typeof(float) ||
                            parameters[0].ParameterType == typeof(int) ||
                            parameters[0].ParameterType == typeof(bool)))
                    {
                        string address = kvp.Key;

                        // Remove the leading "/" character from each string.
                        if (address.StartsWith("/"))
                        {
                            address = address.Substring(1);
                        }

                        addressList.Add(address);
                    }
                }
            }
            if (addressList.Count > 0)
            {
                paramPopupValues = addressList.ToArray();
            }

        }
    }
}
