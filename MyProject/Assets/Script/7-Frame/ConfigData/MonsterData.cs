using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterData : ExcelBase {
    //序列化的时候忽略
    [XmlIgnore]
    public Dictionary<int,MonsterBase> m_AllMonsterDic = new Dictionary<int, MonsterBase>();

    [XmlElement("AllMonster")]
    public List<MonsterBase> AllMonster {get;set;}

    public override void Init(){
        m_AllMonsterDic.Clear();
        foreach (MonsterBase info in AllMonster)
        {
            if(m_AllMonsterDic.ContainsKey(info.ID)){
                Debug.LogError("ID重复 ID:"+info.ID);
            }
            else{
                m_AllMonsterDic.Add(info.ID,info);
            }
        }
    }

#if UNITY_EDITOR
    //初始化构造 通过类构造出xml结构
    public override void Construction(){
        AllMonster = new List<MonsterBase>();
        for(int k = 0;k <2;k++){
            MonsterBase monster = new MonsterBase();
            monster.ID = k;
            monster.Name = "测试怪物";
            monster.OutLook = "Assets/GameData/Prefabs/Model/Attack/asil.prefab";
            monster.Level = k+1;
            monster.AllString = new List<string>();
            monster.AllString.Add("你好");
            monster.AllString.Add("我好");
            monster.AllString.Add("大家好");
            AllMonster.Add(monster);
            monster.AllData = new List<TestData>();
            for(int i = 0;i < 3;i++){
                TestData temp = new TestData(){
                    Name = "Name"+i,
                    ID = i,
                };
                monster.AllData.Add(temp);
            }
            monster.AllDataList = new List<TestData>();
            for(int i = 0;i < 3;i++){
                TestData temp = new TestData(){
                    Name = "Name"+i,
                    ID = i,
                };
                monster.AllDataList.Add(temp);
            }
        }
    }
#endif

    public MonsterBase FinMonsterBaseByID(int ID){
        return m_AllMonsterDic[ID];
    }
}

[System.Serializable]
public class MonsterBase {
    [XmlAttribute("ID")]
    public int ID {get;set;}
    //名称
    [XmlAttribute("Name")]
    public string Name {get;set;}
    //预制路径
    [XmlAttribute("OutLook")]
    public string OutLook {get;set;}
    //等级
    [XmlAttribute("Level")]
    public int Level {get;set;}
    //对话
    [XmlElement("AllString")]
    public List<string> AllString {get;set;}
    //集合测试
    [XmlElement("AllData")]
    public List<TestData> AllData {get;set;}
    //外键测试
    [XmlElement("AllDataList")]
    public List<TestData> AllDataList {get;set;}
}

[System.Serializable]
public class TestData
{
    [XmlElement("Name")]
    public string Name{get;set;}
    [XmlElement("ID")]
    public int ID{get;set;}
}