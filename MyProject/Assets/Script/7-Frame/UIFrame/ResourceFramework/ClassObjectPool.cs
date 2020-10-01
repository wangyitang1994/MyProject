using System.Collections.Generic;
using System.Reflection;

// 对象池使用ObjectManager.Instance.GetOrCreteObject<T>(count)创建,在ObjectManager中管理
public class ClassObjectPool<T> where T : class,new(){
    protected Stack<T> m_Pool = new Stack<T>();
    protected int m_MaxCount = 0;    //最大储存个数
    protected int m_NoRecycleNum = 0;//未归还的对象

    public ClassObjectPool(int max_count){
        m_MaxCount = max_count;
        for(int i = 0;i<max_count;i++){
            m_Pool.Push(new T());
        }
    }

    /// <summary>
    /// 从池中取对象
    /// </summary>
    /// <param name="create_if_pool_empty">如果池是空的则创建</param>
    /// <returns></returns>
    public T Spawn(bool create_if_pool_empty = true){
        if(m_Pool.Count > 0){
            T res = m_Pool.Pop();
            if(res == null){
                if(create_if_pool_empty){
                    res = new T();
                    m_NoRecycleNum++;
                }
            }
            if(res != null)
                m_NoRecycleNum++;
            return res;
        }
        else{
            if(create_if_pool_empty){
                m_NoRecycleNum++;
                return new T();
            }
        }
        return null;
    }

    public bool Recycle(T obj){
        if(obj == null)
            return false;
        m_NoRecycleNum--;
        //如果池内对象已经达到最大值，则不放入池中
        if(m_MaxCount > 0 && m_Pool.Count >= m_MaxCount){
            obj = null;
            return false;
        }
        MethodInfo[] methods = obj.GetType().GetMethods();
        foreach(MethodInfo info in methods){
            //如果对象拥有Reset()方法 则自动调用
            if(info.Name == "Reset"){
                obj.GetType().InvokeMember("Reset",BindingFlags.InvokeMethod|BindingFlags.Default,null,obj,new object[]{});
            }
        }
        m_Pool.Push(obj);
        return true;
    }
}