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
        private SerializedProperty fxAddressesProperty;
        private ReorderableList reorderableList;
        private string[]  paramPopupValues = new string[] { "Test 1", "Test 2", "Test 3" }; // Test array
        private SerializedProperty valueProperty;
        FXGroupController controller;


        private void OnEnable()
        {
            FXManager.Instance.OnFXItemAdded += UpdateParamPopupValues;

            controller = (FXGroupController)target;
            fxAddressesProperty = serializedObject.FindProperty("fxAddresses");
            valueProperty = serializedObject.FindProperty("value"); 


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
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginChangeCheck();
            float value = EditorGUILayout.Slider(new GUIContent("value"), controller.value.Value, 0, 1);
            if (EditorGUI.EndChangeCheck())
            {
                controller.value.Value = value;
                controller.SetValue(value);
            }

            //UpdateParamPopupValues(); 

            EditorGUILayout.LabelField("Connections");

            reorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateParamPopupValues()
        {
            List<string> addressList = new List<string>();

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

            if (addressList.Count > 0)
            {
                paramPopupValues = addressList.ToArray();
            }
            else
            {
                Debug.LogError("No addresses matching the criteria found.");
            }
        }


    }
}
