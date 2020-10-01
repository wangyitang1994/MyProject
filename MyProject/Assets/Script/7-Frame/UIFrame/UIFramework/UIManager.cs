using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum UIMsgType{
    None = 0,
}

public class UIManager:Singleton<UIManager>
{
    private const string UIPREFABPATH = "Assets/GameData/Prefabs/UGUI/Window/";
    //UI节点
    RectTransform m_UIRoot;
    //窗口节点
    RectTransform m_WindowRoot;
    //UI摄像机
    Camera m_UICamera;
    //EventSystem
    EventSystem m_EventSystem;
    //宽高比
    public float m_CanvasRate{get;private set;}

    //创建的window字典
    private Dictionary<string,Window> m_WindowDic = new Dictionary<string, Window>();
    //创建的windowList
    private List<Window> m_WindowList = new List<Window>();
    //注册的Type字典
    private Dictionary<string,Type> m_RegisterDic = new Dictionary<string, Type>();
    private Dictionary<Type,string> m_WindowTypeDic = new Dictionary<Type, string>();

    public void Init(RectTransform uiRoot,RectTransform winRoot,Camera uiCamera,EventSystem eventSystem){
        m_UIRoot = uiRoot;
        m_WindowRoot = winRoot;
        m_UICamera = uiCamera;
        m_EventSystem = eventSystem;
        m_CanvasRate = Screen.height/(m_UICamera.orthographicSize*2);
        //TODO 分成打开时再注册 和 初始化时注册
        ResgisterPanel();
    }

    void ResgisterPanel(){
       
    }

    /// <summary>
    /// 注册窗口类型
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    public void Resgister(Type type,string name){
        if(typeof(Window).IsAssignableFrom(type)){
            m_RegisterDic[name] = type;
            m_WindowTypeDic[type] = name;
        }
        else
        {
            Debug.LogErrorFormat("UIManager.Resgister() 参数type:{0} 不是Window类型",type.Name);
        }
    }

    public T FindWindowByName<T>(string name) where T:Window{
        Window window;
        if(m_WindowDic.TryGetValue(name,out window) && window != null){
            return (T)window;
        }
        return null;
    }


    public Window PopupWindow(Type type,bool is_top = true,params object[] param){
        string name = null;
        if(m_WindowTypeDic.TryGetValue(type,out name) && name != null){
            return PopupWindow(name,is_top,param);
        }
        return null;
    }

    /// <summary>
    /// 打开一个窗口(没有则创建)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="is_top"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public Window PopupWindow(string name,bool is_top = true,params object[] param){
        Window window = FindWindowByName<Window>(name);
        if(window == null){
            Type type = null;
            if(m_RegisterDic.TryGetValue(name, out type)){
                //创建一个类
                window = Activator.CreateInstance(type) as Window;
                object obj = Activator.CreateInstance(type);
            }
            else
            {
                Debug.LogError("PopupWindow() 未注册的窗口类型 name:"+name);
                return null;
            }
            GameObject go = ObjectManager.Instance.InstantiateObject(UIPREFABPATH + name,false,false);
            if(go == null){
                Debug.LogError("PopupWindow() 创建窗口失败 name:"+name);
                return null;
            }
            if(!m_WindowDic.ContainsKey(name)){
                m_WindowDic.Add(name,window);
                m_WindowList.Add(window);
            }
            window.GameObject = go;
            window.Transform = go.transform;
            window.Name = name;
            window.OnAwake(param);
            go.transform.SetParent(m_WindowRoot,false);
            window.OnOpen(param);
        }
        else{
            OpenWindow(window,is_top,param);
        }
        return window;
    }

    /// <summary>
    /// 打开一个窗口
    /// </summary>
    /// <param name="name">窗口名</param>
    /// <param name="is_top">是否设置到最顶层</param>
    /// <param name="param">参数</param>
    public void OpenWindow(string name,bool is_top = true,params object[] param){
        Window window = FindWindowByName<Window>(name);
        OpenWindow(window,is_top,param);
    }

    /// <summary>
    /// 打开一个窗口
    /// </summary>
    /// <param name="window">窗口类</param>
    /// <param name="is_top">是否设置到最顶层</param>
    /// <param name="param">参数</param>
    public void OpenWindow(Window window,bool is_top = true,params object[] param){
        if(window == null){return;}
        if(window.GameObject != null && !window.GameObject.activeSelf){
            window.GameObject.SetActive(true);
        }
        if(is_top){
            window.Transform.SetAsLastSibling();
        }
        window.OnOpen(param);
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    /// <param name="name">名字</param>
    /// <param name="is_destory">是否销毁</param>
    public void CloseWindow(string name,bool is_destory = false){
        Window window = FindWindowByName<Window>(name);
        CloseWindow(window,is_destory);
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    /// <param name="window">窗口类</param>
    /// <param name="is_destory">是否销毁</param>
    public void CloseWindow(Window window,bool is_destory = false){
        if(window == null){return;}
        window.OnClose();
        if(m_WindowDic.ContainsKey(window.Name)){
            m_WindowDic.Remove(window.Name);
            m_WindowList.Remove(window);
        }
        if(is_destory){
            ObjectManager.Instance.ReleaseObject(window.GameObject,0,true);
        }
        else{
            ObjectManager.Instance.ReleaseObject(window.GameObject);
        }
        window.GameObject = null;
        window = null;
    }

    /// <summary>
    /// 关闭所有窗口
    /// </summary>
    public void CloseAllWindow(){
        for(int i = m_WindowList.Count - 1;i >= 0;i--){
            CloseWindow(m_WindowList[i]);
        }
    }

    //关闭所有窗口,打开一个新窗口
    public void SwitchStateByName(string name,bool is_top = true,params object[] param){
        CloseAllWindow();
        PopupWindow(name,is_top,param);
    }

    public void HideWindow(string name){
        Window window = FindWindowByName<Window>(name);
        HideWindow(window);
    }

    public void HideWindow(Window window){
        if(window == null){return;}
        window.Hide();
    }

    public bool SendMessage(string name,UIMsgType type,params object[] param){
        Window window = FindWindowByName<Window>(name);
        if(window == null) return false;
        return window.OnMessage(type,param);
    }

    public void OnUpdate(){
        // LogTool.Log("//OnUpdate?",m_WindowList);
        for(int i = 0;i < m_WindowList.Count;i++){
            if(m_WindowList[i] != null){
                m_WindowList[i].OnUpdate();
            }
        }
    }

    /// <summary>
    /// 设置默认选择对象
    /// </summary>
    /// <param name="obj"></param>
    public void SetNormalSelectObj(GameObject obj){
        if(m_EventSystem == null){
            m_EventSystem = EventSystem.current;
        }
        m_EventSystem.firstSelectedGameObject = obj;
    }

    public void SetAllWindowState(bool state){
        if(m_UIRoot != null){
            m_UIRoot.gameObject.SetActive(state);
        }
    }


    //  public void DisplayWindow(string name){
    //     Window window = FindWindowByName<Window>(name);
    //     DisplayWindow(window);
    // }

    // public void DisplayWindow(Window window){
    //     if(window == null){return;}
    //     window.Display();
    // }
}
