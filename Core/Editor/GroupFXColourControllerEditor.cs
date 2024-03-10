using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace FX
{
    [CustomEditor(typeof(GroupFXColourController))]
    public class GroupFXColourControllerEditor : Editor
    {
        private SerializedProperty fxAddressesProperty;
        private SerializedProperty colorOneProperty;
        private ReorderableList reorderableList;
        private string[] fxAddressPopupValues = new string[] { };

        private GroupFXColourController controller; // Declare the controller field


        private void OnEnable()
        {
            controller = (GroupFXColourController)target;
            fxAddressesProperty = serializedObject.FindProperty("fxAddresses");
            colorOneProperty = serializedObject.FindProperty("color");

            UpdateFXAddressPopupValues();
            FXManager.Instance.OnFXListChaned += OnFXListChaned;

            reorderableList = new ReorderableList(serializedObject, fxAddressesProperty, true, true, true, true)
            {
                drawElementCallback = DrawListItems,
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "FX Addresses")
            };
        }

        private void OnDisable()
        {
            FXManager.Instance.OnFXListChaned -= OnFXListChaned;
        }

        private void OnFXListChaned()
        {
            UpdateFXAddressPopupValues();
        }

        private void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            int selectedAddressIndex = Mathf.Max(Array.IndexOf(fxAddressPopupValues, element.stringValue), 0);
            selectedAddressIndex = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), selectedAddressIndex, fxAddressPopupValues);
            element.stringValue = fxAddressPopupValues[selectedAddressIndex];
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(colorOneProperty);
            if (fxAddressPopupValues.Length == 0)
            {
                UpdateFXAddressPopupValues();
            }

            reorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateFXAddressPopupValues()
        {
            List<string> addressList = new List<string>();
            List<string> addressListTriggers = new List<string>();

            foreach (var kvp in FXManager.fxItemsByAddress_)
            {
                if (kvp.Value.type == FXManager.FXItemInfoType.Parameter)
                {
                    if (kvp.Value.item is FXParameter<Color>)
                    {
                        string address = kvp.Key;

                        if (address.StartsWith("/"))
                        {
                            address = address.Substring(1);
                        }

                        if (!Regex.IsMatch(address, @"^Group\d"))
                        {
                            addressList.Add(address);
                        }

                    }
                }

            }
            if (addressList.Count > 0)
            {
                fxAddressPopupValues = addressList.ToArray();
            }


        }
    }
}



