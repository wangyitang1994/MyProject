using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

//储存AB资源信息
//用于转成xml binary
//开始游戏时通过 LoadAssetBundleConfig 反序列化加载AB资源

[System.Serializable]
public class AssetBundleConfig
{
    [XmlElement("ABList")]
    public List<AssetBundleBase> ABList { get; set; }
}

[System.Serializable]
public class AssetBundleBase
{
    //path = "Assets/.../xx.mp3
    [XmlAttribute("Path")]
    public string Path { get; set; }
    [XmlAttribute("ABName")]
    public string ABName { get; set; }
    [XmlAttribute("AssetName")]
    public string AssetName { get; set; }
    [XmlAttribute("Crc")]
    public uint Crc { get; set; }
    [XmlElement("ABDependce")]
    public List<string> ABDependce { get; set; }

    public void Print(){
        MyTool.LogTool.Log("AssetBundleBase:",Path,ABName,AssetName,Crc,ABDependce);
    }
}