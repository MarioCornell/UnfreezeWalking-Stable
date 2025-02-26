using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_DestroySelfIfVR : MonoBehaviour
{
    private void Awake()
    {
        if (Application.platform == RuntimePlatform.VisionOS
            || Application.platform == RuntimePlatform.Android)
        {
            Destroy(gameObject);
        }
    }
}
