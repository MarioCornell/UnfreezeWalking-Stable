using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SC_ClientCamera : MonoBehaviour
{
    // public override void OnNetworkSpawn()
    // {
    //     if (IsServer)
    //     {
    //         Destroy(gameObject);
    //     }
    // }

    public Transform ClientHead;
    public Transform ClientControllerL;
    public Transform ClientControllerR;
    public Transform ClientHandL; 
    public Transform ClientHandR; 
    

    private void Start()
    {
        FindObjectOfType<SC_EntitySyncingManager>().ClientHead = ClientHead;
        FindObjectOfType<SC_EntitySyncingManager>().ClientLeftController = ClientControllerL;
        FindObjectOfType<SC_EntitySyncingManager>().ClientRightController = ClientControllerR;
        FindObjectOfType<SC_EntitySyncingManager>().ClientLeftHand = ClientHandL; 
        FindObjectOfType<SC_EntitySyncingManager>().ClientRightHand = ClientHandR; 
    }
}