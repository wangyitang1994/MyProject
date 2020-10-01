using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TestXml
{
    [XmlAttribute("ID")]
    public int ID{get;set;}
    [XmlAttribute("Name")]
    public string Name{get;set;}
    [XmlArray("List")]
    public List<int> List{get;set;}
}