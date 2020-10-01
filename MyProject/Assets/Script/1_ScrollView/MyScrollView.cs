using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class MyScrollView : ScrollRect
{
    [SerializeField]
    private float up = 0;
    [SerializeField]
    private float down = 0;
    [SerializeField]
    private float left = 0;
    [SerializeField]
    private float right = 0;

    protected override void SetContentAnchoredPosition(Vector2 position){
        Vector2 result_v2 = position;
        if(vertical){
            if(up > 0)
            {
                if(position.y <= -up){
                    result_v2.y = -up;
                }
            }
            if(down > 0)
            {
                float view_value = viewRect.rect.height;
                float content_value = content.rect.height;
                float limit = content_value <= view_value ? down : content_value - view_value + down;
                if(position.y >= limit){
                    result_v2.y = limit;
                }
            }
        }
        if(horizontal){
            if(left > 0)
            {
                if(position.x <= -left){
                    result_v2.x = -left;
                }
            }
            if(right > 0)
            {
                float view_value = viewRect.rect.width;
                float content_value = content.rect.width;
                float limit = content_value <= view_value ? right : content_value - view_value + right;
                if(position.x >= limit){
                result_v2.x = limit;
                }
            }
        }
        
        base.SetContentAnchoredPosition(result_v2);
    }
}
