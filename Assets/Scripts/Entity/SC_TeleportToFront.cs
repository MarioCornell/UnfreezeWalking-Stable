using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_TeleportToFront : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The object that will be teleported.")]
    [SerializeField] private Transform target;
    [Tooltip("The reference transform (e.g., player camera) to teleport in front of.")]
    [SerializeField] private Transform destination;

    [Header("Position Offset")]
    [Tooltip("How far in front of the destination to teleport.")]
    [SerializeField] private float frontDistance = 0.7f;
    [Tooltip("How far above/below the destination's origin to teleport.")]
    [SerializeField] private float upDistance = -0.5f;
    [Tooltip("How far to the left/right of the destination to teleport.")]
    [SerializeField] private float rightDistance = 0.5f;

    [SerializeField] private float targetYPosition = 1.0f; 

    [Tooltip("If checked, the target will rotate to face the destination (on the Y-axis only).")]
    [SerializeField] private bool faceDestination = true;


    [ContextMenu("Teleport")]
    public void Teleport()
    {
        if (target == null || destination == null)
        {
            Debug.LogWarning("Target or Destination transform is not set.", this);
            return;
        }
        
        Vector3 targetPos = destination.position +
                            destination.forward * frontDistance +
                            destination.up * upDistance +
                            destination.right * rightDistance;

     
        targetPos.y = targetYPosition;
        

        target.position = targetPos;


        if (faceDestination)
        {
            Vector3 lookAtPosition = destination.position;
       
            lookAtPosition.y = target.position.y; 

            target.LookAt(lookAtPosition);
        }
    }
}