using UnityEngine.UI;


public class LoadingWindow : Window
{
    #region 自动生成控件绑定代码
    public Slider sld_progress;
    public Text txt_progress;
    #endregion

    private string sceneName;

    public override void OnAwake(object[] param){
        base.OnAwake(param);
    }

    public override void OnOpen(object[] param){
        base.OnOpen(param);
        sceneName = param[0] as string;
    }

    public override void OnUpdate(){
        base.OnUpdate();
        sld_progress.value = GameMapManager.loadingProgress/100.0f;
        txt_progress.text = string.Format("{0}%",GameMapManager.loadingProgress);
        if(GameMapManager.loadingProgress >= 100){
            LoadOtherScene();
        }
    }

    public void LoadOtherScene(){
        if(sceneName == ConstString.MENU_SCENE){
            UIManager.Instance.PopupWindow(typeof(StartPanelWindow));
        }
        CloseWindow();
    }
}