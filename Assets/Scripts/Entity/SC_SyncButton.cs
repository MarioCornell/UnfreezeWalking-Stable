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
    private bool canPress = true;
    private SC_Manager sceneManager;

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
        sceneManager = FindObjectOfType<SC_Manager>();
        
        if (sceneManager == null)
        {
            Debug.LogWarning("SC_SyncButton: Could not find SC_Manager in scene. Event data logging will not include scene parameters.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canPress)
        {
            Pressed();
        }
        else
        {
            Debug.Log("Sync Button press ignored - debounce active");
        }
    }

    [ContextMenu("Pressed")]
    public void Pressed()
    {
        _isPressed = true;
        onPressed.Invoke();
        Debug.Log("Sync Button Pressed");
        
        if (animator != null)
        {
            animator.SetTrigger("Press");
        }
        
        LogEventData();
        
        StartCoroutine(DebounceCoroutine());
    }

    private IEnumerator DebounceCoroutine()
    {
        canPress = false;
        yield return new WaitForSeconds(debounceTime);
        canPress = true;
        Debug.Log("Sync Button ready for next press");
    }

    private void LogEventData()
    {
        string sceneName = "Unknown";
        
        if (sceneManager != null && sceneManager.SceneNameInputField != null)
        {
            sceneName = sceneManager.SceneNameInputField.inputText.text;
            if (string.IsNullOrEmpty(sceneName))
            {
                sceneName = "Unnamed";
            }
        }
        
        if (sceneManager == null || sceneManager.CurrentSceneConfig == null)
        {
            DataLogger.LogEvent($"SceneName:{sceneName}|SyncButton_NoConfig");
            return;
        }
        
        SO_SceneConfig config = sceneManager.CurrentSceneConfig;
        Vector3 stepScale = config.StepScale;
        
        string eventData = $"SceneName:{sceneName}|StepScale_X:{stepScale.x:F2}|Y:{stepScale.y:F2}|Z:{stepScale.z:F2}|TrialDistance:{config.TotalDistance:F2}|StrideLength:{config.DistanceBetweenSteps:F2}|DoorWidth:{config.DoorScale:F2}";
        
        DataLogger.LogEvent(eventData);
    }

    private void Released()
    {
        _isPressed = false;
        Debug.Log("Sync Button Released");
    }
}