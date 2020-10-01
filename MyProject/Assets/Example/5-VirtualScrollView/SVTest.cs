using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SVTest : MonoBehaviour
{
    public VirtualScrollview virtualScrollview;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Space)){
            virtualScrollview.Init(10);
        }
    }
}
