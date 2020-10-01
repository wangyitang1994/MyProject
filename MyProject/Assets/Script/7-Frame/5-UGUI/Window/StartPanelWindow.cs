using UnityEngine.UI;
using MyTool;
using UnityEngine;
using System.Collections.Generic;

public class StartPanelWindow : Window
{

#region 自动生成控件绑定代码
    public Text txt_tip;
    public GameObject img_bg;
    public Button btn_start;
    public Button btn_load;
    public Button btn_del;
    public Button btn_exit;
    public Image img_1;
    public Image img_2;
    public Image img_3;
#endregion

    List<GameObject> objList;

    public override void OnAwake(object[] param){
        base.OnAwake(param);
        objList = new List<GameObject>();
    }

    public override void OnOpen(object[] param){
        base.OnOpen(param);
        AddBtnEvent(btn_start,()=>{
            img_bg.SetActive(!img_bg.activeSelf);
            img_1.enabled = !img_1.enabled;
            img_2.enabled = !img_2.enabled;
            img_3.enabled = !img_3.enabled;
            btn_start.gameObject.GetComponentInChildren<Text>().text = !img_bg.activeSelf?"显示":"隐藏";
        });
        AddBtnEvent(btn_load,()=>{
            //从配置读取路径
            MonsterData monsterData = ConfigManager.Instance.LoadData<MonsterData>(ConfigData.CONFIG_MONSTER);
            string path = monsterData.FinMonsterBaseByID(0).OutLook;
            txt_tip.text = "outlook路径："+path;
            txt_tip.color = Color.green;
            GameObject obj = ObjectManager.Instance.InstantiateObject(path,true,false);
            objList.Add(obj);
            obj.transform.position = new Vector3(-17.5f + objList.Count*5,-28,40);
            obj.transform.rotation = Quaternion.Euler(0,180,0);
        });
        AddBtnEvent(btn_del,()=>{
            if(objList.Count == 0){
                txt_tip.text = "没有创建模型";
                txt_tip.color = Color.red;
                return;
            }
            int index = objList.Count-1;
            GameObject obj = objList[index];
            if(obj == null){
                txt_tip.text = "index:"+index+"为空!!";
                txt_tip.color = Color.red;
                return;
            }
            ObjectManager.Instance.ReleaseObject(obj);
            objList.RemoveAt(index);
        });
        AddBtnEvent(btn_exit,()=>{
            Application.Quit();
        });
        ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/mao.jpg",(path,obj,param1,param2,param3) => {
            img_1.sprite = obj as Sprite;
        },true);
        // ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/timg.jpg",(path,obj,param1,param2,param3) => {
        //     img_2.sprite = obj as Sprite;
        // });
        //????????????????????????????
        // Object sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>("Assets/GameData/UGUI/mao.jpg");
        // MyTool.LogTool.Log("//?Sprite",sp);
        Sprite sp1 = ResourceManager.Instance.LoadResource<Sprite>("Assets/GameData/UGUI/mao.jpg");
        img_2.sprite = sp1;
    }
}


