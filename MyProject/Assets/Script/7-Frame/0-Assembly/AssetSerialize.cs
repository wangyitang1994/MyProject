using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CreateAssetMenu(fileName = "TestAsset",menuName = "CreateTestAsset",order = 0)]
public class AssetSerialize:ScriptableObject{
    public int ID;
    public string Name;
    public List<int> List;
}