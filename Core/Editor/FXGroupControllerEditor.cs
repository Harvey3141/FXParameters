using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static FX.GroupFXController;

namespace FX
{
    [CustomEditor(typeof(GroupFXController))]
    public class GroupFXControllerEditor : Editor
    {
        private ReorderableList reorderableList;
        private ReorderableList reorderableListTrigger;

        private string[] paramPopupValues        = new string[] {};
        private string[] triggerParamPopupValues = new string[] { };


        private SerializedProperty fxAddressesProperty;
        private SerializedProperty fxTriggerAddressesProperty;
        private SerializedProperty valueProperty;
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
            FXManager.Instance.OnFXItemAdded += UpdateParamPopupValues;
            
            controller           = (GroupFXController)target;
            controller.OnFXTriggered += HandleFXTriggered;
            fxAddressesProperty  = serializedObject.FindProperty("fxAddresses");
            valueProperty        = serializedObject.FindProperty("value");
            addressProperty      = serializedObject.FindProperty("address");
            fxTypeProperty       = serializedObject.FindProperty("fxType");
            signalSourcePropery  = serializedObject.FindProperty("signalSource");
            patternTypeProperty  = serializedObject.FindProperty("patternType");
            frequencyProperty    = serializedObject.FindProperty("audioFrequency");

            fxTriggerAddressesProperty = serializedObject.FindProperty("fxTriggerAddresses");

            reorderableList        = new ReorderableList(serializedObject, fxAddressesProperty       , true, true, true, true);
            reorderableListTrigger = new ReorderableList(serializedObject, fxTriggerAddressesProperty, true, true, true, true);


            reorderableList.drawElementCallback   = DrawListItems;
            reorderableList.elementHeightCallback = (index) => EditorGUIUtility.singleLineHeight * 1.5f;
            reorderableList.drawHeaderCallback    = DrawHeader;

            reorderableListTrigger.drawElementCallback   = DrawListItems2;
            reorderableListTrigger.elementHeightCallback = (index) => EditorGUIUtility.singleLineHeight * 1.5f;
            reorderableListTrigger.drawHeaderCallback    = DrawHeader2;
        }

        private void OnDisable()
        {
            FXManager.Instance.OnFXItemAdded -= UpdateParamPopupValues;
            controller.OnFXTriggered -= HandleFXTriggered;
        }

        private void HandleFXTriggered()
        {
            isIndicatorOn = true;
            timeSinceTriggered = 0f;
        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Values");
        }

        private void DrawHeader2(Rect rect)
        {
            EditorGUI.LabelField(rect, "Triggers");
        }

        private void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {

            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 2;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            EditorGUI.EndDisabledGroup();


            if (Application.isPlaying)
            {
                int selectedParamIndex = Mathf.Max(Array.IndexOf(paramPopupValues, element.stringValue), 0);
                selectedParamIndex = EditorGUI.Popup(new Rect(rect.width + rect.x - 90, rect.y, 90, EditorGUIUtility.singleLineHeight), selectedParamIndex, paramPopupValues);
                element.stringValue = paramPopupValues[selectedParamIndex];
            }
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
                reorderableList.DoLayoutList();
                GUILayout.Space(10);
                float value = EditorGUILayout.Slider(new GUIContent("Value"), controller.value.Value, 0, 1);
                if (EditorGUI.EndChangeCheck())
                {
                    controller.value.Value = value;
                    controller.SetValue(value);
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
                if (kvp.Value.type == FXManager.FXItemInfoType.Parameter)
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

                        // Regex to check if the address starts with 'Group' followed by a number
                        if (!Regex.IsMatch(address, @"^Group\d"))
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
