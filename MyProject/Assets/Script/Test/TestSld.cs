using UnityEngine;
using UnityEngine.UI;
using MyTool;

public class TestSld : MonoBehaviour
{
    RectTransform rtf;
    void Start(){
        if(rtf == null)
        rtf = gameObject.GetComponent<RectTransform>();
        
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.Space)){
            Vector3[] world_corners = new Vector3[4];
            rtf.GetWorldCorners(world_corners);
            LogTool.Log("world_corners:",world_corners,"world_pos:",rtf.position);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            LogTool.SetRectTransformPivot(rtf,new Vector2(0,0));
        if (Input.GetKeyDown(KeyCode.Alpha2))
            LogTool.SetRectTransformPivot(rtf,new Vector2(0,1));
        if (Input.GetKeyDown(KeyCode.Alpha3))
            LogTool.SetRectTransformPivot(rtf,new Vector2(1,1));
        if (Input.GetKeyDown(KeyCode.Alpha4))
            LogTool.SetRectTransformPivot(rtf,new Vector2(1,0));
        if (Input.GetKeyDown(KeyCode.Alpha5))
            LogTool.SetRectTransformPivot(rtf,new Vector2(0.5f,0.5f));

        if (Input.GetKeyDown(KeyCode.Q))
            LogTool.SetRectTransformAnchors(rtf,AnchorType.BottomLeft);    
        if (Input.GetKeyDown(KeyCode.W))
            LogTool.SetRectTransformAnchors(rtf,AnchorType.TopLeft);
        if (Input.GetKeyDown(KeyCode.E))
            LogTool.SetRectTransformAnchors(rtf,AnchorType.TopRight);
        if (Input.GetKeyDown(KeyCode.R))
            LogTool.SetRectTransformAnchors(rtf,AnchorType.BottomRight);
        if (Input.GetKeyDown(KeyCode.T))
            LogTool.SetRectTransformAnchors(rtf,AnchorType.Center);

        if (Input.GetKeyDown(KeyCode.A))
            LogTool.SetRectTransformAnchors(rtf,AnchorType.Full);
        if (Input.GetKeyDown(KeyCode.S))
            LogTool.SetRectTransformAnchors(rtf,AnchorType.StretchLeftAndRight);
        if (Input.GetKeyDown(KeyCode.D))
            LogTool.SetRectTransformAnchors(rtf,AnchorType.StretchUpAndDown);            
    }
}