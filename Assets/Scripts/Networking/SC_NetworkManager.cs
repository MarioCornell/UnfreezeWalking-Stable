using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class SC_NetworkManager : NetworkBehaviour
{
    public SO_NetworkConfig networkConfig;

    public void StartClient()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        
        if (IsValidIP(networkConfig.ServerIP))
        {
            transport.SetConnectionData(networkConfig.ServerIP, 7777);
            NetworkManager.Singleton.StartClient();
        }
        else
        {
            Debug.LogError("Invalid IP address");
            return;
        }
    }
    
    private bool IsValidIP(string ip)
    {
        string[] splitValues = ip.Split('.');
        
        if (splitValues.Length != 4)
        {
            return false;
        }
        
        byte tempForParsing;
        
        return splitValues[0].Length > 0
               && splitValues[1].Length > 0
               && splitValues[2].Length > 0
               && splitValues[3].Length > 0
               && byte.TryParse(splitValues[0], out tempForParsing)
               && byte.TryParse(splitValues[1], out tempForParsing)
               && byte.TryParse(splitValues[2], out tempForParsing)
               && byte.TryParse(splitValues[3], out tempForParsing);
    }
    
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    private void OnApplicationQuit()
    {
        if (IsServer)
        {
            QuitAppClientRpc();
        }
    }
    
    [ClientRpc]
    private void QuitAppClientRpc()
    {
        Application.Quit();
    }
}
