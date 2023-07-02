using System;
using System.Linq;
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

    private void OnEnable()
        {
            fxAddressesProperty = serializedObject.FindProperty("fxAddresses");

            reorderableList = new ReorderableList(serializedObject, fxAddressesProperty, true, true, true, true);

            reorderableList.drawElementCallback = DrawListItems;
            reorderableList.elementHeightCallback = (index) => EditorGUIUtility.singleLineHeight * 1.5f;
            reorderableList.drawHeaderCallback = DrawHeader;
        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "FX Addresses");
        }

        private void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (Application.isPlaying)
            {
                SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);

                rect.y += 2;

                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                EditorGUI.EndDisabledGroup();

                int selectedParamIndex = Mathf.Max(Array.IndexOf(paramPopupValues, element.stringValue), 0);
                selectedParamIndex = EditorGUI.Popup(new Rect(rect.width + rect.x - 90, rect.y, 90, EditorGUIUtility.singleLineHeight), selectedParamIndex, paramPopupValues);
                element.stringValue = paramPopupValues[selectedParamIndex];
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UpdateParamPopupValues(); // Call this here

            EditorGUILayout.LabelField("Connections");

            reorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateParamPopupValues()
        {
            paramPopupValues = FXManager.Instance.GetAddresses();

        }
    }
}
