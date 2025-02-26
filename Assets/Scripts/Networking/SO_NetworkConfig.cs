using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SSSoftware.Attributes.Mutable]
[CreateAssetMenu(fileName = "NetworkConfig", menuName = "NetworkConfig")]
public class SO_NetworkConfig : SSSoftware.SOPro.ScriptableObject
{
    public string ServerIP = "127.0.0.1";
}
