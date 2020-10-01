using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor;
using MyTool;

[InitializeOnLoad]
#if UNITY_EDITOR
public class PrefabStageListener : MonoBehaviour
{
    public const string ModelPrefabsPath = "Assets/GameData/Prefabs/Model";
    public const string UIPrefabsPath = "Assets/GameData/Prefabs/UGUI/";
    public const string EffectPrefabsPath = "Assets/GameData/Prefabs/Effect/";

    static PrefabStageListener(){
        // LogTool.Log("PrefabStageListenerStart");
        // PrefabStage.prefabSaved += (go) => {Debug.Log("save");};
        // PrefabStage.prefabSaving += (go) => {Debug.Log("saveing");};
        // PrefabStage.prefabStageClosing += (stage) => {Debug.Log("closing");};
        // PrefabStage.prefabStageOpened += (stage) => {Debug.Log("opened");};

        PrefabUtility.prefabInstanceUpdated += BindOfflineData;
    }

    private enum PrefabType{
        None,
        Default,
        UI,
        Effect,
    }

    //保存预设时 自动挂载离线数据
    public static void BindOfflineData(GameObject go){
        string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
        PrefabType type = PrefabType.None;
        if(path.Contains(PrefabStageListener.UIPrefabsPath)){
            type = PrefabType.UI;
        }
        else if(path.Contains(PrefabStageListener.EffectPrefabsPath)){
            type = PrefabType.Effect;
        }
        else if(path.Contains(PrefabStageListener.ModelPrefabsPath)){
            type = PrefabType.Default;
        }
        if(type != PrefabType.None){
            OfflineData data = null;
            switch(type){
                case PrefabType.UI:
                    data = go.GetComponent<UIOfflineData>();
                    if(data == null){
                        data = go.AddComponent<UIOfflineData>();
                    }
                    break;
                case PrefabType.Effect:
                    data = go.GetComponent<EffectOfflineData>();
                    if(data == null){
                        data = go.AddComponent<EffectOfflineData>();
                    }
                    break;
                case PrefabType.Default:
                    data = go.GetComponent<OfflineData>();
                    if(data == null){
                        data = go.AddComponent<OfflineData>();
                    }
                    break;
            }
            data.BindData();
            EditorUtility.SetDirty(go);
            Resources.UnloadUnusedAssets();
        }
        LogTool.Log("预设以保存 prefab:",go.name);
    }
}
#endif
