using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_TeleportToFront : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform destination;
    [SerializeField] private float frontDistance = 1f;
    [SerializeField] private float upDistance = -0.5f;
    [SerializeField] private float rightDistance = 0.5f;


    [ContextMenu("Teleport")]
    public void Teleport()
    {
        target.position = destination.position + destination.forward * frontDistance + destination.up * upDistance + destination.right * rightDistance;
    }
}
