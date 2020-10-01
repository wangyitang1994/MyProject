using System;
using System.IO;
using UnityEngine;

namespace MyTool
{
    public class FileTool
    {
        public static void Copy(string srcPath,string targetPath){
            try
            {
                if(!Directory.Exists(targetPath)){
                    Directory.CreateDirectory(targetPath);
                }
                string dir = targetPath; //Path.Combine(targetPath,Path.GetFileName(srcPath));
                // MyTool.LogTool.Log("//?GetFileSystemEntries",dir,Path.GetFileName(srcPath),srcPath);
                //判断文件夹是否存在
                if(Directory.Exists(srcPath))
                    dir += Path.DirectorySeparatorChar;
                if(!Directory.Exists(dir)){
                    Directory.CreateDirectory(dir);
                }
                //目录下的所有文件夹
                string[] files = Directory.GetFileSystemEntries(srcPath);
                foreach (string file in files)
                {
                    if(Directory.Exists(file)){
                        Copy(file,dir);
                    }
                    else{
                        // MyTool.LogTool.Log("//?Copy",dir + Path.GetFileName(file));
                        File.Copy(file,dir + Path.GetFileName(file),true);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("复制文件\""+ srcPath+"\" 到 \""+ targetPath+"\"错误 :"+e);
                throw;
            }
        }

        public static void Delete(string srcPath){
            try{
                DirectoryInfo dirInfo = new DirectoryInfo(srcPath);
                //获得所有文件和子目录路径
                FileSystemInfo[] fileInfo = dirInfo.GetFileSystemInfos();
                foreach (FileSystemInfo file in fileInfo)
                {
                    //判断文件是否是文件夹
                    if(file is DirectoryInfo){
                        DirectoryInfo temp = new DirectoryInfo(srcPath);
                        temp.Delete(true);
                    }else{
                        File.Delete(file.FullName);
                    }
                }
            }
            catch(Exception e){
                Debug.LogError("删除文件\""+ srcPath+"\"错误 :"+e);
            }
        }
    }
}