using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Object = System.Object;


public class BinarySerializeOpt : Singleton<BinarySerializeOpt>
{
    /// <summary>
    /// 类转成xml
    /// </summary>
    /// <param name="path"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool XmlSerialize(string path,Object obj){
        try
        {
            //在using作用域的末尾自动调用IDisposable接口
            //Close()被设计成public的，并且在Close()里面调用被隐藏的Dispose();
            //而后Dispose()再去调用另一个virtual的Dispose(bool)函数,用户不应该改变Close的行为
            using(FileStream fs = new FileStream(path,FileMode.Create,FileAccess.ReadWrite,FileShare.ReadWrite)){
                using(StreamWriter sw = new StreamWriter(fs,Encoding.UTF8)){
                   XmlSerializer xs = new XmlSerializer(obj.GetType());
                   xs.Serialize(sw,obj);
                }
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("转XML错误 path:"+path+" Obj:"+obj.GetType()+" "+e);
        }
        return false;
    }

    /// <summary>
    /// xml转成类(从文件读取)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T XmlDeserialize<T>(string path) where T:class{
        T t = default(T);
        try
        {
            using(FileStream fs = new FileStream(path,FileMode.Open,FileAccess.ReadWrite,FileShare.ReadWrite)){
                XmlSerializer xs = new XmlSerializer(typeof(T));
                t = xs.Deserialize(fs) as T;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("XML转类错误 path:"+path+" "+e);
        }
        return t;
    }

    public static Object XmlDeserialize(Type type,string path){
        Object obj = null;
        try
        {
            using(FileStream fs = new FileStream(path,FileMode.Open,FileAccess.ReadWrite,FileShare.ReadWrite)){
                XmlSerializer xs = new XmlSerializer(type);
                obj = xs.Deserialize(fs);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("XML转类错误 path:"+path+" "+e);
        }
        return obj;
    }

    /// <summary>
    /// xml转成类(从AB包读取)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T XmlDeserializeRuntime<T>(string path) where T:class{
        T t = default(T);
        TextAsset ta = ResourceManager.Instance.LoadResource<TextAsset>(path);
        if(ta == null){
            Debug.LogError("XmlDeserializeRuntime XML转换错误 path:"+path);
            return null;
        }
        try
        {
            using(MemoryStream ms = new MemoryStream(ta.bytes)){
                XmlSerializer xs = new XmlSerializer(typeof(T));
                t = (T)xs.Deserialize(ms);
            }
            ResourceManager.Instance.ReleaseResource(path,true);
        }
        catch (Exception e)
        {
            Debug.LogError("XML反序列化错误 path:"+path+" "+e);
        }
        return t;
    }

    public static bool BinarySerialize(string path,Object obj){
        try
        {
            using(FileStream fs = new FileStream(path,FileMode.Create,FileAccess.ReadWrite,FileShare.ReadWrite)){
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs,obj);
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("转二进制错误 path:"+path+" Obj:"+obj.GetType()+" "+e);
        }
        return false;
    }

    public static T BinaryDeserialize<T>(string path) where T:class{
        T t = default(T);
        TextAsset ta = ResourceManager.Instance.LoadResource<TextAsset>(path);
        if(ta == null){
            Debug.LogError("BinaryDeserialize Binary转换错误 path:"+path);
            return null;
        }
        try
        {
            using(MemoryStream ms = new MemoryStream(ta.bytes)){
                BinaryFormatter bf = new BinaryFormatter();
                t = (T)bf.Deserialize(ms);
            }
            ResourceManager.Instance.ReleaseResource(path,true);
        }
        catch (Exception e)
        {
            Debug.LogError("Binary反序列化错误 path:"+path+" "+e);
        }
        return t;
    }
}