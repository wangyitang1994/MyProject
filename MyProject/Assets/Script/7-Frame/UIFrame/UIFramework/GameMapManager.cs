using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameMapManager:Singleton<GameMapManager>{
    //开始加载回调
    public Action LoadSceneEnterCallback;
    //加载结束回调
    public Action LoadSceneFinishCallback;

    public static string CurrentMapName{get;set;}

    public static int loadingProgress = 0;

    private MonoBehaviour m_Mono;

    public void Init(MonoBehaviour mono){
        m_Mono = mono;
    }

    public void LoadScene(string name){
        loadingProgress = 0;
        m_Mono.StartCoroutine(LoadSceneAsync(name));
        UIManager.Instance.PopupWindow("LoadingWindow.prefab",true,name);
    }

    //协程显示进度
    IEnumerator LoadSceneAsync(string name){
        CleanCache();
        if(LoadSceneEnterCallback != null){
            LoadSceneEnterCallback();
        }
        //卸载的场景
        AsyncOperation unloadScene = SceneManager.LoadSceneAsync(ConstString.EMPTY_SCENE,LoadSceneMode.Single);
        while(unloadScene != null && !unloadScene.isDone){
            yield return new WaitForEndOfFrame();
        }
        
        int target = 0;
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(name);
        if(loadScene != null && !loadScene.isDone){
            loadScene.allowSceneActivation = false;
            while(loadScene.progress < 0.9f){
                target = (int)(loadScene.progress * 100);
                yield return new WaitForEndOfFrame();
                while (loadingProgress <= target)
                {
                    loadingProgress++;
                    yield return new WaitForEndOfFrame();
                }
            }
            target = 100;
            while (loadingProgress < target - 2)
            {
                loadingProgress++;
                yield return new WaitForEndOfFrame();
            }
            loadingProgress = 100;
            loadScene.allowSceneActivation = true;
            if(LoadSceneFinishCallback != null){
                LoadSceneFinishCallback();
            }
        }
        CurrentMapName = name;
    }

    private void CleanCache(){
        ObjectManager.Instance.CleanObjectPool();
        ResourceManager.Instance.ClearResource();
    }
}