using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VirtualScrollview : MonoBehaviour
{
    public GameObject template;
    public ScrollRect scroll_rect;
    public delegate void callback(GameObject cell,int index);
    public Vector2 offset;
    private List<VirtualItem> items;
    private Vector2 cell_size;
    private int data_count;
    private int per_line;
    private int per_count;

    public void Init(int count){
        if(template == null || scroll_rect == null){
            return;
        }
        if(cell_size == Vector2.zero){
            Rect cell_rect = template.GetComponent<RectTransform>().rect;
            cell_size = new Vector2(cell_rect.width,cell_rect.height);
        }
        SetDataCount(count);
    }

    private void SetDataCount(int count){
        if(count == data_count){return;}
        data_count = count;
        SetContentSize();
    }

    private void SetContentSize(){
        if(scroll_rect.vertical){
            per_count = Mathf.FloorToInt(scroll_rect.content.rect.width/(cell_size.x+offset.x));
            if(per_count<=0){
                per_count = 1;
            }
            per_line = Mathf.FloorToInt(data_count/per_count);
            scroll_rect.content.sizeDelta = new Vector2(scroll_rect.content.sizeDelta.x, per_line*cell_size.y+(per_line-1)*offset.y);
        }else if(scroll_rect.horizontal){
           //todo 横向排列
        }
    }
}
