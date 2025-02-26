using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SC_StartupManager : NetworkBehaviour
{
    private SC_NetworkManager networkManager;

    public bool TestAsServer;
    public bool TestAsClient;

    public List<GameObject> SpawnForServer;
    public List<GameObject> SpawnForClient;
    
    public List<GameObject> DeleteForServer;
    public List<GameObject> DeleteForClient;
    
    public List<Renderer> DisableRenderersForServer;
    public List<Renderer> DisableRenderersForClient;
    
    
    public void Start()
    {
        networkManager = GetComponent<SC_NetworkManager>();

        if (TestAsServer)
        {
            StartServer();
        }
        else if (TestAsClient)
        {
            StartClient();
        }
        else
        {
            if (Application.platform == RuntimePlatform.VisionOS
                || Application.platform == RuntimePlatform.Android)
            {
                StartClient();
            }
            else
            {
                StartServer();
            }
        }
    }
    
    private void StartClient()
    {
        foreach (var obj in SpawnForClient)
        {
            Instantiate(obj);
        }
        
        foreach (var obj in DeleteForClient)
        {
            Destroy(obj);
        }
        
        foreach (var renderer in DisableRenderersForClient)
        {
            renderer.enabled = false;
        }
        
        networkManager.StartClient();
    }
    
    private void StartServer()
    {
        foreach (var obj in SpawnForServer)
        {
            Instantiate(obj);
        }
        
        foreach (var obj in DeleteForServer)
        {
            Destroy(obj);
        }
        
        foreach (var renderer in DisableRenderersForServer)
        {
            renderer.enabled = false;
        }
        
        networkManager.StartServer();
    }
    
}
