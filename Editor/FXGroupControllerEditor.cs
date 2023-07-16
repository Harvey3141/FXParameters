using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace FX
{
    [CustomEditor(typeof(FXGroupController))]
    public class FXGroupControllerEditor : Editor
    {
        private ReorderableList reorderableList;
        private string[] paramPopupValues = new string[] {};
        
        private SerializedProperty fxAddressesProperty;
        private SerializedProperty valueProperty;
        private SerializedProperty addressProperty;
        private SerializedProperty fxTypeProperty;

        FXGroupController controller;


        private void OnEnable()
        {
            FXManager.Instance.OnFXItemAdded += UpdateParamPopupValues;

            controller = (FXGroupController)target;
            fxAddressesProperty = serializedObject.FindProperty("fxAddresses");
            valueProperty = serializedObject.FindProperty("value");
            addressProperty = serializedObject.FindProperty("address");
            fxTypeProperty = serializedObject.FindProperty("fxType");


            reorderableList = new ReorderableList(serializedObject, fxAddressesProperty, true, true, true, true);

            reorderableList.drawElementCallback = DrawListItems;
            reorderableList.elementHeightCallback = (index) => EditorGUIUtility.singleLineHeight * 1.5f;
            reorderableList.drawHeaderCallback = DrawHeader;
        }

        private void OnDisable()
        {
            FXManager.Instance.OnFXItemAdded -= UpdateParamPopupValues;
        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "FX Addresses");
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

        public override void OnInspectorGUI()
        {
            if (paramPopupValues.Length == 0 || controller.presetLoaded) {
                UpdateParamPopupValues();
                if (controller.presetLoaded) controller.presetLoaded = false;
            } 

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(addressProperty, new GUIContent("Address"));
            EditorGUILayout.PropertyField(fxTypeProperty);

            FXManager.FXItemInfoType currentFXType = (FXManager.FXItemInfoType)fxTypeProperty.enumValueIndex;
            if (currentFXType != controller.fxType)
            {
                controller.fxType = currentFXType;
                SerializedProperty fxAddressesArrayProperty = serializedObject.FindProperty("fxAddresses");
                fxAddressesArrayProperty.ClearArray();
                serializedObject.ApplyModifiedProperties();
                UpdateParamPopupValues();
                return;
            }

            switch (currentFXType)
            {
                case FXManager.FXItemInfoType.Method:
                    if (GUILayout.Button("Trigger"))
                    {
                        controller.FXTrigger();
                    }
                    break;
                case FXManager.FXItemInfoType.Parameter:
                    float value = EditorGUILayout.Slider(new GUIContent("value"), controller.value.Value, 0, 1);
                    if (EditorGUI.EndChangeCheck())
                    {
                        controller.value.Value = value;
                        controller.SetValue(value);
                    }
                    break;
            }


            EditorGUILayout.LabelField("Connections");

            reorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateParamPopupValues()
        {
            List<string> addressList = new List<string>();

            FXManager.FXItemInfoType currentFXType = (FXManager.FXItemInfoType)fxTypeProperty.enumValueIndex;

            switch (currentFXType)
            {
                case FXManager.FXItemInfoType.Method:
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

                                addressList.Add(address);
                            }
                        }
                    }
                    break;
                case FXManager.FXItemInfoType.Parameter:
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

                            // Check if the method has at least one parameter and if the first parameter is of type float, int, or bool.
                            if (parameters.Length > 0 &&
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
                    break;
                default:

                    break;
            }



            if (addressList.Count > 0)
            {
                paramPopupValues = addressList.ToArray();
            }
            else
            {
                //Debug.LogWarning("No addresses matching the criteria found.");
            }
        }


    }
}
