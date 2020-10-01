using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

/***************************************************
   二进制文件命名 BinaryXX.bytes Xml文件命名 XmlXX.xml
***************************************************/

public class ConfigManager : Singleton<ConfigManager>
{
    //已加载的ExcelData
    public Dictionary<string,ExcelBase> m_ExcelDataDic = new Dictionary<string,ExcelBase>();

    /// <summary>
    /// 加载data
    /// </summary>
    /// <param name="path">路径</param>
    /// <typeparam name="T">ExcelBase类</typeparam>
    /// <returns></returns>
    public T LoadData<T>(string path) where T : ExcelBase {
        if(string.IsNullOrEmpty(path)){
            return null;
        }
        if(m_ExcelDataDic.ContainsKey(path)){
            return m_ExcelDataDic[path] as T;
        }
        T data = null;
        data = BinarySerializeOpt.BinaryDeserialize<T>(path);
        //如果在编译器中 未生成二进制 转为xml读取
#if UNITY_EDITOR
        if(data == null){
            Debug.LogWarning("LoadData 未生成Binary文件 path:"+path);
            path = path.Replace("Binary","Xml").Replace(".bytes",".xml");
            data = BinarySerializeOpt.XmlDeserialize<T>(path);
        }
#endif
        if(data != null){
            data.Init();
        }
        return data;
    }

    /// <summary>
    /// 查找data
    /// </summary>
    /// <param name="path">路径</param>
    /// <typeparam name="T">ExcelBase类</typeparam>
    /// <returns></returns>
    public T FindData<T>(string path) where T : ExcelBase {
        if(string.IsNullOrEmpty(path)){
            return null;
        }
        ExcelBase data = null;
        if(!m_ExcelDataDic.TryGetValue(path,out data)){
            data = LoadData<T>(path);
        }
        return data as T;
    }
}

public class ConfigData {
    public const string CONFIG_MONSTER = "Assets/GameData/Data/Binary/MonsterData.bytes";
}