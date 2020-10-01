using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyTool;

public class SldBar : MonoBehaviour
{

    // private SldType sld_type = SldType.SldShowByValue;
    // private int pre_value;
    // private Sprite[] sprite_group;
    // private Sprite sprite_shield;
    // private Text txt_value_num;
    // private Text txt_cur_value;

    // private float orgin_width = 0;
    // private Image[] template_img;

    // private int value_num;
    // public int ValueNum{
    //     get{return value_num;}
    //     set{
    //         if(value != value_num){
    //             value_num = value;
    //             txt_value_num.text = value_num.ToString();
    //         }
    //     }
    // }
    // private float cur_value;
    // public float CurValue{
    //     get{return value_num;}
    //     set{
    //         if(value != cur_value){
    //             cur_value = value;
    //             txt_cur_value.text = cur_value.ToString();
    //             if(sld_show_function == null){
    //                 return;
    //             }
    //             sld_show_function.SetValue(cur_value);
    //         }
    //     }
    // }
    // private SldShowFunction sld_show_function;
    
    // public void Init(Sprite[] spite = null,Sprite shield = null,int value = 0)
    // {
    //     sprite_group = spite;
    //     sprite_shield = shield;
    //     pre_value = value;
    //     if(template_img == null){
    //         template_img = new Image[3];
    //         for (int i = 0; i < 3; i++)
    //         {
    //             template_img[i] = LogTool.CreateImage("img_bar_"+i,
    //             (sprite_group!=null && i < sprite_group.Length) ? sprite_group[i] : null,
    //             transform);
    //             //LogTool.SetRectTransformAnchors(template_img[i].GetComponent<RectTransform>(),AnchorType.StretchLeftAndRight);
    //             RectTransform rtf = template_img[i].GetComponent<RectTransform>();
    //             rtf.sizeDelta = new Vector2(100,150);
    //             LogTool.SetRectTransformAnchors(rtf,AnchorType.Full);
    //             LogTool.SetRectTransformAnchors(rtf,AnchorType.MidLeft);
    //            // LogTool.SetRectTransformAnchors(rtf,AnchorType.Center);
    //             //LogTool.SetRectTransformPivot(template_img[i].GetComponent<RectTransform>(),new Vector2(0,0.5f));
    //         }
    //     }
    //     if(orgin_width == 0){
    //         orgin_width = template_img[0].gameObject.GetComponent<RectTransform>().rect.width;
    //     }
    //     if (sld_type == SldType.SldShowByValue)
    //     {
    //         sld_show_function = new SldShowByValue();
    //     }
    //     sld_show_function.Init(this,pre_value);
    // }

    //     void Update(){
    //     if (Input.GetKeyDown(KeyCode.W))
    //     {
            
    //     }
    // }

    // public void SetImg(ImgIndex index,float rate){
    //     float width = orgin_width * rate;
    //     RectTransform rtf_img = template_img[(int)index].GetComponent<RectTransform>();
    //     rtf_img.sizeDelta = new Vector2(width,rtf_img.sizeDelta.y);
    // }

    // private enum SldType{
    //     SldShowByValue = 1,
    // }

    // public enum ImgIndex{
    //     Up = 1,
    //     Mid = 2,
    //     Bottom = 3,
    // }
}
