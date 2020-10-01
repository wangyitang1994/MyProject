using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MyTool;

public class Bundle
{
    //配置路径
    private const string ABCONFIGPATH = "Assets/Script/7-Frame/UIFrame.Editor/Editor/ABConfig.asset";
    //配置二进制路径
    private static string BINARY_PATH = RealConfig.GetRealFram().m_BinaryPath; //"Assets/GameData/Data/AssetBundleConfigData/AssetBundleConfig.bytes";
    private static string BundleTargetPath = Application.dataPath + "/../AssetBundle/" + UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
    //AB包名对应路径 k为name v为路径
    //其实就是ABConfig 配置里的文件路径
    private static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();
    //单个Prefab 名称 对应它的依赖路径
    private static Dictionary<string, List<string>> m_AllPrefabDir = new Dictionary<string, List<string>>();
    //有效路径
    private static List<string> m_VaildDir = new List<string>();

    [MenuItem("工具/打包")]
    public static void Pack()
    {
        //AB包放在项目工程外 避免生成mate文件
        if(!Directory.Exists(BundleTargetPath)){
            Directory.CreateDirectory(BundleTargetPath);
        }
        AssetBunldePathConfig config = AssetDatabase.LoadAssetAtPath<AssetBunldePathConfig>(ABCONFIGPATH);
        //所有xml转二进制
        DataEditor.AssetAllXmlToBinary();
        m_AllFileDir.Clear();
        m_AllPrefabDir.Clear();
        m_VaildDir.Clear();
        //普通文件(没有依赖的)
        foreach (AssetBunldePathConfig.NameAndPath item in config.m_AllFileABPath)
        {
            if (m_AllFileDir.ContainsKey(item.name))
            {
                Debug.LogWarning("Bundle:m_AllFileDir中存在重复key值," + item.name);
                return;
            }
            else
            {
                m_AllFileDir.Add(item.name, item.path);
                m_VaildDir.Add(item.path);
            };
        }
        //Prefab(有依赖的)
        //得到文件夹里的预制guid
        string[] guid = AssetDatabase.FindAssets("t:Prefab", config.m_AllPrefabPath.ToArray());
        for (int i = 0; i < guid.Length; i++)
        {
            //prefab的path(相对路径)
            string path = AssetDatabase.GUIDToAssetPath(guid[i]);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            string[] all_depend = AssetDatabase.GetDependencies(path);
            List<string> depend_path = new List<string>();
            m_VaildDir.Add(path);
            for (int j = 0; j < all_depend.Length; j++)
            {  
                // 过滤掉依赖的脚本(打包脚本没用 而且会占用空间) 并且不在普通文件下 避免重复打包资源
                if (!all_depend[j].EndsWith(".cs") && !LogTool.IsPathContains(all_depend[j],m_VaildDir))
                {
                    depend_path.Add(all_depend[j]);
                }
            }
            // 预制体的依赖信息(不包括普通文件)
            if (!m_AllPrefabDir.ContainsKey(obj.name))
            {
                m_AllPrefabDir.Add(obj.name, depend_path);
            }
            else
            {
                Debug.LogWarning("Bundle:m_AllPrefabDir中存在重复key值," + obj.name);
            }
            EditorUtility.DisplayProgressBar("查找prefab", "Prefab:" + path, i * 1.0f / guid.Length);
        }
        //设置普通文件AB名称
        foreach (string name in m_AllFileDir.Keys)
        {
            SetABName(name, m_AllFileDir[name]);
        }
        //设置预制体AB名称
        foreach (string name in m_AllPrefabDir.Keys)
        {
            SetABName(name, m_AllPrefabDir[name]);
        }
        //打包
        BiuldAssetBundle();
        //设置完成后 把ABName删除，避免manifest文件改变
        string[] ab_names = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < ab_names.Length; i++)
        {
            //forceRemove 是否强制删除
            AssetDatabase.RemoveAssetBundleName(ab_names[i], true);
            EditorUtility.DisplayProgressBar("清除ABName", "Name:" + ab_names[i], i * 1.0f / ab_names.Length);
        }
        AssetDatabase.Refresh();//刷新界面
        EditorUtility.ClearProgressBar();
    }

    //打包
    static void BiuldAssetBundle()
    {
        string[] ab_names = AssetDatabase.GetAllAssetBundleNames();
        //key:path value:ABname
        Dictionary<string, string> ab_path_dic = new Dictionary<string, string>();
        for (int i = 0; i < ab_names.Length; i++)
        {
            string[] ab_path = AssetDatabase.GetAssetPathsFromAssetBundle(ab_names[i]);
            for (int j = 0; j < ab_path.Length; j++)
            {
                if (ab_path[j].EndsWith(".cs")) continue;
                //所有依赖都需要存入字典
                ab_path_dic.Add(ab_path[j], ab_names[i]);
            }
        }
        DeleteAB();
        //生成配置
        //这个path是依赖的 Asset/.../xx.mp3 相对路径
        WriteData(ab_path_dic);
        AssetDatabase.Refresh();
        SetABName("assetbundleconfig", BINARY_PATH);
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(BundleTargetPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        if(manifest == null){
            Debug.LogError("打包失败");
        }
        else{
            Debug.Log("打包成功");
        }
    }

    //将需要的配置生成xml 和 binary
    static void WriteData(Dictionary<string, string> path_dic)
    {
        AssetBundleConfig ab_config = new AssetBundleConfig();
        ab_config.ABList = new List<AssetBundleBase>();
        foreach (string path in path_dic.Keys)
        {
            //path在有效目录下
            if(!LogTool.IsPathContains(path,m_VaildDir))
                continue;
            AssetBundleBase ab_base = new AssetBundleBase();
            ab_base.Path = path;
            ab_base.ABName = path_dic[path];
            ab_base.AssetName = path.Remove(0, path.LastIndexOf('/') + 1);
            ab_base.Crc = Crc32.GetCrc32(path);
            ab_base.ABDependce = new List<string>();
            string[] all_depend = AssetDatabase.GetDependencies(path);
            for (int i = 0; i < all_depend.Length; i++)
            {
                if (path == all_depend[i] || all_depend[i].EndsWith(".cs") )
                    continue;
                string name;
                if (path_dic.TryGetValue(all_depend[i], out name))
                {
                    if (name == ab_base.ABName)//ABName已经添加过了
                        continue;
                    if (!ab_base.ABDependce.Contains(name))
                        ab_base.ABDependce.Add(name);
                }
            }
            ab_config.ABList.Add(ab_base);
        }
        //写入xml
        string xml_path = Application.dataPath + "/Script/7-Frame/2-Bundle/AssetBundleConfig.xml";
        if (File.Exists(xml_path)) File.Delete(xml_path);
        FileStream fs = new FileStream(xml_path, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
        XmlSerializer xs = new XmlSerializer(ab_config.GetType());
        xs.Serialize(sw, ab_config);
        sw.Close();
        fs.Close();
        //写入binary
        foreach (AssetBundleBase ab_base in ab_config.ABList)
        {
            ab_base.Path = "";//使用CRC计算出path，所以不需要path
        }
        fs = new FileStream(BINARY_PATH, FileMode.Create);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs,ab_config);
        fs.Close();
    }

    //普通设置逻辑
    static void SetABName(string name, string path)
    {
        //获取指定路径下的资源导入器
        AssetImporter asset_importer = AssetImporter.GetAtPath(path);
        if (asset_importer == null)
        {
            Debug.LogWarning("不存在的文件路径，path:" + path);
        }
        else
        {
            asset_importer.assetBundleName = name;
        }
    }

    //prefab设置依赖用的
    static void SetABName(string name, List<string> path)
    {
        for (int i = 0; i < path.Count; i++)
        {
            SetABName(name, path[i]);
        }
    }

    //删除变更名称的AB包
    static void DeleteAB()
    {
        string[] names = AssetDatabase.GetAllAssetBundleNames();
        DirectoryInfo dir_info = new DirectoryInfo(BundleTargetPath);
        FileInfo[] file_info = dir_info.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < file_info.Length; i++)
        {
            if (file_info[i].Name.EndsWith("assetbundleconfig") || file_info[i].Name.EndsWith("assetbundleconfig.manifest") || file_info[i].Name.EndsWith(".meta") || IsContainsName(file_info[i].Name, names))
                continue;
            Debug.LogWarning("AB包转移或删除,删除文件:" + file_info[i].FullName);
            file_info[i].Delete();
            //删除对应的manifest文件
            if(File.Exists(file_info[i].FullName + ".manifest"))
            {
                File.Delete(file_info[i].FullName + ".manifest");
            }
        }
    }

    static bool IsContainsName(string name, string[] strs)
    {
        string root_name = Path.GetFileName(BundleTargetPath);
        for (int i = 0; i < strs.Length; i++)
        {
            if (name == strs[i] || name == strs[i] + ".manifest" || name == root_name || name == root_name + ".manifest")
                return true;
        }
        return false;
    }
    // [MenuItem("工具/打包")]
    // public static void Pack()
    // {
    //     //打包所有AB
    //     BuildPipeline.BuildAssetBundles(AssetBundleManager.BundleTargetPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
    //     AssetDatabase.Refresh();//刷新界面
    //     // GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Attack.prefab")) ;
    // }

    // [MenuItem("工具/SetXml")]
    // public static void SetXml()
    // {
    //     // TestXml test = SetXmlInfo();
    //     MonsterData test = new MonsterData();
    //     test.Construction();
    //     SerializeXml(test);
    //     // SerializeXml(test);      //序列化成XML
    //     // SerializeBinary(test);   //序列化成二进制
    // }

    // [MenuItem("工具/GetXml")]
    // public static void GetXml()
    // {
    //     // TestXml test = DeserializeXml();     //XML反序列化
    //     // TestXml test = DeserializeBinary();  //二进制反序列化
    //     // LogTool.Log("ID:",test.ID,"Name:",test.Name,"List:",test.List);
    //     DeserializeAsset();                     //Unity的Asset序列化
    // }

    // public static TestXml SetXmlInfo()
    // {
    //     TestXml test = new TestXml();
    //     test.ID = 1;
    //     test.Name = "AB";
    //     test.List = new List<int>();
    //     test.List.Add(1);
    //     test.List.Add(2);
    //     test.List.Add(3);
    //     return test;
    // }

    #region Xml
    //序列化成XML
    public static void SerializeXml(ExcelBase test)
    {
        FileStream fs = new FileStream(Application.dataPath + "/Script/7-Frame/1-Xml/test.xml", FileMode.Create);
        StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
        XmlSerializer xs = new XmlSerializer(test.GetType());
        xs.Serialize(sw, test);
        sw.Close();
        fs.Close();
    }

    //XML反序列化
    public static TestXml DeserializeXml()
    {
        FileStream fs = new FileStream(Application.dataPath + "/Script/7-Frame/1-Xml/test.xml", FileMode.Open);
        XmlSerializer xs = new XmlSerializer(typeof(TestXml));
        TestXml test = (TestXml)xs.Deserialize(fs);
        fs.Close();
        return test;
    }
    #endregion

    #region Binary
    //序列化成二进制
    public static void SerializeBinary(TestXml test)
    {
        FileStream fs = new FileStream(Application.dataPath + "/Script/7-Frame/1-Xml/test.bytes", FileMode.Create);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, test);
        fs.Close();
    }

    //二进制反序列化
    public static TestXml DeserializeBinary()
    {
        TextAsset ta = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Script/7-Frame/1-Xml/test.bytes");
        MemoryStream ms = new MemoryStream(ta.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        TestXml test = (TestXml)bf.Deserialize(ms);
        ms.Close();
        return test;
    }
    #endregion

    #region Asset
    //Unity的Asset序列化
    public static void DeserializeAsset()
    {
        AssetSerialize ta = AssetDatabase.LoadAssetAtPath<AssetSerialize>("Assets/Script/7-Frame/1-Xml/TestAsset.asset");
        LogTool.Log("ID:", ta.ID, "Name:", ta.Name, "List:", ta.List);
    }
    #endregion
}
