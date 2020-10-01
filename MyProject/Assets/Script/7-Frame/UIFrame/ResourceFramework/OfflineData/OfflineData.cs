using UnityEngine;

public class OfflineData:MonoBehaviour{
    public Rigidbody m_Rigidbody;
    public Collider m_Collider;
    public Transform[] m_AllPoint;
    public int[] m_AllPointChildCount;
    public bool[] m_AllPointActive;
    public Vector3[] m_Pos;
    public Vector3[] m_Scale;
    public Quaternion[] m_Rot;

    public virtual void ResetProp(){
        if(m_AllPoint != null){
            for(int i = 0;i < m_AllPoint.Length;i++){
                Transform temp = m_AllPoint[i];
                if(temp != null){
                    temp.localPosition = m_Pos[i];
                    temp.localScale = m_Scale[i];
                    temp.localRotation = m_Rot[i];
                    bool isActive = m_AllPointActive[i];
                    if(temp.gameObject.activeSelf != isActive){
                        temp.gameObject.SetActive(isActive);
                    }
                }
                //TODO 如何判断插入的情况
                if(temp.childCount > m_AllPointChildCount[i]){
                    for(int j = m_AllPointChildCount[i];j < temp.childCount;j++){
                        GameObject go = temp.GetChild(j).gameObject;
                        if(!ObjectManager.Instance.IsObjectManagerCreate(go)){
                            GameObject.Destroy(go);
                        }
                    }
                }
            }
        }
    }

    public virtual void BindData(){
        m_Rigidbody = gameObject.GetComponentInChildren<Rigidbody>(true);
        m_Collider = gameObject.GetComponentInChildren<Collider>(true);
        m_AllPoint = gameObject.GetComponentsInChildren<Transform>(true);
        int allPointCount = m_AllPoint.Length;
        m_AllPointChildCount = new int[allPointCount];
        m_AllPointActive = new bool[allPointCount];
        m_Pos = new Vector3[allPointCount];
        m_Scale = new Vector3[allPointCount];
        m_Rot = new Quaternion[allPointCount];
        for(int i = 0;i < allPointCount;i++){
            Transform temp = m_AllPoint[i];
            // LogTool.Log("BindData",temp.name,temp.childCount,temp.gameObject.activeSelf);
            m_AllPointChildCount[i] = temp.childCount;
            m_AllPointActive[i] = temp.gameObject.activeSelf;
            m_Pos[i] = temp.localPosition;
            m_Scale[i] = temp.localScale;
            m_Rot[i] = temp.localRotation;
        }
    }
 }