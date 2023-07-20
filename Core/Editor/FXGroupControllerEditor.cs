using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static FX.FXGroupController;

namespace FX
{
    [CustomEditor(typeof(FXGroupController))]
    public class FXGroupControllerEditor : Editor
    {
        private ReorderableList reorderableList;
        private ReorderableList reorderableListTrigger;

        private string[] paramPopupValues = new string[] {};
        private string[] triggerParamPopupValues = new string[] { };


        private SerializedProperty fxAddressesProperty;
        private SerializedProperty fxTriggerAddressesProperty;
        private SerializedProperty valueProperty;
        private SerializedProperty addressProperty;
        private SerializedProperty fxTypeProperty;
        private SerializedProperty signalSourcePropery;
        private SerializedProperty patternTypeProperty;


        FXGroupController controller;


        private void OnEnable()
        {
            FXManager.Instance.OnFXItemAdded += UpdateParamPopupValues;

            controller           = (FXGroupController)target;
            fxAddressesProperty  = serializedObject.FindProperty("fxAddresses");
            valueProperty        = serializedObject.FindProperty("value");
            addressProperty      = serializedObject.FindProperty("address");
            fxTypeProperty       = serializedObject.FindProperty("fxType");
            signalSourcePropery  = serializedObject.FindProperty("signalSource");
            patternTypeProperty  = serializedObject.FindProperty("patternType");
            fxTriggerAddressesProperty = serializedObject.FindProperty("fxTriggerAddresses");

            reorderableList = new ReorderableList(serializedObject, fxAddressesProperty, true, true, true, true);
            reorderableListTrigger = new ReorderableList(serializedObject, fxTriggerAddressesProperty, true, true, true, true);


            reorderableList.drawElementCallback = DrawListItems;
            reorderableList.elementHeightCallback = (index) => EditorGUIUtility.singleLineHeight * 1.5f;
            reorderableList.drawHeaderCallback = DrawHeader;

            reorderableListTrigger.drawElementCallback = DrawListItems2;
            reorderableListTrigger.elementHeightCallback = (index) => EditorGUIUtility.singleLineHeight * 1.5f;
            reorderableListTrigger.drawHeaderCallback = DrawHeader2;
        }

        private void OnDisable()
        {
            FXManager.Instance.OnFXItemAdded -= UpdateParamPopupValues;
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
                element.stringValue = triggerParamPopupValues[selectedParamIndex];
            }
        }

        public override void OnInspectorGUI()
        {
            if (paramPopupValues.Length == 0 || controller.presetLoaded) {
                UpdateParamPopupValues();
                if (controller.presetLoaded) controller.presetLoaded = false;
            } 

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(addressProperty, new GUIContent("Address"));

            EditorGUILayout.PropertyField(signalSourcePropery);
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
                    break;
            }

            if (GUILayout.Button("Trigger"))
            {
                controller.FXTrigger();
            }
            float value = EditorGUILayout.Slider(new GUIContent("value"), controller.value.Value, 0, 1);
            if (EditorGUI.EndChangeCheck())
            {
                controller.value.Value = value;
                controller.SetValue(value);
            }

            EditorGUILayout.LabelField("Connections");

            reorderableList.DoLayoutList();
            reorderableListTrigger.DoLayoutList();

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

                        addressList.Add(address);
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
