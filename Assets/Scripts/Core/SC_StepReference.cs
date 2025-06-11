using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SC_StepReference : MonoBehaviour
{
    public static UnityAction<Vector3> OnScaleChanged;

    public Material NormalMaterial;
    public Material HighlightMaterial;

    private Vector3 lastScale;
    private Renderer objectRenderer;

    void Start()
    {
        lastScale = transform.localScale;
        objectRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (transform.localScale != lastScale)
        {
            lastScale = transform.localScale;
            OnScaleChanged?.Invoke(lastScale);
        }
    }

    public void Highlight()
    {
        if (objectRenderer != null && HighlightMaterial != null)
        {
            objectRenderer.material = HighlightMaterial;
        }
    }

    public void Unhighlight()
    {
        if (objectRenderer != null && NormalMaterial != null)
        {
            objectRenderer.material = NormalMaterial;
        }
    }
}