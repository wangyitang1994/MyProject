using UnityEditor;
using UnityEngine;

namespace UIEditor
{
    [CustomPropertyDrawer(typeof(AssetBunldePathConfig.NameAndPath))]
    public class AssetBunldConfigPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty name = property.FindPropertyRelative("name");
            SerializedProperty objProp = property.FindPropertyRelative("path");

            Rect nameRect = new Rect(position)
            {
                width = 80,
                height = EditorGUIUtility.singleLineHeight,
            };
            Rect objRect = new Rect(position)
            {
                x = position.x + nameRect.width + 10f,
                width = position.width - nameRect.width - 10f,
                height = EditorGUIUtility.singleLineHeight,
            };
            EditorGUI.PropertyField(nameRect, name, new GUIContent(string.Empty));
            EditorGUI.PropertyField(objRect, objProp, new GUIContent(string.Empty));
        }
    }
}