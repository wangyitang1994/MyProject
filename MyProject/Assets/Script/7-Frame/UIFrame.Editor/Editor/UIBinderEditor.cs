using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(UIBinder))]
public class UIBinderEditor:Editor{
    private UIBinder binder;
    private ReorderableList reorderableList;

    private void OnEnable(){
        SerializedProperty prop = serializedObject.FindProperty("binderList");
        reorderableList = new ReorderableList(serializedObject, prop, true, true, true, true);
        reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;
            SerializedProperty element = prop.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element,GUIContent.none);
        };
        reorderableList.drawHeaderCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "控件组");
        };
        reorderableList.onAddCallback = (ReorderableList list) =>
        {
            if (list.serializedProperty != null)
            {
                ReorderableList.defaultBehaviours.DoAddButton(list);
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.index);
                SerializedProperty bindTypeProp = element.FindPropertyRelative("bindType");
                SerializedProperty objProp = element.FindPropertyRelative("obj");
                bindTypeProp.enumValueIndex = 0;
                objProp.objectReferenceValue = null;
            }
            else
            {
                ReorderableList.defaultBehaviours.DoAddButton(list);
            }
        };
        reorderableList.onRemoveCallback = (ReorderableList list) =>
        {
            if (EditorUtility.DisplayDialog("提示", string.Format("是否确定删除下标为{0}的控件？", list.index), "确定", "取消"))
            {
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.index);
                SerializedProperty bindTypeProp = element.FindPropertyRelative("bindType");
                SerializedProperty objProp = element.FindPropertyRelative("obj");
                bindTypeProp.enumValueIndex = 0;
                objProp.objectReferenceValue = null;
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            }
        };
    }

    public override void OnInspectorGUI(){
        UIBinder binder = (UIBinder)target;
        // DrawDefaultInspector();
        EditorGUILayout.Space();
        serializedObject.Update();
        reorderableList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
        
        if(GUILayout.Button("查找对应组件")){
            binder.FindComponent();
        }
        if(GUILayout.Button("生成绑定代码到剪贴板")){
            binder.GenerateBindCode();
        }
    }
}