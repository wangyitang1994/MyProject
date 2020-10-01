using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class LoadAssetManager:Singleton<LoadAssetManager>{
    public static void LoadPrefab(string path)
    {
        AssetBundle ab = AssetBundle.LoadFromFile(AssetBundleManager.BundleTargetPath + AssetBundleManager.m_ABConfigABName);
        TextAsset ta = ab.LoadAsset<TextAsset>("AssetBundleConfig");
        MemoryStream ms = new MemoryStream(ta.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        AssetBundleConfig ab_config = (AssetBundleConfig)bf.Deserialize(ms);
        ms.Close();

        uint crc = Crc32.GetCrc32(path);
        AssetBundleBase ab_base = null;
        for (int i = 0; i < ab_config.ABList.Count; i++)
        {
            if(crc == ab_config.ABList[i].Crc){
                ab_base = ab_config.ABList[i];
                break;
            }
        }
        if(ab_base == null){
            Debug.LogError("LoadAssetManager.LoadPrefab:预制不存在,path:"+path);
            return;
        }
        for (int i = 0; i < ab_base.ABDependce.Count; i++)
        {
            AssetBundle.LoadFromFile(AssetBundleManager.BundleTargetPath + "/" + ab_base.ABDependce[i]);
        }
        AssetBundle asset_bundle = AssetBundle.LoadFromFile(AssetBundleManager.BundleTargetPath + "/" + ab_base.ABName);
        GameObject.Instantiate(asset_bundle.LoadAsset<GameObject>(ab_base.ABName));
    }
}