using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.IO;
using System.Xml;
using Object = UnityEngine.Object;
using OfficeOpenXml;
using System.ComponentModel;

public class DataEditor
{
    public static string BINARY_DATA_PATH = RealConfig.GetRealFram().m_BinaryDataPath;// "Assets/GameData/Data/Binary/";
    public static string XML_DATA_PATH = RealConfig.GetRealFram().m_XmlDataPath;//"Assets/GameData/Data/Xml/";
    public static string SCRIPT_DATA_PATH = RealConfig.GetRealFram().m_ScriptDataPath;//"Assets/Script/7-Frame/ConfigData/";
    public static string EXCEL_DATA_PATH = Application.dataPath + "/../ExcelData/Excel/";
    public static string REG_DATA_PATH = Application.dataPath + "/../ExcelData/Reg/";

    static List<string> XML_BASE_TYPE = new List<string>{"listStr","listInt","listBool","listFloat"};

    [MenuItem("Assets/类转Xml")]
    public static void AssetClassToXml(){
        Object[] objs =  Selection.objects;
        for(int i = 0;i < objs.Length;i++){
            EditorUtility.DisplayProgressBar("类转Xml",objs[i].name,1.0f/objs.Length*i);
            ClassToXml(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 实际的类转XML
    /// </summary>
    /// <param name="name"></param>
    static void ClassToXml(string name){
        if (string.IsNullOrEmpty(name))
            return;
        try
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type type = null;
            foreach (Assembly assembly in assemblies)
            {
                type = assembly.GetType(name);
                if(type != null){
                    break;
                }
            }
            if(type != null){
                //反射创建类
                var tempClass = Activator.CreateInstance(type);
                if(tempClass is ExcelBase){
                    //执行编译器下构造方法
                    (tempClass as ExcelBase).Construction();
                }
                string xmlPath = XML_DATA_PATH + name + ".xml";
                if(File.Exists(xmlPath)){
                    Debug.LogError("该类已生成过Xml，需删除后重新生成 path:"+xmlPath);
                }else{
                    BinarySerializeOpt.XmlSerialize(xmlPath,tempClass);
                    Debug.Log("类转Xml成功 path:"+xmlPath);
                }
            }
        }
        catch (System.Exception)
        {
            Debug.LogError("ClassToXml 转换失败 name:"+name);
        }
        
    }

    [MenuItem("Assets/Xml转Binary")]
    public static void AssetsXmlToBinary(){
        Object[] objs =  Selection.objects;
        for(int i = 0;i < objs.Length;i++){
            EditorUtility.DisplayProgressBar("类转Xml",objs[i].name,1.0f/objs.Length*i);
            XmlToBinary(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    static void XmlToBinary(string name){
        try
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type type = null;
            foreach (Assembly assembly in assemblies)
            {
                type = assembly.GetType(name);
                if(type != null){
                    break;
                }
            }
            if(type != null){
                string xmlPath = XML_DATA_PATH + name + ".xml";
                object obj = BinarySerializeOpt.XmlDeserialize(type,xmlPath);
                string binaryPath = BINARY_DATA_PATH + name + ".bytes";
                BinarySerializeOpt.BinarySerialize(binaryPath,obj);
                Debug.Log("Xml转Binary成功 path:"+binaryPath);
            }
        }
        catch (System.Exception)
        {
            Debug.LogError("XmlToBinary 转换失败 name:"+name);
        }
    }

    [MenuItem("工具/全部Xml转Binary")]
    public static void AssetAllXmlToBinary(){
        string path = Application.dataPath.Replace("Assets",XML_DATA_PATH);
        //全路径
        string[] fileNames = Directory.GetFiles(path,"*.*",SearchOption.AllDirectories);
        for(int i = 0;i < fileNames.Length;i++){
            EditorUtility.DisplayProgressBar("Xml转Binary",fileNames[i],1.0f/fileNames.Length*i);
            string className = Path.GetFileName(fileNames[i]);
            if(className.EndsWith(".xml")){
                XmlToBinary(className.Replace(".xml",""));
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("工具/全部Excel转Xml")]
    public static void AssetAllExcelToXml(){
        string path = REG_DATA_PATH;
        //全路径
        string[] fileNames = Directory.GetFiles(path,"*.*",SearchOption.AllDirectories);
        for(int i = 0;i < fileNames.Length;i++){
            EditorUtility.DisplayProgressBar("Excel转Xml",fileNames[i],1.0f/fileNames.Length*i);
            string className = Path.GetFileName(fileNames[i]);
            if(className.EndsWith(".xml")){
                ExcelToXml(className.Replace(".xml",""));
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    private static void ExcelToXml(string name){
        string className = "";
        string from = "";
        string to = "";
        //储存所有变量 sheet名 SheetClass
        Dictionary<string,SheetClass> allSheetClassDic = ReadReg(name,ref className,ref from,ref to);
        //储存所有data sheet名 sheetdata
        Dictionary<string,SheetData> allSheetDataDic = new Dictionary<string, SheetData>();

        string excelPath = EXCEL_DATA_PATH+from;
        // string xmlPath = XML_DATA_PATH+to;
        
        //读取excel文件
        try{
            using(FileStream fs = new FileStream(excelPath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
            using(ExcelPackage package = new ExcelPackage(fs)){
                ExcelWorksheets workSheets = package.Workbook.Worksheets;
                for(int i = 1;i <= workSheets.Count;i++){
                    ExcelWorksheet workSheet = workSheets[i];
                    int colCount = workSheet.Dimension.End.Column;
                    int rowCount = workSheet.Dimension.End.Row;
                    SheetData sheetData = new SheetData();//储存这一页的信息
                    //Reg中sheet的信息
                    SheetClass sheetClass = allSheetClassDic[workSheet.Name];
                    for(int j = 0;j < sheetClass.VarList.Count;j++){
                        VarClass curVarClass = sheetClass.VarList[j];
                        //储存 变量名 类型
                        sheetData.AllName.Add(curVarClass.Name);
                        sheetData.AllType.Add(curVarClass.Type);
                    }
                    //读取excel中的数据
                    //第一行是标题 所以跳过
                    for(int row = 1;row < rowCount;row++){
                        RowData data = new RowData();
                        int col = 0;
                        //如果这页sheet是外键数据 则第一列对应mainKey 数据从第二列开始
                        if(string.IsNullOrEmpty(sheetClass.SplitStr) && !string.IsNullOrEmpty(sheetClass.ParentVar.Foreign)){
                            data.ParentKey = workSheet.Cells[row+1,1].Value.ToString().Trim();
                            col = 1;
                        }
                        for(;col < colCount;col++){
                            //每一行的信息
                            ExcelRange range = workSheet.Cells[row+1,col+1];
                            string colName = workSheet.Cells[1,col+1].Value.ToString();
                            string value = range.Value != null ? range.Value.ToString().Trim() : "";
                            data.RowDataDic.Add(GetNameFormCol(sheetClass.VarList,colName),value);
                        }
                        sheetData.AllRowData.Add(data);
                    }
                    allSheetDataDic.Add(workSheet.Name,sheetData);
                }
            }
        }
        catch (System.Exception e){
            Debug.LogError("XmlToExcel:Excel写入错误 "+e);
            return;
        }
        //创建类 并赋值
        object objClass = CreateClass(className);
        List<string> outKeyList = new List<string>();
        foreach (var item in allSheetClassDic)
        {
            SheetClass sheetClass = item.Value;
            if(sheetClass.Depth == 1){
                outKeyList.Add(item.Key);
            }
        }
        for(int i = 0;i < outKeyList.Count;i++){
            string key = outKeyList[i];
            ReadDataToClass(objClass,allSheetClassDic[key],allSheetDataDic[key],allSheetClassDic,allSheetDataDic);
        }
        string xmlPath = XML_DATA_PATH + name + ".xml";
        object obj = BinarySerializeOpt.XmlSerialize(xmlPath,objClass);
        //转成二进制
        // BinarySerializeOpt.BinarySerialize(BINARY_DATA_PATH, obj);
        Debug.Log("Excel转Xml完成！" + from + "-->" + to);
        AssetDatabase.Refresh();
    }

    public static string GetNameFormCol(List<VarClass> list,string col){
        foreach(VarClass varClass in list){
            if(varClass.Col == col){
                return varClass.Name;
            }
        }
        return null;
    }

    public static void ReadDataToClass(object obj,SheetClass sheetClass,SheetData sheetData,
    Dictionary<string,SheetClass> allSheetClassDic,Dictionary<string,SheetData> allSheetDataDic,object mainKey = null){
        object temp = CreateClass(sheetClass.Name);
        object list = CreateList(temp.GetType());
        for(int i = 0;i < sheetData.AllRowData.Count;i++){
            if(mainKey != null && !string.IsNullOrEmpty(sheetData.AllRowData[i].ParentKey)){
                //如果传进来的主键值 和保存的不同则跳过
                if(mainKey.ToString() != sheetData.AllRowData[i].ParentKey) continue;
            }
            object addItem = CreateClass(sheetClass.Name);
            //一行的数据
            Dictionary<string,string> rowDic = sheetData.AllRowData[i].RowDataDic;
            foreach (VarClass varClass in sheetClass.VarList)
            {
                if(varClass.Type == "list"){
                    if(string.IsNullOrEmpty(varClass.SplitStr)){
                        //变量名获取
                        object curKey = rowDic[sheetClass.MainKey];
                        ReadDataToClass(addItem,allSheetClassDic[varClass.SheetName],allSheetDataDic[varClass.SheetName],allSheetClassDic,allSheetDataDic,curKey);
                    }
                    else{
                        SetSplitClass(addItem,allSheetClassDic[varClass.SheetName],rowDic[varClass.Name]);
                    }
                }
                //xml基础类型
                else if(XML_BASE_TYPE.Contains(varClass.Type)){
                    SetSplitBaseClass(addItem,varClass,rowDic[varClass.Name]);
                }
                else{
                    string value = rowDic[varClass.Name];
                    //设置默认值
                    if(string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(varClass.DefaultValue)){
                        value = varClass.DefaultValue;
                    }
                    if(value == null){
                        Debug.LogError("Excel表未填值 Reg也没有默认值!" + varClass.Col);
                        continue;
                    }
                    SetMemberValue(addItem,varClass.Name,varClass.Type,value);
                }
            }
            list.GetType().InvokeMember("Add",BindingFlags.InvokeMethod|BindingFlags.Default,null,list,new object[]{addItem});
        }
        obj.GetType().GetProperty(sheetClass.ParentVar.Name).SetValue(obj,list);
    }

    /// <summary>
    /// Xml基础类型转Class
    /// </summary>
    /// <param name="addItem"></param>
    /// <param name="varClass"></param>
    /// <param name="value"></param>
    private static void SetSplitBaseClass(object addItem,VarClass varClass,string value){
        Type type = null;
        string varType = varClass.Type;
        switch (varType)
        {
            case "listStr":
                type = typeof(string);
                break;
            case "listInt":
                type = typeof(int);
                break;
            case "listBool":
                type = typeof(bool);
                break;
            case "listFloat":
                type = typeof(float);
                break;    
        }
        string[] strArray = value.Split(new string[]{varClass.SplitStr},StringSplitOptions.None);
        if(type != null){
            object list = CreateList(type);
            for(int i = 0;i < strArray.Length;i++){
                string tempValue = strArray[i];
                if(tempValue == null){
                    Debug.LogWarning("SetSplitBaseClass():" + varClass.SheetName + " 中" +varClass.Name+" 数据为空 检查是否配置正确");
                }
                else{
                    list.GetType().InvokeMember("Add",BindingFlags.Default|BindingFlags.InvokeMethod,null,list,new object[]{tempValue});
                }
            }
            addItem.GetType().GetProperty(varClass.Name).SetValue(addItem,list);
        }
    }
    
    /// <summary>
    /// 集合数据转类
    /// </summary>
    /// <param name="addItem"></param>
    /// <param name="sheetClass"></param>
    /// <param name="value"></param>
    private static void SetSplitClass(object addItem,SheetClass sheetClass,string value){
        if (string.IsNullOrEmpty(value))
        {
            Debug.Log("SetSplitClass(): Excel里面自定义list的列里有空值！" + sheetClass.Name);
            return;
        }
        string[] strArray = value.Split(new string[]{sheetClass.ParentVar.SplitStr},StringSplitOptions.None);
        object tempClass = CreateClass(sheetClass.Name);
        //集合数据的list
        object list = CreateList(tempClass.GetType());
        for(int i = 0;i < strArray.Length;i++){
            string[] itemValue = strArray[i].Trim().Split(new string[]{sheetClass.SplitStr},StringSplitOptions.None);
            object item = CreateClass(sheetClass.Name);
            for(int j = 0;j < itemValue.Length;j++){
                //集合数据的元素
                VarClass curVar = sheetClass.VarList[j];
                SetMemberValue(item,curVar.Name,curVar.Type,itemValue[j].Trim());
            }
            list.GetType().InvokeMember("Add",BindingFlags.Default|BindingFlags.InvokeMethod,null,list,new object[]{item});
        }
        addItem.GetType().GetProperty(sheetClass.ParentVar.Name).SetValue(addItem,list);
    }

    [MenuItem("Assets/XmlToExcel")]
    public static void AssetsXmlToExcel(){
        Object[] objs =  Selection.objects;
        for(int i = 0;i < objs.Length;i++){
            EditorUtility.DisplayProgressBar("Xml转Excel",objs[i].name,1.0f/objs.Length*i);
            XmlToExcel(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    private static void XmlToExcel(string name){
        string className = "";
        string from = "";
        string to = "";
        //储存所有变量 sheet名 SheetClass
        Dictionary<string,SheetClass> allSheetClassDic = ReadReg(name,ref className,ref from,ref to);
        //储存所有data sheet名 sheetdata
        Dictionary<string,SheetData> allSheetDataDic = new Dictionary<string, SheetData>();
        //xml反序列化转成类
        object data = XmlToObject(name);
        //最外层的sheet
        List<SheetClass> outSheetList = new List<SheetClass>();
        foreach (SheetClass sheetClass in allSheetClassDic.Values)
        {
            if(sheetClass.Depth == 1){
                outSheetList.Add(sheetClass);
            }
        }
        //从最外层sheet节点 读取整个sheet
        for(int i = 0;i < outSheetList.Count;i++){
            ReadData(data,outSheetList[i],allSheetClassDic,allSheetDataDic);
        }
        //写入Excel
        string excel_path = EXCEL_DATA_PATH+from;
        if(FileIsUsed(excel_path)){
            return;
        }
        else{
            FileInfo file = new FileInfo(excel_path);
            if(file.Exists){
                file.Delete();
                file = new FileInfo(excel_path);
            }
            try
            {
                using(ExcelPackage package = new ExcelPackage(file)){
                    foreach (var item in allSheetDataDic)
                    {
                        ExcelWorksheet sheet = package.Workbook.Worksheets.Add(item.Key);
                        SheetData sheetData = item.Value;
                        for(int i = 0;i < sheetData.AllName.Count;i++){
                            ExcelRange range = sheet.Cells[1,i+1];
                            range.Value = sheetData.AllName[i];
                            range.AutoFitColumns();
                        }
                        for(int i = 0;i < sheetData.AllRowData.Count;i++){
                            Dictionary<string,string> rowDic = sheetData.AllRowData[i].RowDataDic;
                            for(int j = 0;j < rowDic.Count;j++){
                                ExcelRange range = sheet.Cells[i+2,j+1];
                                range.Value = rowDic[sheetData.AllName[j]];
                                range.AutoFitColumns();
                            }
                        }
                    }
                    package.Save();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("XmlToExcel:Excel写入错误 "+e);
                return;
            }
        }
        Debug.Log("生成excel成功"+excel_path);
    }

    //xml反序列化转成类
    private static object XmlToObject(string name){
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        Type type = null;
        foreach (Assembly assembly in assemblies)
        {
            type = assembly.GetType(name);
            if(type != null){
                break;
            }
        }
        if(type != null){
            string xmlPath = XML_DATA_PATH + name + ".xml";
            return BinarySerializeOpt.XmlDeserialize(type,xmlPath);
        }
        return null;
    }

    //读取reg配置信息
    private static Dictionary<string,SheetClass> ReadReg(string name,ref string className,ref string from,ref string to){
        string regPath = REG_DATA_PATH+name+".xml";
        if(!File.Exists(regPath)){
            Debug.LogError("文件不存在 name:" + name);
            return null;
        }
        XmlDocument xd = new XmlDocument();
        XmlReader xr = XmlReader.Create(regPath);
        //忽略xml中的注释
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.IgnoreComments = true;
        xd.Load(xr);
        xr.Close();
        XmlNode node = xd.SelectSingleNode("data");
        XmlElement xe = (XmlElement)node;
        className = xe.GetAttribute("name");
        from = xe.GetAttribute("from");
        to = xe.GetAttribute("to");
        Dictionary<string,SheetClass> allSheetClassDic = new Dictionary<string, SheetClass>();
        ReadXmlElement(xe,allSheetClassDic,0);
        return allSheetClassDic;
    }

    /// <summary>
    /// 判断文件是否被占用
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static bool FileIsUsed(string path){
        bool result = false;
        if(!File.Exists(path)){
            result = false;
        }
        else{
            FileStream file = null;
            try
            {
                //成功打开就是没有占用
                file = File.Open(path,FileMode.Open,FileAccess.ReadWrite,FileShare.ReadWrite);
                result = false;
            }
            catch (System.Exception e)
            {
                //打开出错则是被占用了
                Debug.LogError("文件被占用 无法修改 path:"+path+"\n"+e);
                result = true;
            }
            finally{
                //把之前打开的文件流关掉
                if(file != null){
                    file.Close();
                }
            }
        }
        return result;
    }

    /// <summary>
    /// XmlToExcel 将sheetClass数据读取到类
    /// </summary>
    /// <param name="data">Xml转化的类</param>
    /// <param name="sheetClass">需要读取的sheetClass</param>
    /// <param name="allSheetClassDic">读取</param>
    /// <param name="allSheetDataDic">储存</param>
    private static void ReadData(object data,SheetClass sheetClass,Dictionary<string,SheetClass> allSheetClassDic,Dictionary<string,SheetData> allSheetDataDic,string mainKey = ""){
        VarClass parentClass = sheetClass.ParentVar;
        string name = parentClass.Name;
        //外部主体
        object dataList = GetMemberValue(data,name);
        int count = Convert.ToInt32(dataList.GetType().GetProperty("Count").GetValue(dataList));
        List<VarClass> varList = sheetClass.VarList;//sheet下所有的variable
        //储存name和type
        SheetData sheetData = new SheetData();
        if(!string.IsNullOrEmpty(parentClass.Foreign)){
            sheetData.AllName.Add(parentClass.Foreign);
            sheetData.AllType.Add(parentClass.Type);
        }
        for(int i = 0;i < varList.Count;i++){
            if(!string.IsNullOrEmpty(varList[i].Col)){
                //这里看看是否存 Col
                sheetData.AllName.Add(varList[i].Col);
                sheetData.AllType.Add(varList[i].Type);
            }
        }
        string tempKey = mainKey;
        //储存rowdata
        for(int i = 0;i < count;i++){
            //
            object item = dataList.GetType().GetProperty("Item").GetValue(dataList,new object[]{i});
            //反射rowdata
            RowData rowData = new RowData();
            if(!string.IsNullOrEmpty(parentClass.Foreign) && !string.IsNullOrEmpty(tempKey)){
                rowData.RowDataDic.Add(parentClass.Foreign,tempKey);
            }

            if(!string.IsNullOrEmpty(sheetClass.MainKey)){
                mainKey = GetMemberValue(item,sheetClass.MainKey).ToString();
            }
            //
            for(int j = 0;j < varList.Count;j++){
                VarClass curVarClass = varList[j];
                if(curVarClass.Type == "list"){
                    if(string.IsNullOrEmpty(curVarClass.SplitStr)){
                        SheetClass temp = allSheetClassDic[curVarClass.SheetName];
                        ReadData(item,temp,allSheetClassDic,allSheetDataDic,mainKey);
                    }
                    else{
                        SheetClass temp = allSheetClassDic[curVarClass.SheetName];
                        string split = GetSplitStrList(item,curVarClass,temp);
                        rowData.RowDataDic.Add(curVarClass.Col,split);
                    }
                }
                //xml基础list类型
                else if(XML_BASE_TYPE.Contains(curVarClass.Type)){
                    string split = GetSplitBaseList(item,curVarClass);
                    rowData.RowDataDic.Add(curVarClass.Col,split);
                }
                else{
                    object value = GetMemberValue(item,curVarClass.Name);
                    if(value != null){
                        rowData.RowDataDic.Add(curVarClass.Col,value.ToString());
                    }
                    else{
                        Debug.LogError("ReadData ："+curVarClass.Name+" 反射为空 请检查配置");
                    }
                }
            }
            //储存进字典(Excel表的sheet)
            string key = parentClass.SheetName;
            if(allSheetDataDic.ContainsKey(key)){
                allSheetDataDic[key].AllRowData.Add(rowData);
            }
            else{
                sheetData.AllRowData.Add(rowData);
                allSheetDataDic.Add(key,sheetData);
            }
        }
    }

    /// <summary>
    /// List类型转化成分隔的字符串
    /// </summary>
    /// <param name="data"></param>
    /// <param name="varClass"></param>
    /// <returns></returns>
    private static string GetSplitStrList(object data,VarClass varClass,SheetClass sheetClass ){
        if(string.IsNullOrEmpty(varClass.SplitStr) || string.IsNullOrEmpty(sheetClass.SplitStr)){
             Debug.LogError("GetSplitStrList: List分隔符为空");
            return null;
        }
        object dataList = GetMemberValue(data,varClass.Name);
        int count = Convert.ToInt32(dataList.GetType().GetProperty("Count").GetValue(dataList));
        string str = "";
        for(int i = 0;i < count;i++){
            object value = dataList.GetType().GetProperty("Item").GetValue(dataList,new object[]{i});
            for(int j = 0;j < sheetClass.VarList.Count;j++){
                VarClass curVarClass = sheetClass.VarList[j];
                object varValue = GetMemberValue(value,curVarClass.Name);
                str += varValue.ToString();
                if(j != sheetClass.VarList.Count - 1){
                    str += sheetClass.SplitStr;
                }
            }
            if(i != count - 1){
                str += varClass.SplitStr;
            }
        }
        return str;
    }

    /// <summary>
    /// 基础类型List转化成分隔的字符串
    /// </summary>
    /// <param name="data">Xml转化的类</param>
    /// <param name="varClass">varClass信息</param>
    /// <returns></returns>
    private static string GetSplitBaseList(object data,VarClass varClass){
        if(string.IsNullOrEmpty(varClass.SplitStr)){
            Debug.LogError("GetSplitBaseList: 基础类型List分隔符为空");
            return null;
        }
        object dataList = GetMemberValue(data,varClass.Name);
        int count = Convert.ToInt32(dataList.GetType().GetProperty("Count").GetValue(dataList));
        string str = "";
        for(int i = 0;i < count;i++){
            object value = dataList.GetType().GetProperty("Item").GetValue(dataList,new object[]{i});
            str += value.ToString();
            if(i != count - 1){
                str += varClass.SplitStr;
            }
        }
        return str;
    }

    //递归读取xml中的数据
    private static void ReadXmlElement(XmlElement xe,Dictionary<string,SheetClass> allSheetClassDic,int depth){
        depth++;//sheet表的深度
        foreach (XmlElement item in xe.ChildNodes)
        {
            string type = item.GetAttribute("type");
            if(type == "list"){
                XmlElement first = (XmlElement)item.FirstChild;
                VarClass parentVar = new VarClass(){
                    Name = item.GetAttribute("name"),
                    Type = item.GetAttribute("type"),
                    Col = item.GetAttribute("col"),
                    DefaultValue = item.GetAttribute("default"),
                    Foreign = item.GetAttribute("foreign"),
                    SplitStr = item.GetAttribute("split"),
                };
                //如果是list储存 和sheet的关联信息
                if(parentVar.Type == "list"){
                    parentVar.ListName = first.GetAttribute("name");
                    parentVar.SheetName = first.GetAttribute("sheetname");
                }
                string sheetName = first.GetAttribute("sheetname");
                if(!string.IsNullOrEmpty(sheetName)){
                    if(!allSheetClassDic.ContainsKey(sheetName)){
                        //如果未储存的SheetName 则新建一个储存
                        SheetClass sheetClass = new SheetClass(){
                            ParentVar = parentVar,
                            Name = first.GetAttribute("name"),
                            SheetName = first.GetAttribute("sheetname"),
                            MainKey = first.GetAttribute("mainkey"),
                            SplitStr = first.GetAttribute("split"),
                            Depth = depth,
                        };
                        //遍历包含的变量
                        foreach (XmlElement xmlEle in first.ChildNodes)
                        {
                            VarClass eleVar = new VarClass(){
                                Name = xmlEle.GetAttribute("name"),
                                Type = xmlEle.GetAttribute("type"),
                                Col = xmlEle.GetAttribute("col"),
                                DefaultValue = xmlEle.GetAttribute("deafult"),
                                Foreign = xmlEle.GetAttribute("foreign"),
                                SplitStr = xmlEle.GetAttribute("split"),
                            };
                            //如果是list储存 和sheet的关联信息
                            if(eleVar.Type == "list"){
                                eleVar.ListName = ((XmlElement)xmlEle.FirstChild).GetAttribute("name");
                                eleVar.SheetName = ((XmlElement)xmlEle.FirstChild).GetAttribute("sheetname");
                            }
                            sheetClass.VarList.Add(eleVar);//添加到sheet的变量list
                        }
                        allSheetClassDic.Add(sheetName,sheetClass);//储存到sheet字典
                    }
                }
                ReadXmlElement(first,allSheetClassDic,depth);
            }
        }
    }

    [MenuItem("测试/CheckXml")]
    public static void CheckXml(){
        string regPath = Application.dataPath + "/../ExcelData/Reg/MonsterData.xml";
        XmlDocument xml = new XmlDocument();
        //文件读取用try catch 如果出错文件流未关闭 会造成文件无法修改
        try
        {
            XmlReader reader = XmlReader.Create(regPath);
            xml.Load(reader);
            reader.Close();//读取文件要记得Close
            XmlNode xn = xml.SelectSingleNode("data");//选择xml <data xx></data>
            XmlElement xe = (XmlElement)xn;
            string className = xe.GetAttribute("name");
            string excelName = xe.GetAttribute("from");
            string xmlName = xe.GetAttribute("to");
            MyTool.LogTool.Log("data:",className,excelName,xmlName);
            foreach (XmlNode node in xe.ChildNodes)
            { 
                //读取配置了
                XmlElement nodeElement = (XmlElement)node;
                string name = nodeElement.GetAttribute("name");
                string type = nodeElement.GetAttribute("type");
                MyTool.LogTool.Log("variable:",name,type);
                XmlElement listElement = (XmlElement)node.FirstChild;
                string listName = listElement.GetAttribute("name");
                string sheetName = listElement.GetAttribute("sheetname");
                string mainKey = listElement.GetAttribute("mainkey");
                MyTool.LogTool.Log("list:",listName,sheetName,mainKey);
                foreach (XmlNode list in listElement.ChildNodes)
                {
                    XmlElement valueElement = (XmlElement)list;
                    string valueName = valueElement.GetAttribute("name");
                    string colName = valueElement.GetAttribute("col");
                    string valueType = valueElement.GetAttribute("type");
                    MyTool.LogTool.Log("value:",valueName,colName,valueType);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    [MenuItem("测试/TestXlsx")]
    public static void TestXlsx(){
        string xlsxPath = Application.dataPath+"/../ExcelData/Excel/G_怪物.xlsx";
        FileInfo fileInfo = new FileInfo(xlsxPath);
        //重新生成
        if(fileInfo.Exists){
            fileInfo.Delete();
            fileInfo = new FileInfo(xlsxPath);
        }
        //需要引入dll
        using(ExcelPackage package = new ExcelPackage(fileInfo)){
            ExcelWorksheet sheet = package.Workbook.Worksheets.Add("怪物配置");
            //sheet默认宽 高
            // sheet.DefaultColWidth = 30;
            // sheet.DefaultRowHeight = 30;
            // sheet.Cells.Style.WrapText = true;
            // sheet.TabColor

            ExcelRange range = sheet.Cells[1,1];
            range.Value = "AAA\nAAA";
            //自动适应宽
            range.AutoFitColumns();
            //自动换行
            range.Style.WrapText = true;
            package.Save();
        }
    }

    [MenuItem("测试/TestReflect")]
    public static void TestReflect(){
        TestClass test = new TestClass();
        test.name = "啦啦啦";
        test.id = 123465;
        test.list = new List<int>();
        test.list.Add(11111);
        test.list.Add(22222);
        MyTool.LogTool.Log("name:",GetMemberValue(test,"name"),"id:",GetMemberValue(test,"id"));
        //通过反射获得list数据
        object list = GetMemberValue(test,"list");
        //通过count属性获得list长度 因为是object 需要转成int格式
        int count = Convert.ToInt32(list.GetType().GetProperty("Count").GetValue(list));
        MyTool.LogTool.Log("count:",count);
        for(int i = 0;i < count;i++){
            //new object[]{x} 里填入的是参数 即获取Item[i]
            object item = list.GetType().GetProperty("Item").GetValue(list,new object[]{i});
            MyTool.LogTool.Log("item:",item);
        }
    }

    [MenuItem("测试/TestReflect2")]
    public static void TestReflect2(){
        object testClass = CreateClass("TestClass");
        // testClass.GetType().GetProperty("name").SetValue(testClass,"老王");
        // testClass.GetType().GetProperty("id").SetValue(testClass,Convert.ToInt32("132"));
        // testClass.GetType().GetProperty("tfloat").SetValue(testClass,Convert.ToSingle("132.34"));
        // testClass.GetType().GetProperty("tbool").SetValue(testClass,Convert.ToBoolean("true"));

        SetMemberValue(testClass,"name","string","老王");
        SetMemberValue(testClass,"id","int","132");
        SetMemberValue(testClass,"tfloat","float","132.34");
        SetMemberValue(testClass,"tbool","bool","true");

        //通过属性设置枚举
        PropertyInfo enumInfo = testClass.GetType().GetProperty("ttype");
        object enumType = System.ComponentModel.TypeDescriptor.GetConverter(enumInfo.PropertyType).ConvertFromInvariantString("TT1");
        enumInfo.SetValue(testClass,enumType);
        //通过属性设置List
        // Type listType = typeof(List<>);
        // Type newType = listType.MakeGenericType(new Type[]{typeof(int)});//设置List<T>的类型
        // object tempList = Activator.CreateInstance(newType,new object[]{});//创建一个list
        object tempList = CreateList(typeof(int));
        for (int i = 0; i < 3; i++)
        {
            tempList.GetType().InvokeMember("Add",BindingFlags.InvokeMethod|BindingFlags.Default,null,tempList,new object[]{i+100});
        }
        testClass.GetType().GetProperty("list").SetValue(testClass,tempList);

        MyTool.LogTool.Log("name:",GetMemberValue(testClass,"name"),"id:",GetMemberValue(testClass,"id"),"tfloat:",GetMemberValue(testClass,"tfloat")
        ,"tbool:",GetMemberValue(testClass,"tbool"),"ttype:",GetMemberValue(testClass,"ttype"),"list:",GetMemberValue(testClass,"list"));

    }

    /// <summary>
    /// 创建一个类
    /// </summary>
    /// <param name="name">类名</param>
    /// <returns></returns>
    public static object CreateClass(string name){
        try
        {
            //获取已加载到此应用程序域的执行上下文中的程序集
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type type = null;
            //遍历查找传入的类名
            foreach (Assembly assembly in assemblies)
            {
                type = assembly.GetType(name);
                if(type != null){
                    break;
                }
            }
            if(type != null){
                //反射创建类
                return Activator.CreateInstance(type);
            }
        }
        catch (System.Exception)
        {
            Debug.LogError("CreateClass 创建失败 name:"+name);
        }
        return null;
    }

    /// <summary>
    /// 创建List
    /// </summary>
    /// <param name="type">泛型类型</param>
    /// <returns></returns>
    public static object CreateList(Type type){
        Type listType = typeof(List<>);
        //指定泛型的具体类型
        Type newType = listType.MakeGenericType(new Type[]{type});
        //创建一个list返回
        return Activator.CreateInstance(newType,new object[]{});//创建一个list
    }

    /// <summary>
    /// 通过反射赋值
    /// </summary>
    /// <param name="obj">需要赋值的类</param>
    /// <param name="name">赋值的字段名</param>
    /// <param name="type">字段类型</param>
    /// <param name="value">值</param>
    public static void SetMemberValue(object obj,string name,string type,string value){
        object val = null;
        PropertyInfo info = obj.GetType().GetProperty(name);
        switch (type)
        {
            case "int":
                val = Convert.ToInt32(value);
                break;
            case "float":
                val = Convert.ToSingle(value);
                break;
            case "bool":
                val = Convert.ToBoolean(value);
                break;
            case "string":
                val = value;
                break;
            case "enum":
                val = TypeDescriptor.GetConverter(info.PropertyType).ConvertFromInvariantString(value.ToString());
                break;
        }
        info.SetValue(obj,val);
    }

    /// <summary>
    /// 通过反射获得值  
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public static object GetMemberValue(object obj,string name,BindingFlags flags =
    BindingFlags.Instance|BindingFlags.Public|BindingFlags.Static){
        Type type = obj.GetType();
        MemberInfo[] infos = type.GetMember(name,flags);
        switch(infos[0].MemberType){
            case MemberTypes.Field:
                return type.GetField(name,flags).GetValue(obj);//获取字段value值
            case MemberTypes.Property:
                return type.GetProperty(name,flags).GetValue(obj);//获取字段value值
            default:
                return null;
        }
    }
}

public class VarClass{
    public string Name{get;set;} //变量名
    public string Type{get;set;} //类型
    public string Col{get;set;}  //变量对应的Excel列
    public string DefaultValue{get;set;}  //默认值
    public string Foreign{get;set;}  //外联部分 列名
    public string SplitStr{get;set;}  //分隔符
    //如果是List
    public string ListName{get;set;}  //List的名称
    public string SheetName{get;set;} //Sheet的名称
}

public class SheetClass{
    public VarClass ParentVar{get;set;}//所属父级
    public List<VarClass> VarList = new List<VarClass>();//包含的变量
    public string Name{get;set;} //类名
    public string SheetName{get;set;} //类型
    public string MainKey{get;set;}  //主键 变量名
    public string SplitStr{get;set;}  //分隔符
    public int Depth{get;set;}  //深度
}

//一个sheet页
public class SheetData{
    public List<string> AllName = new List<string>();//所有key=>col
    public List<string> AllType = new List<string>();//所有的类型
    public List<RowData> AllRowData = new List<RowData>();//(多行) key对应的value
}

public class RowData{
    public string ParentKey{get;set;} //如果有外键 表示对应主键的key
    public Dictionary<string,string> RowDataDic = new Dictionary<string, string>();
}

public enum TestType{
    TT1,TT2
}

public class TestClass{
    public string name{get;set;}
    public int id{get;set;}
    public List<int> list{get;set;}
    public float tfloat {get;set;}
    public TestType ttype {get;set;}
    public bool tbool {get;set;}
}
