using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.UI;

[CustomEditor(typeof(CircleImage),true)]
[CanEditMultipleObjects]
public class CircleImageEditor : ImageEditor
{
    SerializedProperty _segments;
    SerializedProperty _originPos;
    SerializedProperty _redius;
    SerializedProperty _rotation;
    protected override void OnEnable()
    {
        base.OnEnable();
        _segments = serializedObject.FindProperty("segments");
        _originPos = serializedObject.FindProperty("originPos");
        _redius = serializedObject.FindProperty("redius");
        _rotation = serializedObject.FindProperty("rotation");
    }
    
    public override void OnInspectorGUI(){
        base.OnInspectorGUI();
        serializedObject.Update();
        // EditorGUILayout.Slider
        EditorGUILayout.PropertyField(_segments);
        EditorGUILayout.PropertyField(_originPos);
        EditorGUILayout.PropertyField(_redius);
        EditorGUILayout.PropertyField(_rotation);
        serializedObject.ApplyModifiedProperties();
        if(GUI.changed){
            EditorUtility.SetDirty(target);
        }
    }

    private const int UI_LAYER = 5;
    [MenuItem("GameObject/MyComponent/CustomImage", priority = 0)]
    private static void AddImage()
    {
        Transform canvasTrans = GetCanvasTrans();
        Transform image = AddCustomImage();
        if (Selection.activeGameObject!= null && Selection.activeGameObject.layer == UI_LAYER)
        {
            image.SetParent(Selection.activeGameObject.transform);
        }
        else
        {
            image.SetParent(canvasTrans);
        }
        image.localPosition = Vector3.zero;
    }

    private static Transform GetCanvasTrans()
    {
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            SetLayer(canvasObj);
            canvasObj.AddComponent<RectTransform>();
            canvasObj.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            return canvasObj.transform;
        }
        else
        {
            return canvas.transform;
        }
    }

    private static Transform AddCustomImage()
    {
        GameObject image = new GameObject("Image");
        SetLayer(image);
        image.AddComponent<RectTransform>();
        image.AddComponent<CircleImage>();
        return image.transform;
    }

    private static void SetLayer(GameObject ui)
    {
        ui.layer = UI_LAYER;
    }
}

