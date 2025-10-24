using System;
using Unity.Netcode;
using UnityEngine;

public class SC_EntitySyncingManager : NetworkBehaviour
{
    [HideInInspector]public Transform ClientHead;
    [HideInInspector]public Transform ClientLeftController;
    [HideInInspector]public Transform ClientRightController;
    
    public Transform HeadRepresentor;
    public Transform LeftControllerRepresentor;
    public Transform RightControllerRepresentor;
    
    public SyncData MySyncData;

    [Serializable]
    public struct SyncData : INetworkSerializable
    {
        public Vector3 HeadPosition;
        public Quaternion HeadRotation;
        
        public Vector3 LeftControllerPosition;
        public Quaternion LeftControllerRotation;
        
        public Vector3 RightControllerPosition;
        public Quaternion RightControllerRotation;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref HeadPosition);
            serializer.SerializeValue(ref HeadRotation);
            serializer.SerializeValue(ref LeftControllerPosition);
            serializer.SerializeValue(ref LeftControllerRotation);
            serializer.SerializeValue(ref RightControllerPosition);
            serializer.SerializeValue(ref RightControllerRotation);
        }
    }
    
    private void Update()
    {
        if (!IsServer && AllClientTransformsNotNull() && NetworkManager.Singleton.IsConnectedClient)
        {
            MySyncData.HeadPosition = ClientHead.position;
            MySyncData.HeadRotation = ClientHead.rotation;
            
            MySyncData.LeftControllerPosition = ClientLeftController.position;
            MySyncData.LeftControllerRotation = ClientLeftController.rotation;
            
            MySyncData.RightControllerPosition = ClientRightController.position;
            MySyncData.RightControllerRotation = ClientRightController.rotation;
            
            SyncEntitiesServerRPC(MySyncData);
            SyncRepresentation();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncEntitiesServerRPC(SyncData syncData)
    {
        MySyncData = syncData;
        
        HeadRepresentor.position = syncData.HeadPosition;
        HeadRepresentor.rotation = syncData.HeadRotation;
        
        LeftControllerRepresentor.position = syncData.LeftControllerPosition;
        LeftControllerRepresentor.rotation = syncData.LeftControllerRotation;
        
        RightControllerRepresentor.position = syncData.RightControllerPosition;
        RightControllerRepresentor.rotation = syncData.RightControllerRotation;
    }

    private void SyncRepresentation()
    {
        HeadRepresentor.position = MySyncData.HeadPosition;
        HeadRepresentor.rotation = MySyncData.HeadRotation;
        
        LeftControllerRepresentor.position = MySyncData.LeftControllerPosition;
        LeftControllerRepresentor.rotation = MySyncData.LeftControllerRotation;
        
        RightControllerRepresentor.position = MySyncData.RightControllerPosition;
        RightControllerRepresentor.rotation = MySyncData.RightControllerRotation;
    }

    private bool AllClientTransformsNotNull()
    {
        if (ClientHead == null || ClientLeftController == null || ClientRightController == null)
        {
            return false;
        }
        else
        {
            return true;
        }
        
    }
}