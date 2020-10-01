using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using MyTool;

public class Window
{
    protected UIBinder binder;

    public GameObject GameObject{get;set;}
    public Transform Transform{get;set;}
    public string Name{get;set;}
    public List<Button> m_AllButton = new List<Button>();
    public List<Toggle> m_AllToggle = new List<Toggle>();

    public bool isHide{get;private set;}

    public virtual void OnAwake(object[] param) {
        AutoBindField();
    }

    private void AutoBindField(){
        binder = GameObject.GetComponent<UIBinder>();
        binder.RuntimeBind(this);
    }

    public virtual void OnOpen(object[] param){}

    public virtual void OnClose(){
        RemoveAllBtnEvent();
        RemoveAllTogEvent();
        m_AllButton.Clear();
        m_AllToggle.Clear();
    }

    public void Hide(){
        GameObject.SetActive(false);
        OnHide();
    }

    public virtual void OnHide(){
        LogTool.Log("Hide");
    }

    // public void Display(){
    //     GameObject.SetActive(true);
    //     OnDisplay();
    // }

    // public virtual void OnDisplay(){}

    public virtual void OnUpdate(){}

    public virtual bool OnMessage(UIMsgType type,params object[] param){
        return true;
    }

    public void RemoveAllBtnEvent(){
        foreach(Button btn in m_AllButton){
            btn.onClick.RemoveAllListeners();
        }
    }

    public void RemoveAllTogEvent(){
        foreach(Toggle tog in m_AllToggle){
            tog.onValueChanged.RemoveAllListeners();
        }
    }

    public void AddBtnEvent(Button btn,UnityAction callback){
        if(btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(callback);
        btn.onClick.AddListener(PlayBtnSound);
        if(!m_AllButton.Contains(btn)){
            m_AllButton.Add(btn);
        }
    }

    public void AddTogEvent(Toggle tog,UnityAction<bool> callback){
        if(tog == null) return;
        tog.onValueChanged.RemoveAllListeners();
        tog.onValueChanged.AddListener(callback);
        tog.onValueChanged.AddListener(PlayTogSound);
        if(!m_AllToggle.Contains(tog)){
            m_AllToggle.Add(tog);
        }
    }

    void PlayBtnSound(){

    }

    void PlayTogSound(bool is_on){

    }

    /// <summary>
    /// 设置Image(同步)
    /// </summary>
    /// <param name="path">路径带后缀</param>
    /// <param name="img"></param>
    /// <param name="setNative">设置原始尺寸</param>
    /// <returns></returns>
    public bool SetImageSprite(string path,Image img,bool setNative = false){
        if(img == null) return false;
        Sprite sp = ResourceManager.Instance.LoadResource<Sprite>(path);
        if(sp != null){
            img.sprite = sp;
            if(setNative){
                img.SetNativeSize();
            }
            return true;
        }
        return false;
    }

    public void SetImageSprite(string path,Image img,bool setNative = false,UnityAction callback = null){
        if(img == null) return;
        ResourceManager.Instance.AsyncLoadResource(path,OnSetImageFinish,img,setNative,callback);
    }

    void OnSetImageFinish(string path,UnityEngine.Object obj,object param1 = null,object param2 = null,object param3 = null){
        if(obj == null) return;
        Image img = (Image)param1;
        bool setNative = (bool)param2;

        if(param3 != null){
            UnityAction callback = (UnityAction)param3;
            callback();
        }
        img.sprite = obj as Sprite;
        if(setNative){
            img.SetNativeSize();
        }
    }

    public void CloseWindow(bool is_destory = false){
        UIManager.Instance.CloseWindow(this,is_destory);
    }
}
