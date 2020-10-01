using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CreateAssetMenu(fileName = "ABConfig",menuName = "CreateABConfig",order = 0)]
public class AssetBunldePathConfig:ScriptableObject
{
    public List<string> m_AllPrefabPath = new List<string>();
    public List<NameAndPath> m_AllFileABPath = new List<NameAndPath>();
    [System.Serializable]
    public struct NameAndPath{
        [SerializeField]
        public string name;
        [SerializeField]
        public string path;
    }
}

[CustomEditor(typeof(AssetBunldePathConfig))]
public class AssetBunldConfigInspector:Editor{
    private ReorderableList prefabReorderableList;
    private ReorderableList fileReorderableList;

    private void OnEnable() {
        //预制路径
        SerializedProperty prefabProp = serializedObject.FindProperty("m_AllPrefabPath");
        prefabReorderableList = new ReorderableList(serializedObject,prefabProp,true,true,true,true);
        prefabReorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;
            SerializedProperty element = prefabProp.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element,GUIContent.none);
        };
        prefabReorderableList.drawHeaderCallback = (rect) =>{
            EditorGUI.LabelField(rect,"m_AllPrefabPath 预制路径");
        };
        //文件路径
        SerializedProperty fileProp = serializedObject.FindProperty("m_AllFileABPath");
        fileReorderableList = new ReorderableList(serializedObject,fileProp,true,true,true,true);
        fileReorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;
            SerializedProperty element = fileProp.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element,GUIContent.none);
        };
        fileReorderableList.drawHeaderCallback = (rect) =>{
            EditorGUI.LabelField(rect,"m_AllFileABPath 文件路径");
        };
    }

    public override void OnInspectorGUI(){
        EditorGUILayout.Space();
        serializedObject.Update();
        prefabReorderableList.DoLayoutList();
        GUILayout.Space(5);
        fileReorderableList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}
