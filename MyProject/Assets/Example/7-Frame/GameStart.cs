using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MyTool;

public class GameStart : MonoSingleton<GameStart>
{
    public AudioSource audioSource;
    private AudioClip clip;
    private GameObject obj;
    private Dictionary<Type,string> m_WindowTypeDic;

    protected override void Awake(){
        base.Awake();
        bool is_suc = AssetBundleManager.Instance.LoadAssetBundleConfig();
        GameObject.DontDestroyOnLoad(gameObject);
        Transform recycleTrs = transform.Find("RecyclePool");
        Transform sceneTrs = transform.Find("SceneObj");
        ObjectManager.Instance.InitRecyclePool(recycleTrs,sceneTrs);

        // AssetBundle ab = AssetBundle.LoadFromFile(path);
        // LogTool.Log(ab);
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadConfig();
        //异步加载
        ResourceManager.Instance.InitAsync(this);
        //UI节点
        UIManager.Instance.Init(transform.Find("UIRoot") as RectTransform,
            transform.Find("UIRoot/WinRoot") as RectTransform,
            transform.Find("UIRoot/UICamera").GetComponent<Camera>(),
            transform.Find("UIRoot/EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>());
        ResgisterPanel();
        // ObjectManager.Instance.InstantiateObjectAsyn("Assets/GameData/Prefabs/Model/Attack/asil.prefab",
        //     (path,tempObj,param1,param2,param3)=>{
        //         obj = tempObj as GameObject;
        //         ObjectManager.Instance.ReleaseObject(obj);
        //         obj = null;
        //     },
        //     ResLoadPriority.RES_FIRST,true);
        //场景切换
        GameMapManager.Instance.Init(this);
        GameMapManager.Instance.LoadScene(ConstString.MENU_SCENE);
        //预加载
        // ResourceManager.Instance.PreloadResource("Assets/GameData/Sounds/senlin.mp3");
        // ObjectManager.Instance.PreloadGameObject("Assets/GameData/Prefabs/Attack/asil.prefab",20);
 
        // ResourceManager.Instance.AsyncLoadResource("Assets/GameData/Sounds/senlin.mp3",
        //     (path,obj,param1,param2,param3) => {
        //         clip = obj as AudioClip;
        //         audioSource.clip = clip;
        //         audioSource.Play();
        //     },
        // ResLoadPriority.RES_FIRST);

        //同步加载
        // clip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");
        // audioSource.clip = clip;
        // audioSource.Play();
    }

    /// <summary>
    /// 注册界面
    /// </summary>
    void ResgisterPanel(){
        m_WindowTypeDic = new Dictionary<Type,string>(){
            {typeof(StartPanelWindow),"StartPanelWindow.prefab"},
            {typeof(LoadingWindow),"LoadingWindow.prefab"},
        };
        foreach(var item in m_WindowTypeDic){
            UIManager.Instance.Resgister(item.Key,item.Value);
        }
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    void LoadConfig(){
        // MonsterData monsterData = ConfigManager.Instance.LoadData<MonsterData>(ConfigData.CONFIG_MONSTER);
        // for(int i = 0;i<monsterData.AllMonster.Count;i++){
        //     MonsterBase monsterBase = monsterData.AllMonster[i];
        //     MyTool.LogTool.Log(monsterBase.ID,monsterBase.Level,monsterBase.Name,monsterBase.OutLook,monsterBase.AllData,monsterBase.AllString,monsterBase.AllDataList);
        // }
    }

    // Update is called once per frame
    void Update()
    {
        UIManager.Instance.OnUpdate();

        //异步加载实例
        // if(Input.GetKeyDown(KeyCode.A)){
        //     ObjectManager.Instance.InstantiateObjectAsyn("Assets/GameData/Prefabs/Attack/asil.prefab",
        //     (path,tempObj,param1,param2,param3)=>{
        //         obj = tempObj as GameObject;
        //     },
        //     ResLoadPriority.RES_FIRST,true);
        // }
        
        //同步加载实例
        if(Input.GetKeyDown(KeyCode.A)){
        LogTool.Log("//?objpath",obj.transform.parent.name);

            // obj = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack/asil.prefab",true);
        }
        // else if(Input.GetKeyDown(KeyCode.S)){
        //     ObjectManager.Instance.ReleaseObject(obj);
        //     obj = null;
        // }
        // else if(Input.GetKeyDown(KeyCode.D)){
        //     ObjectManager.Instance.ReleaseObject(obj,0,true);
        //     obj = null;
        // }

        
        // if(Input.GetKeyDown(KeyCode.A)){
        //     clip = ReDourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");
        //     audioSource.clip = clip;
        //     audioSource.Play();
        // }
        // else if(Input.GetKeyDown(KeyCode.Space)){
        //     // audioSource.Stop();
        //     ResourceManager.Instance.ReleaseResource(clip,true);
        //     audioSource.clip = null;
        //     clip = null;
        // }
    }

    void OnApplicationQuit(){
#if UNITY_EDITOR
        //编辑器状态下,卸载资源
        Resources.UnloadUnusedAssets();
#endif
    }
}