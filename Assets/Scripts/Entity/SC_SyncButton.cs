using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class SC_SyncButton : NetworkBehaviour
{
    [SerializeField] private float debounceTime = 1f;
    [SerializeField] private Animator animator;

    private bool _isPressed = false;

    public UnityEvent onPressed;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
        }
    }

    private void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        Pressed();
        // start the "buttonpress" animation, call it directly
    }

    private void Pressed()
    {
        _isPressed = true;
        onPressed.Invoke();
        Debug.Log("Sync Button Pressed");


        // should invoke the logger event here
    }

    private void Released()
    {
        _isPressed = false;
        Debug.Log("Sync Button Released");
    }
}