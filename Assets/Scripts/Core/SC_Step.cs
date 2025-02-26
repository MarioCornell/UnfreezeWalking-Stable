using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SC_Step : NetworkBehaviour
{
    private NetworkVariable<bool> isActive = new NetworkVariable<bool>(true);

    public Material NormalMaterial;
    public Material HighlightMaterial;
    
    private void OnEnable()
    {
        SC_StepReference.OnScaleChanged += UpdateLocalScale;
        if (SC_StepReference.OnScaleChanged != null) 
        {
            UpdateLocalScale(GameObject.FindObjectOfType<SC_StepReference>().transform.localScale);
        }

        if (IsServer)
        {
            isActive.Value = true;
        }
        else
        {
            UpdateActiveState(isActive.Value);
        }

        isActive.OnValueChanged += OnActiveStateChanged;
    }

    private void OnDisable()
    {
        SC_StepReference.OnScaleChanged -= UpdateLocalScale;
        isActive.OnValueChanged -= OnActiveStateChanged;

        if (IsServer)
        {
            isActive.Value = false;
        }
    }

    private void UpdateLocalScale(Vector3 newScale)
    {
        transform.localScale = newScale;
    }

    private void OnActiveStateChanged(bool oldValue, bool newValue)
    {
        UpdateActiveState(newValue);
    }

    public void UpdateActiveState(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
    
    public void Highlight()
    {
        if (IsServer)
        {
            HighlightInternal();
            HighlightClientRPC();
        }
    }
    
    [ClientRpc]
    private void HighlightClientRPC()
    {
        HighlightInternal();
    }
    
    public void HighlightInternal()
    {
        GetComponent<Renderer>().material = HighlightMaterial;
    }
    
    public void Unhighlight()
    {
        if (IsServer)
        {
            UnhighlightInternal();
            UnhighlightClientRPC();
        }
    }
    
    [ClientRpc]
    private void UnhighlightClientRPC()
    {
        UnhighlightInternal();
    }
    
    public void UnhighlightInternal()
    {
        GetComponent<Renderer>().material = NormalMaterial;
    }
}