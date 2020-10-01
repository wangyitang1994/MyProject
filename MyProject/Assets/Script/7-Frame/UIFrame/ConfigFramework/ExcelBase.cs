using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ExcelBase
{
    //运行中初始化方法
    public virtual void Init(){ }

#if UNITY_EDITOR
    //编译器下构造方法
    public virtual void Construction(){ }
#endif
}
