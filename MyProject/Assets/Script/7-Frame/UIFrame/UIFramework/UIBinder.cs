using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class UIBinder:MonoBehaviour{
    public enum UIBinderType{
        None,
        GameObject,
        RectTransform,
        Button,
        Slider,
        Image,
        Text,
    }

    [Serializable]
    public class UIBindField{
        [SerializeField]
        public UIBinderType bindType;
        public Object obj;
    }

    public UIBindField[] binderList = null;
    public string[] stringList = null;
    public void FindComponent(){
        foreach (UIBindField field in binderList)
        {
            if(field == null){
                Debug.LogErrorFormat("UIBinder:FindComponent {p0} 预制体绑定有空",name);
                continue;
            }
            Type cur_type = field.obj.GetType();
            Type target_type = GetComponentType(field);
            if(cur_type == target_type){
                continue;
            }
            else{
                if(cur_type != typeof(GameObject)){
                    Component com = field.obj as Component;
                    field.obj = com.gameObject;
                }
            }
            Object temp = MatchUIBinderType(field);
            if(temp == null){
                Debug.LogErrorFormat("UIBinder:FindComponent {p0} 未匹配到对应组件",field.obj.name);
                continue;
            }
            field.obj = temp;
        }
    }

    private Type GetComponentType(UIBindField field){
        switch (field.bindType)
        {
            case UIBinderType.GameObject:
                return typeof(GameObject);
            case UIBinderType.RectTransform:
                return typeof(RectTransform);
            case UIBinderType.Button:
                return typeof(Button);
            case UIBinderType.Slider:
                return typeof(Slider);
            case UIBinderType.Text:
                return typeof(Text);
            case UIBinderType.Image:
                return typeof(Image);             
        }
        return null;
    }

    private Object MatchUIBinderType(UIBindField field){
        GameObject go = field.obj as GameObject;
        switch (field.bindType)
        {
            case UIBinderType.GameObject:
                return go;
            case UIBinderType.RectTransform:
                return go.GetComponent<RectTransform>();
            case UIBinderType.Button:
                return go.GetComponent<Button>();
            case UIBinderType.Slider:
                return go.GetComponent<Slider>();  
            case UIBinderType.Text:
                return go.GetComponent<Text>();
            case UIBinderType.Image:
                return go.GetComponent<Image>();                                  
        }
        return null;
    }

    public string GenerateBindCode(){
        StringBuilder sb = new StringBuilder(1024);
        sb.Append("#region 自动生成控件绑定代码");
        foreach (UIBindField field in binderList)
        {
            sb.AppendFormat("\r\n    {0} {1} {2};", "public", field.bindType, field.obj.name);
        }
        sb.Append("\r#endregion");
        GUIUtility.systemCopyBuffer = sb.ToString();
        return sb.ToString();
    }

    public void RuntimeBind(Window window){
        RectTransform rtf = GetComponent<RectTransform>();
        FieldInfo[] fieldInfos = window.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        for (int fIdx = 0; fIdx < binderList.Length; fIdx++)
        {
            UIBindField uiField = binderList[fIdx];
            string binderName =  uiField.obj.name;
            if (uiField.obj == null)
            {
                Debug.LogWarningFormat("Warning: {0} UIBinder Not Find Field {1}!", this.name, binderName);
                continue;
            }
            for (int i = 0;i < fieldInfos.Length; i++)
            {
                FieldInfo fieldInfo = fieldInfos[i];
                string reflectionFiledName = fieldInfo.Name;
                if (binderName == reflectionFiledName)
                {
                    fieldInfo.SetValue(window, uiField.obj);
                    break;
                }
            }
        }
    }
}