
using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : Singleton<ObjectManager>
{
    //缓存ObjectManager创建的对象池
    protected Dictionary<Type, object> obj_dic = new Dictionary<Type, object>();

    //获得或者创建一个对象池
    //当有对象池的时候则直接取，没有的时候按count的数量创建
    public ClassObjectPool<T> GetOrCreteObject<T>(int count = 1) where T : class, new()
    {
        Type type = typeof(T);
        object out_obj = null;
        if (!obj_dic.TryGetValue(type, out out_obj))
        {
            ClassObjectPool<T> new_pool = new ClassObjectPool<T>(count);
            obj_dic.Add(type, new_pool);
            out_obj = new_pool;
        }
        return out_obj as ClassObjectPool<T>;
    }

    public Transform RecyclePoolTrs;
    public Transform SceneTrs;
    //缓存的对象池 同一路径可能会创建多个 ResourceObject
    protected Dictionary<uint,List<ResourceObject>> m_ObjectPoolDic = new Dictionary<uint, List<ResourceObject>>();
    //所有实例化游戏物体<guid,ResourceObject>
    protected Dictionary<int,ResourceObject> m_ResourceObjectDic = new Dictionary<int, ResourceObject>();
    //实例化的游戏物体的POOL
    protected ClassObjectPool<ResourceObject> m_ResourceObjectPool = null;
    //正在异步加载的Obj
    protected Dictionary<long,ResourceObject> m_AsynLoadingDic = new Dictionary<long, ResourceObject>();
    //异步加载的guid
    private long guid = 0;
    protected long m_AsynGuid {
        get{
            return ++guid;
        }
    }

    //初始化游戏物体的节点
    public void InitRecyclePool(Transform recyclePool,Transform sceneTrs){
        m_ResourceObjectPool = GetOrCreteObject<ResourceObject>(1000);
        RecyclePoolTrs = recyclePool;
        SceneTrs = sceneTrs;
    }

    public void PreloadGameObject(string path,int count = 1,bool isClear = false){
        List<GameObject> objectList = new List<GameObject>();
        for(int i = 0;i < count;i++){
            GameObject go = InstantiateObject(path,false,isClear);
            objectList.Add(go);
        }
        for (int i = 0; i < objectList.Count; i++)
        {
            ReleaseObject(objectList[i]);
        }
        objectList.Clear();
    }

    public OfflineData GetOfflineData(GameObject go){
        ResourceObject ResObj = null;
        if(m_ResourceObjectDic.TryGetValue(go.GetInstanceID(),out ResObj)){
            if(System.Object.ReferenceEquals(ResObj.m_OfflineData,null)){
                return ResObj.m_OfflineData;
            }
        }
        return null;
    }

    #region 异步加载
    /// <summary>
    /// 取消异步加载
    /// </summary>
    /// <param name="guid">异步加载的guid</param>
    public void CancelAsynLoading(long guid){
        if(guid == 0){
            return;
        }
        ResourceObject resObj = null;
        if(m_AsynLoadingDic.TryGetValue(guid,out resObj)){
            //删除队列中的异步加载
            ResourceManager.Instance.CancelAsynLoading(resObj);
            //移除记录的异步加载
            m_AsynLoadingDic.Remove(resObj.m_Guid);
            m_ResourceObjectPool.Recycle(resObj);
        }
    }
    /// <summary>
    /// 异步加载创建实例
    /// </summary>
    /// <param name="path"></param>
    /// <param name="callback"></param>
    /// <param name="priority"></param>
    /// <param name="isSceneObj"></param>
    /// <param name="isClear"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    /// <returns></returns>
    public long InstantiateObjectAsyn(string path,AsyncObjFinishCallback callback,ResLoadPriority priority,
        bool isSceneObj = false,bool isClear = true,
        object param1 = null,object param2 = null,object param3 = null)
    {
        if(string.IsNullOrEmpty(path)){
            return 0;
        }
        uint crc = Crc32.GetCrc32(path);
        ResourceObject resObj = GetObjectFromPool(crc);
        if(resObj != null){
            if(isSceneObj){
                resObj.m_CloneObj.transform.SetParent(SceneTrs);
            }
            if(callback != null){
                callback(path,resObj.m_CloneObj,param1,param2,param3);
            }
            return resObj.m_Guid;
        }
        long guid = m_AsynGuid;
        resObj = m_ResourceObjectPool.Spawn(true);
        resObj.m_Crc = crc;
        resObj.m_IsClear = isClear;
        resObj.m_IsSceneObj = isSceneObj;
        resObj.m_Callback = callback;
        resObj.m_Guid = guid;
        resObj.param1 = param1;
        resObj.param2 = param2;
        resObj.param3 = param3;
        ResourceManager.Instance.AsyncLoadResource(path,resObj,OnResObjFinish,priority);
        m_AsynLoadingDic.Add(guid,resObj);
        return guid;
    }

    //完成回调
    void OnResObjFinish(string path,ResourceObject resObj,object param1 = null,object param2 = null,object param3 = null){
        if(resObj == null){
            return;
        }
        if(resObj.m_ResItem.m_Obj == null){
            return;
        }
        else{
            resObj.m_CloneObj = GameObject.Instantiate(resObj.m_ResItem.m_Obj) as GameObject;
            resObj.m_OfflineData = resObj.m_CloneObj.GetComponent<OfflineData>();
        }
        if(m_AsynLoadingDic.ContainsKey(resObj.m_Guid)){
            m_AsynLoadingDic.Remove(resObj.m_Guid);
        }
        if(resObj.m_IsSceneObj){
            resObj.m_CloneObj.transform.SetParent(SceneTrs);
        }
        int guid = resObj.m_CloneObj.GetInstanceID();
        if(!m_ResourceObjectDic.ContainsKey(guid)){
            m_ResourceObjectDic.Add(guid,resObj);
        }
        if(resObj.m_Callback != null){
            resObj.m_Callback(path,resObj.m_CloneObj,param1,param2,param3);
        }
    }
    #endregion

    /// <summary>
    /// 创建一个实例
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isSceneObj">是否创建在场景节点下</param>
    /// <param name="isClear">是否切换场景删除</param>
    /// <returns></returns>
    public GameObject InstantiateObject(string path,bool isSceneObj = false,bool isClear = true){
        uint crc = Crc32.GetCrc32(path);
        ResourceObject resObj = GetObjectFromPool(crc);
        if(resObj == null){
            resObj = m_ResourceObjectPool.Spawn(true);
            resObj.m_Crc = crc;
            resObj.m_IsClear = isClear;
            //从资源管理中得到ResourceItem,
            // Debug.Log("//?path:"+path);
            resObj.m_ResItem = ResourceManager.Instance.GetResourceItem<UnityEngine.Object>(path);
            if(!System.Object.ReferenceEquals(resObj.m_ResItem.m_Obj,null)){
                resObj.m_CloneObj = GameObject.Instantiate(resObj.m_ResItem.m_Obj) as GameObject;
                resObj.m_OfflineData = resObj.m_CloneObj.GetComponent<OfflineData>();
            }
        }
        if(isSceneObj){
            resObj.m_CloneObj.transform.SetParent(SceneTrs);
        }
        int guid = resObj.m_CloneObj.GetInstanceID();
        if(!m_ResourceObjectDic.ContainsKey(guid)){
            m_ResourceObjectDic.Add(guid,resObj);
        }
        return resObj.m_CloneObj;
    }

    /// <summary>
    /// 通过crc取对象
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    public ResourceObject GetObjectFromPool(uint crc){
        List<ResourceObject> resObjList = null;
        if(m_ObjectPoolDic.TryGetValue(crc,out resObjList) && resObjList != null && resObjList.Count > 0){
            ResourceManager.Instance.IncreaseResRef(crc);
            ResourceObject resObj = resObjList[0];
            resObjList.RemoveAt(0);
            //判断obj是否为空
            GameObject go = resObj.m_CloneObj;
            if(!System.Object.ReferenceEquals(go,null)){
                if(System.Object.ReferenceEquals(resObj.m_OfflineData,null)){
                    resObj.m_OfflineData.ResetProp();
                }
                resObj.m_IsRecycle = false;
#if UNITY_EDITOR
                if (go.name.EndsWith("(Recycle)"))
                {
                    go.name = go.name.Replace("(Recycle)", "");
                }
#endif
            }

            return resObj;
        }
        return null;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="maxCacheCount"></param>
    /// <param name="isDestory"></param>
    /// <param name="isRecycleParent">是否放到RecyclePoolTrs</param>
    public void ReleaseObject(GameObject obj,int maxCacheCount = -1,bool isDestory = false,bool isRecycleParent = true){
        if(obj == null){
            return;
        }
        int guid = obj.GetInstanceID();
        ResourceObject resObj = null;
        if(!m_ResourceObjectDic.TryGetValue(guid,out resObj)){
            Debug.LogError("ReleaseObject:obj不是通过InstantiateObject创建");
            return;
        }
        if(resObj == null){
            Debug.LogError("ReleaseObject:没有存储resobj");
            return;
        }
        if(resObj.m_IsRecycle){
            Debug.LogError("ReleaseObject:resobj已回收,检查引用情况");
            return;
        }
#if UNITY_EDITOR
        resObj.m_CloneObj.name += "(Recycle)";
#endif
        //不保留
        if(maxCacheCount == 0){
            ResourceManager.Instance.ReleaseResource(resObj,true);
            m_ResourceObjectDic.Remove(guid);
            m_ResourceObjectPool.Recycle(resObj);
        }
        else{
            List<ResourceObject> resList = null;
            if(!m_ObjectPoolDic.TryGetValue(resObj.m_Crc,out resList) || resList == null){
                resList = new List<ResourceObject>();
                m_ObjectPoolDic.Add(resObj.m_Crc,resList);
            }
           
            if(resObj.m_CloneObj){
                if(isRecycleParent){
                    resObj.m_CloneObj.transform.SetParent(RecyclePoolTrs);
                }
                else{
                    resObj.m_CloneObj.SetActive(false);
                }
            }
            //释放时全部缓存或者在缓存count以内个数
            if (maxCacheCount < 0 || resList.Count < maxCacheCount){
                resList.Add(resObj);
                resObj.m_IsRecycle = true;
                // ResourceManager.Instance.IncreaseResRef(resObj);
            }
            else{
                ResourceManager.Instance.ReleaseResource(resObj,isDestory);
                m_ResourceObjectDic.Remove(guid);
                m_ResourceObjectPool.Recycle(resObj);  
            }
        }
    }

    /// <summary>
    /// 是否正在异步加载
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public bool IsAsynLoading(long guid){
        return m_AsynLoadingDic[guid] != null;
    }

    /// <summary>
    /// 是否是用ObjectManager创建的游戏物体
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public bool IsObjectManagerCreate(GameObject go){
        if(go == null){
            return false;
        }
        int guid = go.GetInstanceID();
        return m_ResourceObjectDic[guid] != null;
    }

    /// <summary>
    /// 切换场景清空对象池
    /// </summary>
    public void CleanObjectPool(){
        List<GameObject> tempList = new List<GameObject>();
        //实例化的
        foreach (ResourceObject res in m_ResourceObjectDic.Values)
        {
            if(!System.Object.ReferenceEquals(res.m_CloneObj,null) && res.m_IsClear){
                tempList.Add(res.m_CloneObj);
            }
        }
        //缓存的
        foreach (uint key in m_ObjectPoolDic.Keys)
        {
            List<ResourceObject> resObjList = m_ObjectPoolDic[key];
            for(int i = resObjList.Count - 1;i >= 0;i--){
                ResourceObject resObj = resObjList[i];
                if(!System.Object.ReferenceEquals(resObj.m_CloneObj,null) && resObj.m_IsClear){
                    tempList.Add(resObj.m_CloneObj);
                }
            }
        }
        foreach (GameObject go in tempList)
        {
            ReleaseObject(go,0,true,false);
        }
    }

    /// <summary>
    /// 通过crc清除游戏物体
    /// </summary>
    public void CleanObjectPoolByCrc(uint crc){
        List<ResourceObject> resObjList = null;
        if(!m_ObjectPoolDic.TryGetValue(crc,out resObjList) || resObjList == null){
            return;
        }
        for(int i = resObjList.Count - 1;i >= 0;i--){
            ResourceObject resObj = resObjList[i];
            if(resObj.m_IsClear){
                GameObject.Destroy(resObj.m_CloneObj);
                m_ResourceObjectPool.Recycle(resObj);
                resObjList.Remove(resObj);
            }
        }
        if(resObjList.Count <= 0){
            m_ObjectPoolDic.Remove(crc);
        }
    }
}

//同步异步加载 中间类
public class ResourceObject{
    public uint m_Crc = 0;
    public GameObject m_CloneObj = null;
    public long m_Guid = 0;
    public bool m_IsClear = true;//切换场景是否清除
    public bool m_IsRecycle = false;//是否已经回收
    public ResourceItem m_ResItem = null;
    public OfflineData m_OfflineData = null;//离线数据
    //--------异步---------
    public bool m_IsSceneObj = false;//放在主场景节点下
    public AsyncObjFinishCallback m_Callback = null;//异步加载完成回调
    public object param1,param2,param3 = null;
    public void Reset(){
        m_Crc = 0;
        m_CloneObj = null;
        m_Guid = 0;
        m_IsClear = true;
        m_ResItem = null;
        m_OfflineData = null;
        m_IsRecycle = false;
        m_IsSceneObj = false;
        m_Callback = null;
        param1 = param2 = param3 = null;
    }
}