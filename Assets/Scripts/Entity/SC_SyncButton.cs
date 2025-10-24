using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class SC_SyncButton : NetworkBehaviour
{
    [SerializeField] private float threshold = 0.2f;
    [SerializeField] private float debounceTime = 1f;

    private bool _isPressed = false;
    private Vector3 _startPosition;
    private ConfigurableJoint _joint;
    private Transform _movingPart;
    private float _timeOfLastEvent;

    public UnityEvent onPressed, onReleased;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
        }
    }

    private void Start()
    {
        _joint = GetComponentInChildren<ConfigurableJoint>();
        _movingPart = _joint.transform;
        _startPosition = _movingPart.localPosition;
        _timeOfLastEvent = -debounceTime;
    }

    private void Update()
    {
        if (Time.time < _timeOfLastEvent + debounceTime)
        {
            return;
        }

        if (!_isPressed && GetValue() + threshold >= 1)
        {
            Pressed();
        }
        else if (_isPressed && GetValue() - threshold <= 0)
        {
            Released();
        }
    }

    private float GetValue()
    {
        var value = Vector3.Distance(_startPosition, _movingPart.localPosition) / _joint.linearLimit.limit;
        return Mathf.Clamp01(value);
    }

    private void Pressed()
    {
        _isPressed = true;
        _timeOfLastEvent = Time.time;
        onPressed.Invoke();
        Debug.Log("Sync Button Pressed");

        FindObjectOfType<SC_RecButton>().StartLogging();
    }

    private void Released()
    {
        _isPressed = false;
        _timeOfLastEvent = Time.time;
        onReleased.Invoke();
        Debug.Log("Sync Button Released");
    }
}