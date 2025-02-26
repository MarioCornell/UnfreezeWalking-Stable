using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SC_StepReference : MonoBehaviour
{
    public static UnityAction<Vector3> OnScaleChanged;

    private Vector3 lastScale;

    void Start()
    {
        lastScale = transform.localScale;
    }

    void Update()
    {
        if (transform.localScale != lastScale)
        {
            lastScale = transform.localScale;
            OnScaleChanged?.Invoke(lastScale);
        }
    }
}
