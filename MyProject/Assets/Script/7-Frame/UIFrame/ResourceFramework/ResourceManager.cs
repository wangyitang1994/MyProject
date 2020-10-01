using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//资源加载优先级
public enum ResLoadPriority{
    RES_HIGHT = 0,
    RES_NORMAL,
    RES_SLOW,
    RES_COUNT,
}

//异步加载成功委托
public delegate void AsyncObjFinishCallback(string path,Object obj,object param1 = null,object param2 = null,object param3 = null);
//异步加载实例化委托
public delegate void AsyncResObjFinishCallback(string path,ResourceObject resObj,object param1 = null,object param2 = null,object param3 = null);

//---------ResourceManager---------//
public class ResourceManager:Singleton<ResourceManager>{
    //缓存使用中的资源
    public Dictionary<uint,ResourceItem> AssetDic{get;set;} = new Dictionary<uint, ResourceItem>();
    //缓存引用次数为0的资源，缓存数量最大时释放时间最早的资源
    public ResMapList<ResourceItem> m_NoReferenceAssetMapList = new ResMapList<ResourceItem>();
    //是否从AB包读取
    public bool m_LoadFormAssetBundle{
        get{
#if UNITY_EDITOR
            return false;
#endif
            return true;
        }
    }
    //mono
    protected MonoBehaviour m_StartMono;
    //重新加载时间 毫秒
    private long MAXLOADRESETIME = 20000;
    //最大缓存数量
    int MaxCacheCount = 500;
    
    //预加载资源
    public void PreloadResource(string path){
        Object obj = LoadResource<Object>(path);
        uint crc = Crc32.GetCrc32(path);
        ResourceItem item = GetCacheResourceItem(crc);
        item.m_NeedClear = false;
        ReleaseResource(obj);
    }

    /// <summary>
    /// 清空未引用的缓存
    /// </summary>
    public void ClearResource(){
        List<ResourceItem> tempList = new List<ResourceItem>();
        foreach(ResourceItem item in AssetDic.Values){
            if(item.m_NeedClear){
                tempList.Add(item);    
            }
        }
        foreach(ResourceItem item in tempList){
            DestoryResourceItme(item,true);
        }
        tempList.Clear();
    }

    //增加引用计数(通过resObj)
    public int IncreaseResRef(ResourceObject resObj,int count = 1){
        if(resObj == null) return 0;
        return IncreaseResRef(resObj.m_Crc,count);
    }

    //增加引用计数(通过crc)
    public int IncreaseResRef(uint crc,int count = 1){
        ResourceItem item = null;
        if(!AssetDic.TryGetValue(crc,out item)||item==null){
            return 0;
        }
        item.m_RefCount += count;
        return item.m_RefCount;
    }

    //减少引用计数
    public int ReduceResRef(ResourceObject resObj,int count = 1){
        if(resObj == null) return 0;
        return ReduceResRef(resObj.m_Crc,count);
    }

    //减少引用计数
    public int ReduceResRef(uint crc,int count = 1){
        ResourceItem item = null;
        if(!AssetDic.TryGetValue(crc,out item)||item==null||item.m_RefCount<=0){
            return 0;
        }
        item.m_RefCount -= count;
        return item.m_RefCount;
    }

#region 同步資源加載
    /// <summary>
    /// 同步资源加载，外部直接调用，仅加载不需要实例化的资源，例如Texture,音频等等
    /// </summary>
    /// <param name="path"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T LoadResource<T>(string path)where T:UnityEngine.Object{
        WashOut();
        ResourceItem item = GetResourceItem<T>(path);
        return (T)item.m_Obj;
    }

    //获得资源 没有则创建
    public ResourceItem GetResourceItem<T>(string path)where T:UnityEngine.Object{
        if(string.IsNullOrEmpty(path)){
            return null;
        }
        uint crc = Crc32.GetCrc32(path);
        ResourceItem item = GetCacheResourceItem(crc);
        //有缓存就直接返回
        if(item!=null){
            return item;
        }
        T obj = null;
#if UNITY_EDITOR
        if(!m_LoadFormAssetBundle){
            item = AssetBundleManager.Instance.FindResourceItemByCrc(crc);
            if(item != null && item.m_Obj != null){
                obj = (T)item.m_Obj;
            }else{
                if (item == null)
                {
                    item = new ResourceItem();
                    item.m_Crc = crc;
                }
                obj = LoadResourceByEditor<T>(path);
            }
        }
#endif
        if(obj==null){
            item = AssetBundleManager.Instance.LoadResourceItem(crc);
            if(item!= null && item.m_AssetBundle!=null){
                if(item.m_Obj != null){
                    obj = (T)item.m_Obj;
                }else{
                    obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
                }
            }
        }
        //缓存资源
        CacheResource(path,ref item,obj);
        return item;
    }

    ResourceItem GetCacheResourceItem(uint crc,int addRefCount = 1){
        ResourceItem item = null;
        if(AssetDic.TryGetValue(crc,out item)){
            if(item!=null){
                item.RefCount += addRefCount;
                item.m_LastUseTime = Time.realtimeSinceStartup;
            }
        }
        return item;
    }

    /// <summary>
    /// 缓存资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="item"></param>
    /// <param name="obj"></param>
    /// <param name="addRefCount"></param>
    void CacheResource(string path,ref ResourceItem item,Object obj,int addRefCount = 1){
        if(item == null){
            Debug.LogError("ResourceManager CacheResource:ResoutceItem is null"+" path:"+path);
            return;
        }
        if(obj == null){
            Debug.LogError("ResourceManager CacheResource:Object is null"+" path:"+path);
            return;
        }
        item.m_Obj = obj;
        item.m_Guid = obj.GetInstanceID();
        item.RefCount += addRefCount;
        item.m_LastUseTime = Time.realtimeSinceStartup;
        if(AssetDic.ContainsKey(item.m_Crc)){
            AssetDic[item.m_Crc] = item;
        }else{
            AssetDic.Add(item.m_Crc,item);
        }
    }

    /// <summary>
    /// 当内存占用过多是,清除资源
    /// </summary>
    protected void WashOut(){
        //没有缓存的资源 不需要清除
        if(AssetDic.Count <= 0){
            return;
        }
        while (m_NoReferenceAssetMapList.Size() >= MaxCacheCount)
        {
            for(int i = 0;i < MaxCacheCount/2;i++){
                ResourceItem item = m_NoReferenceAssetMapList.Back();
                DestoryResourceItme(item,true);
            }
        }
    }

    public bool ReleaseResource(ResourceObject resObj,bool is_destory = false){
        uint crc = resObj.m_Crc;
        ResourceItem item = null;
        if(!AssetDic.TryGetValue(crc,out item)){
            Debug.LogError("ResourceManager ReleaseResource(resObj):AssetDic中没有crc");
            return false;
        }
        GameObject.Destroy(resObj.m_CloneObj);
        item.RefCount--;
        DestoryResourceItme(item,is_destory);
        return true;
    }

    /// <summary>
    /// 外部方法释放资源
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="is_destory"></param>
    /// <returns></returns>
    public bool ReleaseResource(Object obj,bool is_destory = false){
        if(obj == null){
            return false;
        }
        int guid = obj.GetInstanceID();
        ResourceItem item = null;
        foreach(ResourceItem res in AssetDic.Values){
            if(res.m_Guid == guid){
                item = res;
                break;
            }
        }
        if(item == null){
            Debug.LogError("ResourceManager.ReleaseResource:AssetDic中没有obj:"+obj.name);
            return false;
        }

        item.RefCount--;
        DestoryResourceItme(item,is_destory);
        return true;
    }

/// <summary>
/// 外部方法释放资源
/// </summary>
/// <param name="path"></param>
/// <param name="is_destory"></param>
/// <returns></returns>
    public bool ReleaseResource(string path,bool is_destory = false){
        if(string.IsNullOrEmpty(path)){
            Debug.LogError("ResourceManager.ReleaseResource path is null");
            return false;
        }
        uint crc = Crc32.GetCrc32(path);
        ResourceItem item = null;
        if(!AssetDic.TryGetValue(crc,out item)){
            Debug.LogError("ResourceManager ReleaseResource:AssetDic中没有path:"+path);
            return false;
        }
        item.RefCount--;
        DestoryResourceItme(item,is_destory);
        return true;
    }

    /// <summary>
    /// 清除资源,需要删除的在AssetBundle里卸载,不需要删除的放到NoReferenceAssetMapList
    /// </summary>
    /// <param name="item"></param>
    /// <param name="is_destory"></param>
    protected void DestoryResourceItme(ResourceItem item,bool is_destory = false){
        if(item == null || item.RefCount > 0){
            return;
        }
        if(!is_destory){
            m_NoReferenceAssetMapList.InsertToHead(item);
            return;
        }
        if(!AssetDic.Remove(item.m_Crc)){
            return;
        }
        m_NoReferenceAssetMapList.Remove(item);
        //释放assetbundle引用
        AssetBundleManager.Instance.ReleaseAsset(item);
        //清空对象池
        ObjectManager.Instance.CleanObjectPoolByCrc(item.m_Crc);
        if(item.m_Obj!=null){
            item.m_Obj = null;
#if UNITY_EDITOR
            //编译器下卸载资源
            Resources.UnloadUnusedAssets();
#endif
        }
    }
    //编辑器中的资源加载
#if UNITY_EDITOR
    protected T LoadResourceByEditor<T>(string path)where T:UnityEngine.Object{
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif
#endregion

#region 异步资源加载
    //异步正在加载参数Dic
    protected Dictionary<uint,AsyncLoadResParam> m_AsyncLoadDic = new Dictionary<uint, AsyncLoadResParam>();
    //正在异步加载的资源优先级列表
    protected List<AsyncLoadResParam>[] m_AsyncLoadingList = new List<AsyncLoadResParam>[(int)ResLoadPriority.RES_COUNT];
    //中间类 正在加载资源信息的 对象池
    protected ClassObjectPool<AsyncLoadResParam> m_AsyncLoadResParamPool = ObjectManager.Instance.GetOrCreteObject<AsyncLoadResParam>(50);
    protected ClassObjectPool<AsyncCallback> m_AsyncCallbackPool = ObjectManager.Instance.GetOrCreteObject<AsyncCallback>(100);
    protected Coroutine m_StartCoroutine;

    //初始化异步加载
    public void InitAsync(MonoBehaviour mono){
        for(int i = 0;i < (int)ResLoadPriority.RES_COUNT;i++){
            m_AsyncLoadingList[i] = new List<AsyncLoadResParam>();
        }
        m_StartMono = mono;
        m_StartCoroutine = m_StartMono.StartCoroutine(AsyncLoadCoroutine());
    }

    IEnumerator AsyncLoadCoroutine(){
        List<AsyncCallback> callbackList = null;
        long lastYieldTime = System.DateTime.Now.Ticks;
        while(true){
            for(int i = 0;i <(int)ResLoadPriority.RES_COUNT;i++){
                if(m_AsyncLoadingList[(int)ResLoadPriority.RES_HIGHT].Count > 0){
                    i = (int)ResLoadPriority.RES_HIGHT;
                }
                else if(m_AsyncLoadingList[(int)ResLoadPriority.RES_NORMAL].Count > 0){
                    i = (int)ResLoadPriority.RES_NORMAL;
                }
                List<AsyncLoadResParam> paramList = m_AsyncLoadingList[i];
                if(paramList.Count <= 0) {continue;}
                AsyncLoadResParam curParam = paramList[0];
                paramList.RemoveAt(0);
                callbackList = curParam.m_AsyncCallbackList;
                ResourceItem item = null;
                Object obj = null;
#if UNITY_EDITOR
                if(!m_LoadFormAssetBundle){
                    if(curParam.m_IsSprite){
                        obj = LoadResourceByEditor<Sprite>(curParam.m_Path);
                    }
                    else{
                        obj = LoadResourceByEditor<Object>(curParam.m_Path);
                    }
                    //模拟异步加载
                    yield return new WaitForSeconds(0.5f);
                    item = AssetBundleManager.Instance.FindResourceItemByCrc(curParam.m_Crc);
                    //直接在编译器下读取 从AssetBundleManager中是取不到的
                    if(item == null){
                        item = new ResourceItem();
                        item.m_Crc = curParam.m_Crc;
                    }
                }
#endif  
                if(obj == null){
                    item = AssetBundleManager.Instance.LoadResourceItem(curParam.m_Crc);
                    if(item != null && item.m_AssetBundle != null){
                        //执行异步加载
                        AssetBundleRequest request;
                        if(curParam.m_IsSprite){
                            request = item.m_AssetBundle.LoadAssetAsync<Sprite>(item.m_AssetName);
                        }else{
                            request = item.m_AssetBundle.LoadAssetAsync(item.m_AssetName);
                        }
                        yield return request;
                        //加载完成
                        if(request.isDone){
                            obj = request.asset;
                            lastYieldTime = System.DateTime.Now.Ticks;
                        }
                    }
                }
                //缓存资源,可能有多个请求加载资源,所以计数需要+callbackList.Count
                CacheResource(curParam.m_Path,ref item,obj,callbackList.Count);
                for(int j = 0;j < callbackList.Count;j++){
                    AsyncCallback callback = callbackList[j];
                    //实例化
                    if(callback != null && callback.m_ResObj != null && callback.m_ResCallback != null){
                        callback.m_ResObj.m_ResItem = item;
                        callback.m_ResCallback(curParam.m_Path,callback.m_ResObj,callback.m_ResObj.param1,callback.m_ResObj.param2,callback.m_ResObj.param3);
                        callback.m_ResCallback = null;
                    }
                    //资源加载
                    if(callback != null && callback.m_Callback != null){
                        callback.m_Callback(curParam.m_Path,obj,callback.param1,callback.param2,callback.param3);
                        callback.m_Callback = null;
                    }
                    m_AsyncCallbackPool.Recycle(callback);
                }
                //清空变量
                obj = null;
                m_AsyncLoadDic.Remove(curParam.m_Crc);//从正在加载列表移除
                callbackList.Clear();
                m_AsyncLoadResParamPool.Recycle(curParam);

                if(System.DateTime.Now.Ticks - lastYieldTime > MAXLOADRESETIME){
                    yield return null;
                    lastYieldTime = System.DateTime.Now.Ticks;
                }
            }
            if(System.DateTime.Now.Ticks - lastYieldTime > MAXLOADRESETIME){
                lastYieldTime = System.DateTime.Now.Ticks;
                yield return null;
            }
        }
    }

    public void CancelAsynLoading(ResourceObject resObj){
        if(resObj == null){
            return;
        }
        AsyncLoadResParam param = null;
        //如果还未加载才可以取消
        if(m_AsyncLoadDic.TryGetValue(resObj.m_Crc,out param) && m_AsyncLoadingList[(int)param.m_Priority].Contains(param)){
            List<AsyncCallback> callbackList = param.m_AsyncCallbackList;
            for(int i = callbackList.Count - 1;i >= 0 ;i--){
                AsyncCallback temp = callbackList[i];
                if(temp != null && temp.m_ResObj == resObj){
                    callbackList.RemoveAt(i);
                    m_AsyncCallbackPool.Recycle(temp);
                }
            }
            if(callbackList.Count <= 0){
                m_AsyncLoadingList[(int)param.m_Priority].Remove(param);
                m_AsyncLoadResParamPool.Recycle(param);
                m_AsyncLoadDic.Remove(resObj.m_Crc);
            }
        }
    }
    
    /// <summary>
    /// 异步加载资源(普通的)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="callback"></param>
    /// <param name="priority"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    public void AsyncLoadResource(string path,AsyncObjFinishCallback callback,bool isSprite = false,object param1 = null,object param2 = null,object param3 = null,ResLoadPriority priority = ResLoadPriority.RES_NORMAL){
        if(m_StartCoroutine == null){
            Debug.LogError("ResourceManager.AsyncLoadResource 协程未启动,无法使用异步加载");
            return;
        }
        WashOut();
        uint crc = Crc32.GetCrc32(path);
        ResourceItem item = GetCacheResourceItem(crc);
        //如果资源已经加载
        if(item != null){
            if(callback != null){
                callback(path,item.m_Obj,param1,param2,param3);
            }
            return;
        }
        //判断是否正在加载
        AsyncLoadResParam asyncParam = null;
        if(!m_AsyncLoadDic.TryGetValue(crc,out asyncParam)){
            //添加到正在加载
            asyncParam = m_AsyncLoadResParamPool.Spawn(true);
            asyncParam.m_Crc = crc;
            asyncParam.m_Path = path;
            asyncParam.m_Priority = priority;
            asyncParam.m_IsSprite = isSprite;
            m_AsyncLoadDic.Add(crc,asyncParam);
            m_AsyncLoadingList[(int)priority].Add(asyncParam);
        }
        //缓存回调信息
        AsyncCallback asyncCallback = m_AsyncCallbackPool.Spawn(true);
        asyncCallback.m_Callback = callback;
        asyncCallback.param1 = param1;
        asyncCallback.param2 = param2;
        asyncCallback.param3 = param3;
        asyncParam.m_AsyncCallbackList.Add(asyncCallback);
        //异步加载在AsyncLoadCoroutine 里执行
    }

    /// <summary>
    /// 异步加载(实例化)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="resObj"></param>
    /// <param name="callback"></param>
    /// <param name="priority"></param>
    public void AsyncLoadResource(string path,ResourceObject resObj,AsyncResObjFinishCallback callback,ResLoadPriority priority){
        if(m_StartCoroutine == null){
            Debug.LogError("ResourceManager.AsyncLoadResource 协程未启动,无法使用异步加载");
            return;
        }
        WashOut();
        ResourceItem item = GetCacheResourceItem(resObj.m_Crc);
        //如果资源已经加载
        if(item != null){
            resObj.m_ResItem = item;
            if(callback != null){
                callback(path,resObj,resObj.param1,resObj.param2,resObj.param3);
            }
            return;
        }
        //判断是否正在加载
        AsyncLoadResParam asyncParam = null;
        if(!m_AsyncLoadDic.TryGetValue(resObj.m_Crc,out asyncParam)){
            //添加到正在加载
            asyncParam = m_AsyncLoadResParamPool.Spawn(true);
            asyncParam.m_Crc = resObj.m_Crc;
            asyncParam.m_Path = path;
            asyncParam.m_Priority = priority;
            m_AsyncLoadDic.Add(resObj.m_Crc,asyncParam);
            m_AsyncLoadingList[(int)priority].Add(asyncParam);
        }
        //缓存回调信息
        AsyncCallback asyncCallback = m_AsyncCallbackPool.Spawn(true);
        asyncCallback.m_ResCallback = callback;
        asyncCallback.m_ResObj = resObj;
        asyncParam.m_AsyncCallbackList.Add(asyncCallback);
        //异步加载在AsyncLoadCoroutine 里执行
    }
    
#endregion
}

//异步加载中间类
public class AsyncLoadResParam{
    //可能同时会有多个协程在加载,加载完后回调一起执行
    public List<AsyncCallback> m_AsyncCallbackList = new List<AsyncCallback>();
    public uint m_Crc = 0;
    public string m_Path = string.Empty;
    public bool m_IsSprite = false;
    public ResLoadPriority m_Priority = ResLoadPriority.RES_SLOW;
    public void Reset(){
        m_AsyncCallbackList.Clear();
        m_Crc = 0;
        m_Path = string.Empty;
        m_IsSprite = false;
        m_Priority = ResLoadPriority.RES_SLOW;
    }
}

//回调中间类
public class AsyncCallback{
    public AsyncObjFinishCallback m_Callback = null;
    public object param1 = null,param2 = null,param3 = null;
//------------实例化--------------
    public AsyncResObjFinishCallback m_ResCallback = null;
    public ResourceObject m_ResObj = null;
    public void Reset(){
        m_Callback = null;
        m_ResCallback = null;
        m_ResObj = null;
        param1 = null;
        param2 = null;
        param3 = null;
    }
}

#region 双向链表
//双向链表节点类
public class DoubleLinkedListNode<T> where T : class,new(){
    public DoubleLinkedListNode<T> preNode = null;
    public DoubleLinkedListNode<T> nextNode = null;
    public T value = null;
}

//双向链表
public class DoubleLinkedList<T> where T : class,new(){
    public DoubleLinkedListNode<T> Head {get;private set;}
    public DoubleLinkedListNode<T> Tail {get;private set;}
    protected ClassObjectPool<DoubleLinkedListNode<T>> m_DoubleLinkedListPool = ObjectManager.Instance.GetOrCreteObject<DoubleLinkedListNode<T>>(500);
    public int Count{get;private set;}

    /// <summary>
    /// 添加节点到头部
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToHeader(T value){
        DoubleLinkedListNode<T> pNode = m_DoubleLinkedListPool.Spawn(true);
        pNode.preNode = null;
        pNode.nextNode = null;
        pNode.value = value;
        return MoveToHead(pNode);
    }

    public DoubleLinkedListNode<T> MoveToHead(DoubleLinkedListNode<T> pNode){
        if(pNode == null) return null;
        if(Head == null && Tail == null){
            Head = Tail = pNode;
        }
        else if(Head == pNode){
            return pNode;
        }
        else{
            Remove(pNode);
            pNode.preNode = null;
            pNode.nextNode = Head;
            Head.preNode = pNode;
            Head = pNode;
        }
        pNode.preNode = null;
        Count++;
        return pNode;
    }

    /// <summary>
    /// 添加节点到尾部
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToTail(T value){
        DoubleLinkedListNode<T> pNode = m_DoubleLinkedListPool.Spawn(true);
        pNode.preNode = null;
        pNode.nextNode = null;
        pNode.value = value;
        return AddToTail(pNode);
    }

    public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> pNode){
        if(pNode == null) return null;
        pNode.nextNode = null;//添加的节点必须是个空节点，否则就乱套了
        if(Tail == null){
            Head = Tail = pNode;
        }
        else{
            pNode.preNode = Tail;
            Tail.nextNode = pNode;
            Tail = pNode;
        }
        Count++;
        return pNode;
    }

    /// <summary>
    /// 清除节点的链式关系
    /// </summary>
    /// <param name="pNode"></param>
    private void Remove(DoubleLinkedListNode<T> pNode){
        if(pNode == null)
            return;

        if(Head == pNode){
            Head = Head.nextNode;
        }
        
        if(Tail == pNode){
            Tail = Tail.preNode;
        }
        
        if(pNode.preNode != null){
            pNode.preNode.nextNode = pNode.nextNode;
        }
        if(pNode.nextNode != null){
            pNode.nextNode.preNode = pNode.preNode;
        }
    }

    /// <summary>
    /// 移除节点
    /// </summary>
    /// <param name="pNode"></param>
    public void RemoveNode(DoubleLinkedListNode<T> pNode){
        Remove(pNode);
        //资源回收
        RecycleNode(pNode);
        Count--;
    }

    /// <summary>
    /// 回收节点
    /// </summary>
    /// <param name="pNode"></param>
    private void RecycleNode(DoubleLinkedListNode<T> pNode){
        pNode.preNode = null;
        pNode.nextNode = null;
        pNode.value = null;
        m_DoubleLinkedListPool.Recycle(pNode);
    }
}

//外部使用 资源映射的list
public class ResMapList<T> where T:class,new(){
    DoubleLinkedList<T> m_DLinkList = new DoubleLinkedList<T>();
    //因为链表不方便获取元素，所以将链表内元素用字典存起来，用来查找
    Dictionary<T,DoubleLinkedListNode<T>> m_FindMapDic = new Dictionary<T, DoubleLinkedListNode<T>>();

    ~ResMapList(){
        Clean();
    }

    /// <summary>
    /// 添加节点到头部
    /// </summary>
    /// <param name="t"></param>
    public void InsertToHead(T t){
        DoubleLinkedListNode<T> node = null;
        if(m_FindMapDic.TryGetValue(t,out node)){
            m_DLinkList.MoveToHead(node);
            return;
        }
        m_DLinkList.AddToHeader(t);
        m_FindMapDic.Add(t,node);
    }

    /// <summary>
    /// 从尾部移除
    /// </summary>
    /// <returns></returns>
    public T PopBack(){
        T temp = null;
        if(m_DLinkList.Tail != null){
            temp = m_DLinkList.Tail.value;
            Remove(temp);
        }
        return temp;
    }

    /// <summary>
    /// 移除一个节点
    /// </summary>
    /// <param name="t"></param>
    public void Remove(T t){
        DoubleLinkedListNode<T> node = null;
        if(m_FindMapDic.TryGetValue(t,out node)){
            m_DLinkList.RemoveNode(node);
            m_FindMapDic.Remove(t);
        }
    }

    /// <summary>
    /// 返回尾部的T
    /// </summary>
    /// <returns></returns>
    public T Back(){
        return m_DLinkList.Tail != null?m_DLinkList.Tail.value : null;
    }

    //双向链表大小
    public int Size(){
        return m_DLinkList.Count;
    }

    public void Clean(){
        while(m_DLinkList.Tail != null){
            Remove(m_DLinkList.Tail.value);
        }
    }

    public bool IsNodeExist(T t){
        DoubleLinkedListNode<T> node = null;
        return m_FindMapDic.TryGetValue(t,out node);
    }
}
#endregion