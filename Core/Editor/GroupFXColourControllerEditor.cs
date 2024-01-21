using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace FX
{
    [CustomEditor(typeof(GroupFXColourController))]
    public class GroupFXColourControllerEditor : Editor
    {
        private SerializedProperty fxAddressesProperty;
        private SerializedProperty colorOneProperty;

        private string[] fxAddressPopupValues = new string[] { };

        private GroupFXColourController controller;

        private void OnEnable()
        {
            // Assuming FXManager has an event or method to get the list of FX addresses
            FXManager.Instance.OnFXItemAdded += UpdateFXAddressPopupValues;

            controller = (GroupFXColourController)target;
            fxAddressesProperty = serializedObject.FindProperty("fxAddresses");
            colorOneProperty = serializedObject.FindProperty("colorOne");

            UpdateFXAddressPopupValues();
        }

        private void OnDisable()
        {
            FXManager.Instance.OnFXItemAdded -= UpdateFXAddressPopupValues;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(colorOneProperty);

            if (fxAddressPopupValues.Length == 0)
            {
                UpdateFXAddressPopupValues();
            }

            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("FX Addresses");
                for (int i = 0; i < fxAddressesProperty.arraySize; i++)
                {
                    SerializedProperty addressElement = fxAddressesProperty.GetArrayElementAtIndex(i);
                    int selectedAddressIndex = Mathf.Max(Array.IndexOf(fxAddressPopupValues, addressElement.stringValue), 0);
                    selectedAddressIndex = EditorGUILayout.Popup(selectedAddressIndex, fxAddressPopupValues);
                    addressElement.stringValue = fxAddressPopupValues[selectedAddressIndex];
                }

                GUILayout.Space(10);

                if (GUILayout.Button("Add Address"))
                {
                    fxAddressesProperty.arraySize++;
                    serializedObject.ApplyModifiedProperties();
                }
                if (fxAddressesProperty.arraySize > 0 && GUILayout.Button("Remove Last Address"))
                {
                    fxAddressesProperty.arraySize--;
                    serializedObject.ApplyModifiedProperties();
                }
            }

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

            }
            if (addressList.Count > 0)
            {
                fxAddressPopupValues = addressList.ToArray();
            }


        }
    }
}
