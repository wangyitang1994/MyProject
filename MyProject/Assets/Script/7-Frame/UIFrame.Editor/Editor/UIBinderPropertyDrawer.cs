using UnityEditor;
using UnityEngine;

namespace UIEditor
{
    [CustomPropertyDrawer(typeof(UIBinder.UIBindField))]
    public class UIBinderPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty bindTypeProp = property.FindPropertyRelative("bindType");
            SerializedProperty objProp = property.FindPropertyRelative("obj");

            Rect bindTypeRect = new Rect(position)
            {
                width = 130f,
                height = EditorGUIUtility.singleLineHeight,
            };
            Rect objRect = new Rect(position)
            {
                x = position.x + bindTypeRect.width + 10f,
                width = position.width - bindTypeRect.width - 10f,
                height = EditorGUIUtility.singleLineHeight,
            };

            EditorGUI.PropertyField(bindTypeRect, bindTypeProp, new GUIContent(string.Empty));
            EditorGUI.PropertyField(objRect, objProp, new GUIContent(string.Empty));
        }
    }
}