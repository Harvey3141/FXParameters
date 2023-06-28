using System;
using UnityEditor;
using UnityEngine;

namespace FX
{
    [CustomEditor(typeof(FXGroupController))]
    public class FXGroupControllerEditor : Editor
    {
        private SerializedProperty fxAddressesProperty;
        private string[] paramPopupValues = new string[] { "Param1", "Param2", "Param3" };

        private void OnEnable()
        {
            fxAddressesProperty = serializedObject.FindProperty("fxAddresses");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Connections");

            DrawFXAddressesList();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFXAddressesList()
        {
            EditorGUILayout.LabelField("FX Addresses");

            int count = fxAddressesProperty.arraySize;

            // Draw the list elements
            for (int i = 0; i < count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                SerializedProperty addressProperty = fxAddressesProperty.GetArrayElementAtIndex(i);

                // Draw the address field as read-only
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(addressProperty, GUIContent.none);
                EditorGUI.EndDisabledGroup();

                // Find the index of the current address in the paramPopupValues array
                int selectedParamIndex = Mathf.Max(Array.IndexOf(paramPopupValues, addressProperty.stringValue), 0);

                // Draw the param popup
                selectedParamIndex = EditorGUILayout.Popup(selectedParamIndex, paramPopupValues, GUILayout.Width(100f));

                // Update the address property with the selected param value
                addressProperty.stringValue = paramPopupValues[selectedParamIndex];

                // Draw the remove button
                if (GUILayout.Button("-", GUILayout.Width(20f)))
                {
                    fxAddressesProperty.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            // Draw the add button
            if (GUILayout.Button("Add FX Address"))
            {
                fxAddressesProperty.InsertArrayElementAtIndex(count);
            }
        }
    }
}
