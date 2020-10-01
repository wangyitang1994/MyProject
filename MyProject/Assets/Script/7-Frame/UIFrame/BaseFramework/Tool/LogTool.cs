using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MyTool
{
    public class LogTool
    {
        public static void Log(params object[] str){
            string temp = "";
            for (int i = 0; i < str.Length; i++)
            {
                temp += ParseArray(str[i]) + " ";
            }
            Debug.Log(temp);
        }

        private static string ParseArray(object value){
            if(!(value is string) && value is IEnumerable){
                string temp = "{";
                int i = 0;
                foreach(var v in (IEnumerable)value){
                    string str = ParseArray(v);
                    temp += string.Format("[{0}]={1},",i,str);
                    i++;
                }
                temp += "}";     
                return temp;
            }
            return value.ToString();
        }

        public static Image CreateImage(string name,Sprite sprite,Transform parent){
            Image img = new GameObject(name).AddComponent<Image>();
            img.transform.SetParent(parent,true);
            return img;
        }

        //轴心的
        private static void StayPosition(RectTransform rtf,Vector2 pivot){
            //计算物体轴心位置
            Vector3[] world_corners = new Vector3[4];
            rtf.GetWorldCorners(world_corners);
            float pivot_pos_x = Mathf.Lerp(world_corners[0].x,world_corners[2].x,pivot.x);
            float pivot_pos_y = Mathf.Lerp(world_corners[0].y,world_corners[2].y,pivot.y);
            Vector2 temp_pos = new Vector2(pivot_pos_x,pivot_pos_y);
            //计算锚框轴心位置
            Vector2[] anchors_world_pos = GetAnchorsWorldPosition(rtf);
            float anchor_x = Mathf.Lerp(anchors_world_pos[0].x,anchors_world_pos[1].x,pivot.x);
            float anchor_y = Mathf.Lerp(anchors_world_pos[0].y,anchors_world_pos[1].y,pivot.y);
            Vector2 anchor_pos = new Vector2(anchor_x,anchor_y);
            //求两点的向量
            rtf.anchoredPosition = temp_pos - anchor_pos;
        }

        //锚点的
        private static void StayPosition(RectTransform rtf,Vector2 anchor_min,Vector2 anchor_max){
            //设置anchoredPosition
            Vector2 pivot = rtf.pivot;
            Vector2[] anchors_world_pos = GetAnchorsWorldPosition(rtf,anchor_min,anchor_max);
            float anchor_x = Mathf.Lerp(anchors_world_pos[0].x,anchors_world_pos[1].x,pivot.x);
            float anchor_y = Mathf.Lerp(anchors_world_pos[0].y,anchors_world_pos[1].y,pivot.y);
            Vector2 anchor_pos = new Vector2(anchor_x,anchor_y);
            rtf.anchoredPosition = (Vector2)rtf.position - anchor_pos;
            //设置sizeDelta
            Vector3[] world_corners = new Vector3[4];
            rtf.GetWorldCorners(world_corners);
            Vector2 cur_offset_max = (Vector2)world_corners[2] - anchors_world_pos[1];
            Vector2 cur_offset_min = (Vector2)world_corners[0] - anchors_world_pos[0];
            Vector2 cur_size_delta = cur_offset_max - cur_offset_min;
            rtf.sizeDelta = cur_size_delta;
        }

        //获得rtf锚框位置的世界坐标
        private static Vector2[] GetAnchorsWorldPosition(RectTransform rtf,Vector2? anchor_min=null,Vector2? anchor_max=null){
            RectTransform praent_rtf = rtf.parent.GetComponent<RectTransform>();
            Vector3[] p_world_corners = new Vector3[4];
            praent_rtf.GetWorldCorners(p_world_corners);
            if(anchor_min == null) anchor_min = rtf.anchorMin;
            if(anchor_max == null) anchor_max = rtf.anchorMax;
            Vector2 min = (Vector2)p_world_corners[0]+(p_world_corners[2]-p_world_corners[0])*(Vector2)anchor_min;
            Vector2 max = (Vector2)p_world_corners[0]+(p_world_corners[2]-p_world_corners[0])*(Vector2)anchor_max;
            return new Vector2[]{min,max};
        }

        public static void SetRectTransformAnchors(RectTransform rtf,AnchorVerType ver_type,AnchorHorType hor_type,bool is_stay = false){
            Vector2 anchors = Vector2.zero;
            switch (hor_type)
            {
                case AnchorHorType.Left:
                    anchors.x = 0;
                break;
                case AnchorHorType.Center:
                    anchors.x = 0.5f;
                break;
                case AnchorHorType.Right:
                    anchors.x = 1;
                break;
            }
            switch (ver_type)
            {
                case AnchorVerType.Bottom:
                    anchors.y = 0;
                break;
                case AnchorVerType.Center:
                    anchors.y = 0.5f;
                break;
                case AnchorVerType.Top:
                    anchors.y = 1;
                break;
            }
            if(is_stay){
                StayPosition(rtf,anchors,anchors);
            }
            rtf.anchorMax = anchors;
            rtf.anchorMin = anchors;
        }
        
        public static void SetRectTransformAnchors(RectTransform rtf,AnchorType anchor,bool is_stay = false){
            Vector2 anchors_min = new Vector2();
            Vector2 anchors_max = new Vector2();
            if(anchor < AnchorType.Full){
                AnchorVerType ver_type = (AnchorVerType)((int)anchor / 3);
                AnchorHorType hor_type = (AnchorHorType)((int)anchor % 3);
                SetRectTransformAnchors(rtf,ver_type,hor_type,is_stay);
                return;
            }
            else if(anchor == AnchorType.Full){
                anchors_min = new Vector2(0,0);
                anchors_max = new Vector2(1,1);
            }
            else if(anchor == AnchorType.StretchLeftAndRight){
                anchors_min = new Vector2(0,0.5f);
                anchors_max = new Vector2(1,0.5f);
            }
            else if(anchor == AnchorType.StretchUpAndDown){
                anchors_min = new Vector2(0.5f,0);
                anchors_max = new Vector2(0.5f,1);
            }
            if(is_stay){
                StayPosition(rtf,anchors_min,anchors_max);
            }
            rtf.anchorMin = anchors_min;
            rtf.anchorMax = anchors_max;
        }

        public static void SetRectTransformPivot(RectTransform rtf,Vector2 pivot,bool is_stay = false){
            if(is_stay){
                StayPosition(rtf,pivot);
            }
            rtf.pivot = pivot;
        }

        //path在List路径之下包含
        public static bool IsPathContains(string path,List<string> path_list)
        {
            for (int i = 0; i < path_list.Count; i++)
            {
                if(path.Length < path_list[i].Length)
                    continue;
                if (path.Contains(path_list[i]))
                {
                    if (path == path_list[i] || path[path_list[i].Length] == '/')
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}