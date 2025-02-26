using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SC_IPHandler : MonoBehaviour
{
    public SO_NetworkConfig networkConfig;
    
    TMP_Text ipText;
    
    private void Start()
    {
        ipText = GetComponent<TMP_Text>();
        
        ipText.text = networkConfig.ServerIP;
    }
    
    public void AddChar(string c)
    {
        if (networkConfig.ServerIP.Length < 15)
        {
            networkConfig.ServerIP += c;
            ipText.text = networkConfig.ServerIP;
        }
    }
    
    public void RemoveChar()
    {
        if (networkConfig.ServerIP.Length > 0)
        {
            networkConfig.ServerIP = networkConfig.ServerIP.Substring(0, networkConfig.ServerIP.Length - 1);
            ipText.text = networkConfig.ServerIP;
        }
    }
    
    public void Submit()
    {
        Application.Quit();
    }
}
