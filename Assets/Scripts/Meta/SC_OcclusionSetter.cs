using System.Collections;
using System.Collections.Generic;
using Meta.XR.EnvironmentDepth;
using Michsky.MUIP;
using Unity.Netcode;
using UnityEngine;

public class SC_OcclusionSetter : NetworkBehaviour
{
    private EnvironmentDepthManager _environmentDepthManager;
    public CustomToggle OcclusionToggle;

    public override void OnNetworkSpawn()
    {
        _environmentDepthManager = GetComponent<EnvironmentDepthManager>();
        _environmentDepthManager.OcclusionShadersMode = OcclusionShadersMode.SoftOcclusion;

        if (IsServer)
        {
            OcclusionToggle = GameObject.Find("OcclusionToggle").GetComponentInChildren<CustomToggle>();
            OcclusionToggle.toggleObject.onValueChanged.AddListener(OnOcclusionToggleChanged);
        }
    }
    
    private void OnOcclusionToggleChanged(bool value)
    {
        OnOcclusionToggleChangedClientRpc(value);
        OnOcclusionToggleChangedInternal(value);
    }
    
    [ClientRpc]
    private void OnOcclusionToggleChangedClientRpc(bool value)
    {
        OnOcclusionToggleChangedInternal(value);
    }
    
    private void OnOcclusionToggleChangedInternal(bool value)
    {
        _environmentDepthManager.OcclusionShadersMode = value ? OcclusionShadersMode.SoftOcclusion : OcclusionShadersMode.HardOcclusion;
    }
}
