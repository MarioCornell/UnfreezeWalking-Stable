using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SC_ServerAlert : MonoBehaviour
{
    private AudioSource audioSource;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;
        SC_Step step = obj.GetComponent<SC_Step>();
        SC_StepReference reference= obj.GetComponent<SC_StepReference>();
        if (step != null || reference != null)
        {
            Debug.Log("Server Alert: " + obj.name);
            audioSource.Play();
            step.HighlightInternal();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        GameObject obj = other.gameObject;
        SC_Step step = obj.GetComponent<SC_Step>();
        SC_StepReference reference= obj.GetComponent<SC_StepReference>();
        if (step != null || reference != null)
        {
            Debug.Log("Server Alert: " + obj.name);
            step.UnhighlightInternal();
        }
    }
}
