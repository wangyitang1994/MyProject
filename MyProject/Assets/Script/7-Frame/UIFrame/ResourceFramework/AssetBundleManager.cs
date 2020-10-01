using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


public class AssetBundleManager : Singleton<AssetBundleManager>
{
    //已加载的ResourceItem 配置里记录的信息 key是crc
    protected Dictionary<uint, ResourceItem> m_ResourceItemDic = new Dictionary<uint, ResourceItem>();
    //已加载的AB包 key是crc value是AssetBundle
    protected Dictionary<uint, AssetBundleItem> m_AssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();
    protected ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool = ObjectManager.Instance.GetOrCreteObject<AssetBundleItem>(500);
    //打包目录路径
    public static string BundleTargetPath
    {
        get
        {
            return Application.streamingAssetsPath + "/";
        }
    }
    public static string m_ABConfigABName = "assetbundleconfig";

    /// <summary>
    /// 读取AB包的配置
    /// </summary>
    /// <returns></returns>
    public bool LoadAssetBundleConfig()
    {
        //编译器中streamingAssets目录不放东西 从外部文件夹读取
        //所以返回false 否则读不到
        #if UNITY_EDITOR
        if (!ResourceManager.Instance.m_LoadFormAssetBundle)
            return false;
        #endif
        //游戏开始时加载
        string path = BundleTargetPath + m_ABConfigABName;//Config的AB路径
        AssetBundle ab_config = AssetBundle.LoadFromFile(path);
        //这个string参数不区分大小写 Mainfest的Asset文件
        TextAsset ta = ab_config.LoadAsset<TextAsset>(m_ABConfigABName);
        if (ta == null)
        {
            Debug.LogError("AssetBundleManager.LoadAssetBundleConfig() not exist path:" + path);
            return false;
        }
        //反序列化
        MemoryStream ms = new MemoryStream(ta.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        AssetBundleConfig config = (AssetBundleConfig)bf.Deserialize(ms);
        ms.Close();
        //赋值ResourceItem
        for (int i = 0; i < config.ABList.Count; i++)
        {
            AssetBundleBase ab_base = config.ABList[i];
            // ab_base.Print();
            ResourceItem item = new ResourceItem();
            item.m_Crc = ab_base.Crc;
            item.m_ABName = ab_base.ABName;
            item.m_AssetName = ab_base.AssetName;
            item.m_ABDependce = ab_base.ABDependce;
            if (m_ResourceItemDic.ContainsKey(item.m_Crc))
            {
                Debug.LogError("AssetBundleManager.LoadAssetBundleConfig() Crc is exist ABName:" + ab_base.ABName + " AssetName:" + ab_base.AssetName);
                continue;
            }
            //加载的资源信息保存
            m_ResourceItemDic.Add(item.m_Crc, item);
        }
        return true;
    }

    //通过CRC加载ResourceItem
    public ResourceItem LoadResourceItem(uint crc){
        ResourceItem item = null;
        if(!m_ResourceItemDic.TryGetValue(crc,out item)){
            Debug.LogError("AssetBundleManager.LoadResourceItem() Crc is not exist:"+crc);
            return null;
        }
        if(item.m_AssetBundle == null){
            item.m_AssetBundle = LoadAssetBundle(item.m_ABName);
        }
        if(item.m_ABDependce != null && item.m_ABDependce.Count > 0){
            for(int i = 0;i< item.m_ABDependce.Count;i++){
                //加载依赖
                LoadAssetBundle(item.m_ABDependce[i]);
            }
        }
        return item;
    }

    /// <summary>
    /// 加载AB包
    /// </summary>
    /// <param name="name">streamingAssetsPath ABname</param>
    /// <returns></returns>
    public AssetBundle LoadAssetBundle(string name){
        AssetBundleItem item = null;
        string path = BundleTargetPath + name;
        uint crc = Crc32.GetCrc32(path);
        if(!m_AssetBundleItemDic.TryGetValue(crc,out item)){
            AssetBundle ab = null;
            ab = AssetBundle.LoadFromFile(path);
            if(ab==null){
                Debug.LogError("AssetBundleManager.LoadAssetBundle AB包为null path:"+path);
            }else{
                item = m_AssetBundleItemPool.Spawn(true);
                item.assetBundle = ab;
                item.refCount++;
                m_AssetBundleItemDic.Add(crc,item);
            }     
        }else{
            item.refCount++;
        }
        return item.assetBundle;
    }

    /// <summary>
    /// 卸载资源
    /// </summary>
    /// <param name="item"></param>
    public void ReleaseAsset(ResourceItem item){
        if(item == null){
            Debug.LogError("AssetBundleManager.ReleaseAsset(ResourceItem item) 传入的item是null");
            return;
        }
        //如果是预制体 先卸载其依赖
        if(item.m_ABDependce != null && item.m_ABDependce.Count > 0){
            for(int i = 0;i < item.m_ABDependce.Count;i++){
                UnloadAssetBundle(item.m_ABDependce[i]);
            }
        }
        UnloadAssetBundle(item.m_ABName);
    }

    /// <summary>
    /// 卸载AB包
    /// </summary>
    /// <param name="name"></param>
    private void UnloadAssetBundle(string name){
        string path = BundleTargetPath +"/"+ name;
        uint crc = Crc32.GetCrc32(path);
        AssetBundleItem item = null;
        if(m_AssetBundleItemDic.TryGetValue(crc,out item)){
           item.refCount--;
           if(item.refCount <= 0 && item.assetBundle != null){
               item.refCount = 0;
               item.assetBundle.Unload(true);
               m_AssetBundleItemPool.Recycle(item);
               m_AssetBundleItemDic.Remove(crc);
           }
        }
    }

    //name = ABName
    /// <summary>
    /// 查找已加载的ResourceItem
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ResourceItem FindResourceItem(string name){
        string path = BundleTargetPath +"/"+ name;
        uint crc = Crc32.GetCrc32(path);
        return m_ResourceItemDic[crc];
    }

    /// <summary>
    /// 通过CRC查找已加载的ResourceItem
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    public ResourceItem FindResourceItemByCrc(uint crc){
        ResourceItem item = null;
        m_ResourceItemDic.TryGetValue(crc, out item);
        return item;
    }
}

public class AssetBundleItem{
    public AssetBundle assetBundle = null;
    public int refCount = 0;
    public void Reset(){
        assetBundle = null;
        refCount = 0;
    }
}

//中间类 保存AB信息
public class ResourceItem
{
    //是否会在Clear时清除
    public bool m_NeedClear = true;
    //资源CRC路径//CRC是 Asset/.../xx.mp3 这种路径
    public uint m_Crc = 0;
    //资源所在AB名称
    public string m_ABName = string.Empty;
    //资源名称
    public string m_AssetName = string.Empty;
    //资源依赖(预制体才有)
    public List<string> m_ABDependce = null;
    //加载好的AB包
    public AssetBundle m_AssetBundle = null;
    //-------
    //实例化对象的额外信息
    public Object m_Obj = null;
    //唯一标识
    public int m_Guid = 0;
    //最后使用 时间
    public float m_LastUseTime = 0.0f;

    //引用次数
    public int m_RefCount = 0;
    public int RefCount{
        get{return m_RefCount;}
        set{
            if(value < 0){
                Debug.LogError("Error:m_RefCount < 0 "+(m_Obj != null?m_Obj.name:"m_Obj is null"));
                return;
            }else{
                m_RefCount = value;
            }
        }
    }

}
