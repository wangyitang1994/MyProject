using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "MyFrameConfig",menuName = "CreateMyFrameConfig",order = 0)]
public class MyFrameConfig:ScriptableObject
{
    //打包时生成AB包配置表的二进制路径
    public string m_BinaryPath;
    //xml文件夹路径
    public string m_XmlDataPath;
    //二进制文件夹路径
    public string m_BinaryDataPath;
    //脚本文件夹路径
    public string m_ScriptDataPath;
}

[CustomEditor(typeof(MyFrameConfig))]
public class MyFrameConfigInspector:Editor{
    public SerializedProperty m_BinaryPath;
    public SerializedProperty m_XmlDataPath;
    public SerializedProperty m_BinaryDataPath;
    public SerializedProperty m_ScriptDataPath;

    private void OnEnable() {
        m_BinaryPath = serializedObject.FindProperty("m_BinaryPath");
        m_XmlDataPath = serializedObject.FindProperty("m_XmlDataPath");
        m_BinaryDataPath = serializedObject.FindProperty("m_BinaryDataPath");
        m_ScriptDataPath = serializedObject.FindProperty("m_ScriptDataPath");
    }

    public override void OnInspectorGUI(){
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_BinaryPath,new GUIContent("AB包二进制路径"));
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(m_XmlDataPath,new GUIContent("xml文件夹路径"));
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(m_BinaryDataPath,new GUIContent("二进制文件夹路径"));
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(m_ScriptDataPath,new GUIContent("脚本文件夹路径"));
        serializedObject.ApplyModifiedProperties();
    }
}

public class RealConfig
{
    private const string MyFramePath = "Assets/Script/7-Frame/UIFrame.Editor/Editor/MyFrameConfig.asset";
    public static MyFrameConfig GetRealFram()
    {
        MyFrameConfig realConfig = AssetDatabase.LoadAssetAtPath<MyFrameConfig>(MyFramePath);
        return realConfig;
    }
}