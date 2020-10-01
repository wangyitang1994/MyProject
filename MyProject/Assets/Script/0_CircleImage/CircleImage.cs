
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;
using System.Collections.Generic;
using System;


public class CircleImage : Image
{
    [SerializeField]
    private int segments = 10;
    [SerializeField]
    private Vector2 originPos = Vector2.zero;
    [SerializeField]
    private float redius = 0;
    [SerializeField]
    private float rotation = 90;
    private List<Vector2> vertexList;

    //unity绘制image源码里的
    private Vector4 GetDrawingDimensions()
    {
        var padding = overrideSprite == null ? Vector4.zero : DataUtility.GetPadding(overrideSprite);
        var size = overrideSprite == null ? Vector2.zero : new Vector2(overrideSprite.rect.width, overrideSprite.rect.height);
        Rect r = GetPixelAdjustedRect();
        // Debug.Log(string.Format("r:{2}, size:{0}, padding:{1}", size, padding, r));
        int spriteW = Mathf.RoundToInt(size.x);
        int spriteH = Mathf.RoundToInt(size.y);

        var v = new Vector4(
            padding.x / spriteW,
            padding.y / spriteH,
            (spriteW - padding.z) / spriteW,
            (spriteH - padding.w) / spriteH);
        v = new Vector4(
            r.x + r.width * v.x,
            r.y + r.height * v.y,
            r.x + r.width * v.z,
            r.y + r.height * v.w
        );
        return v;
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        // Vector4 v = GetDrawingDimensions();
        // var uv = (overrideSprite != null) ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
        // print("//?uv"+uv);

        // var color32 = color;
        // toFill.Clear();
        // toFill.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
        // toFill.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
        // toFill.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
        // toFill.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));
        // toFill.AddTriangle(0, 1, 2);
        // toFill.AddTriangle(2, 3, 0);


        vertexList = new List<Vector2>();
        toFill.Clear();
        //Sprite外部UV参数
        Vector4 uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
        //Vector2[] uvs = overrideSprite.uv;
        //获取必要的参数
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        float uv_width = uv.z - uv.x;
        float uv_height = uv.w - uv.y;
        Vector2 uv_centre = new Vector2(uv_width * 0.5f+uv.x, uv_height * 0.5f+uv.y);
        Vector2 rate = new Vector2(uv_width / width,uv_height / height);
        //计算半径和弧度
        float radius = redius <= 0 ? width / 2 : redius;
        float radian =( 2 * Mathf.PI )/ segments;
        //添加初始点
        UIVertex origin = new UIVertex();
        var spriteSize = new Vector2(overrideSprite.rect.width, overrideSprite.rect.height);
        var spritePivot = overrideSprite.pivot / spriteSize;
        var rectPivot = rectTransform.pivot;
        Rect r = GetPixelAdjustedRect();
        var drawingSize = new Vector2(r.width, r.height);
        var spriteBoundSize = overrideSprite.bounds.size;
        var drawOffset = (rectPivot - spritePivot) * drawingSize;
        Vector2[] vertices = overrideSprite.vertices;
        Vector2 origin_pos = new Vector2((0.5f-rectTransform.pivot.x)*width,(0.5f-rectTransform.pivot.y)*height);//原点位置
        origin.color = color;
        origin.position = origin_pos;
        float uv_x = originPos.x * rate.x + uv_centre.x;
        float uv_y = originPos.y * rate.y + uv_centre.y;
        origin.uv0 = new Vector2(uv_x,uv_y);
        toFill.AddVert(origin);
        //添加其他点

        // toFill.AddVert(new Vector3(v.x, v.y), color, new Vector2(uv.x, uv.y));
        // toFill.AddVert(new Vector3(v.x, v.w), color, new Vector2(uv.x, uv.w));
        // toFill.AddVert(new Vector3(v.z, v.w), color, new Vector2(uv.z, uv.w));
        // toFill.AddVert(new Vector3(v.z, v.y), color, new Vector2(uv.z, uv.y));

        float total_radian = 0;
        for (int i = 0; i < segments+1; i++)
        {
            //角度转为弧度
            float x = Mathf.Cos(total_radian+MathTool.AngleToRadian(rotation)) * radius;
            float y = Mathf.Sin(total_radian+MathTool.AngleToRadian(rotation)) * radius;
            float real_x = x;
            float real_y = y;
            // float k = (origin_pos.y - y) / (origin_pos.x - x);
            // if (x > v.z)
            // {
            //     real_x = v.z;
            //     real_y = k * (real_x - x) + y;
            // }
            // if (x < v.x)
            // {
            //     real_x = v.x;
            //     real_y = k * (real_x - x) + y;
            // }
            // if (y > v.w)
            // {
            //     real_y = v.w;
            //     real_x = (real_y - y) / k + x;
            // }
            // if (y < v.y)
            // {
            //     real_y = v.y;
            //     real_x = (real_y - y) / k + x;
            // }
            total_radian += radian;
            origin = new UIVertex();
            Vector2 other_pos = new Vector2(real_x,real_y)+origin_pos;
            origin.color = color;
            origin.position = other_pos;
            uv_x = ((originPos.x+x) * rate.x + uv_centre.x);
            uv_y = ((originPos.y+y )* rate.y + uv_centre.y);
            origin.uv0 = new Vector2(uv_x,uv_y);
            toFill.AddVert(origin);
            vertexList.Add(other_pos);
        }
        //绘制圆形
        for (int i = 1; i <= segments; i++)
        {
            toFill.AddTriangle(i, 0, i + 1);
        }
    }

    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera){
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screenPoint,eventCamera,out localPosition);
        print(IsValid(localPosition,vertexList));
        return IsValid(localPosition,vertexList);
    }
    
    private bool IsValid(Vector2 localPosition,List<Vector2> vertexList){
        return GetCorssCount(localPosition,vertexList)%2 == 1;
    }

    private int GetCorssCount(Vector2 localPosition,List<Vector2> vertexList){
        int count = 0;
        Vector2 vertex_1 = Vector2.zero;
        Vector2 vertex_2 = Vector2.zero;
        int vertexCount = vertexList.Count;
        for (int i = 0; i < vertexCount; i++)
        {
            vertex_1 = vertexList[i];
            vertex_2 = vertexList[(i+1)%vertexCount];
            if (IsInRang(localPosition,vertex_1,vertex_2))
            {
                count++;
            }
        }
        return count;
    }

    private bool IsInRang(Vector2 localPosition,Vector2 vert1,Vector2 vert2){
        return IsInYRang(localPosition.y,vert1.y,vert2.y) && IsInXRang(localPosition,vert1,vert2);
    }

    private bool IsInYRang(float pos_y,float vert1_y,float vert2_y){
        if (vert1_y > vert2_y){
            return vert2_y <= pos_y && pos_y <= vert1_y;
        }
        else{
            return vert1_y <= pos_y && pos_y <= vert2_y;
        }
    }

    private bool IsInXRang(Vector2 localPosition,Vector2 vert1,Vector2 vert2){
        float pos_x = localPosition.x;
        float pos_y = localPosition.y;
        float k = (vert1.y - vert2.y) / (vert1.x - vert2.x);
        float x = (pos_y - vert1.y) / k + vert1.x;
        return pos_x <= x;
    }
}
