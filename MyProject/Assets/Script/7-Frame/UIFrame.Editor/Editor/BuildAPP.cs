using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using MyTool;

public class BuildAPP {
    private static string BundleTargetPath = Application.dataPath + "/../AssetBundle/" + UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
    public static string AppName = PlayerSettings.productName;
    public static string m_AndroidPath = Application.dataPath+"/../BuildTarget/Android/";
    public static string m_IOSPath = Application.dataPath+"/../BuildTarget/IOS/";
    public static string m_WindowsPath = Application.dataPath+"/../BuildTarget/Windows/";

    // [MenuItem("工具/Check")]
    // public static void Check(){
    //     string targetPath = BundleTargetPath;
    //     string srcPath = Application.streamingAssetsPath;
    //     FileTool.Copy(targetPath,srcPath);
    // }

    //弃用
    public static void Copy(string srcPath,string targetPath){
        if(!Directory.Exists(targetPath)){
            Directory.CreateDirectory(targetPath);
        }
        string dir = Path.Combine(targetPath,Path.GetFileName(srcPath));
        //判断文件夹是否存在
        if(Directory.Exists(srcPath))
            dir += Path.DirectorySeparatorChar;
        //目录下的所有文件夹
        string[] files = Directory.GetFileSystemEntries(srcPath);
        foreach (string file in files)
        {
            if(Directory.Exists(file)){
                Copy(file,dir);
            }
            else{
                LogTool.Log(dir,dir + Path.GetFileName(file));
                //File.Copy(file,dir + Path.GetFileName(file),true);
            }
        }
        LogTool.Log(dir);
    }

    

    [MenuItem("工具/打包APP")]
    public static void Build(){
        Bundle.Pack();
        //打包前 将AB包复制到 StreamingAsset目录
        FileTool.Copy(BundleTargetPath,Application.streamingAssetsPath);
        EditorBuildSettingsScene[] scenes = FindEnableScenes();
        string path = null;
        switch(EditorUserBuildSettings.activeBuildTarget){
            case BuildTarget.Android:
                path = m_AndroidPath + AppName + string.Format("_{0:yyyyMMddHHmm}.apk",DateTime.Now);
                break;
            case BuildTarget.iOS:
                path = m_IOSPath + AppName + string.Format("_{0:yyyyMMddHHmm}",DateTime.Now);
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                path = m_WindowsPath + AppName + string.Format("_{0:yyyyMMddHHmm}/{1}.exe",DateTime.Now,AppName);
                break;
        }
        if(path != null){
            try
            {
                BuildPipeline.BuildPlayer(scenes,path,EditorUserBuildSettings.activeBuildTarget,BuildOptions.None);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                return;
            }
            LogTool.Log("项目打包成功...平台:",EditorUserBuildSettings.activeBuildTarget," path：",path);
        }
        AssetDatabase.Refresh();//刷新界面
    }

    private static EditorBuildSettingsScene[] FindEnableScenes(){
        List<EditorBuildSettingsScene> sceneList = new List<EditorBuildSettingsScene>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if(scene.enabled){
                sceneList.Add(scene);
            }
        }
        return sceneList.ToArray();
    }
}