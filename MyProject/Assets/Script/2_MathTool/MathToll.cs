using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class MathTool
{
    public static float AngleToRadian(float angle){
        //float temp_angle = angle>=360 ? angle - 360 :angle;
        float radian = angle/180*Mathf.PI;
        return radian;
    }
}