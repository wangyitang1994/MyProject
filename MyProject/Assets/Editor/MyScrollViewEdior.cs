using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MyScrollView),true)]
[CanEditMultipleObjects]
public class MyScrollViewEdior : UnityEditor.UI.ScrollRectEditor
{
    SerializedProperty _up;
    SerializedProperty _down;
    SerializedProperty _left;
    SerializedProperty _right;
    protected override void OnEnable()
    {
        base.OnEnable();
        _up = serializedObject.FindProperty("up");
        _down = serializedObject.FindProperty("down");
        _left = serializedObject.FindProperty("left");
        _right = serializedObject.FindProperty("right");
    }
    
    public override void OnInspectorGUI(){
        base.OnInspectorGUI();
        //serializedObject.Update();
        EditorGUILayout.PropertyField(_up);
        EditorGUILayout.PropertyField(_down);
        EditorGUILayout.PropertyField(_left);
        EditorGUILayout.PropertyField(_right);
        serializedObject.ApplyModifiedProperties();
        // if(GUI.changed){
        //     EditorUtility.SetDirty(target);
        // }
    }
}

