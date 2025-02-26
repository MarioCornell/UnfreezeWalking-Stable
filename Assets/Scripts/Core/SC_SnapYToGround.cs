using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SC_SnapYToGround : MonoBehaviour
{
    public GameObject Reference;
    
    private float yScale;

    private void Update()
    {
        yScale = Reference.transform.localScale.y;

        if (yScale < 0)
        {
            Reference.transform.localScale = new Vector3(transform.localScale.x, yScale * -1, transform.localScale.z);
            yScale = Reference.transform.localScale.y;
        }
        
        transform.position = new Vector3(transform.position.x, yScale / 2, transform.position.z);
    }
    
    private void LateUpdate()
    {
        // need to be in late update cuz gizmos tool seems to force position first
        Reference.transform.localPosition = new Vector3(Reference.transform.localPosition.x, 0, Reference.transform.localPosition.z);
    }
}
