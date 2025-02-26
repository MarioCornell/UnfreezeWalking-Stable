using System;
using Unity.Netcode;
using UnityEngine;

public class SC_EntitySyncingManager : NetworkBehaviour
{
    [HideInInspector]public Transform ClientHead;
    [HideInInspector]public Transform ClientLeftHand;
    [HideInInspector]public Transform ClientRightHand;
    
    public Transform HeadRepresentor;
    public Transform LeftHandRepresentor;
    public Transform RightHandRepresentor;
    
    public SyncData MySyncData;

    [Serializable]
    public struct SyncData : INetworkSerializable
    {
        public Vector3 HeadPosition;
        public Quaternion HeadRotation;
        
        public Vector3 LeftHandPosition;
        public Quaternion LeftHandRotation;
        
        public Vector3 RightHandPosition;
        public Quaternion RightHandRotation;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref HeadPosition);
            serializer.SerializeValue(ref HeadRotation);
            serializer.SerializeValue(ref LeftHandPosition);
            serializer.SerializeValue(ref LeftHandRotation);
            serializer.SerializeValue(ref RightHandPosition);
            serializer.SerializeValue(ref RightHandRotation);
        }
    }
    
    private void Update()
    {
        if (!IsServer && AllClientTransformsNotNull())
        {
            MySyncData.HeadPosition = ClientHead.position;
            MySyncData.HeadRotation = ClientHead.rotation;
            
            MySyncData.LeftHandPosition = ClientLeftHand.position;
            MySyncData.LeftHandRotation = ClientLeftHand.rotation;
            
            MySyncData.RightHandPosition = ClientRightHand.position;
            MySyncData.RightHandRotation = ClientRightHand.rotation;
            
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
        
        LeftHandRepresentor.position = syncData.LeftHandPosition;
        LeftHandRepresentor.rotation = syncData.LeftHandRotation;
        
        RightHandRepresentor.position = syncData.RightHandPosition;
        RightHandRepresentor.rotation = syncData.RightHandRotation;
    }

    private void SyncRepresentation()
    {
        HeadRepresentor.position = MySyncData.HeadPosition;
        HeadRepresentor.rotation = MySyncData.HeadRotation;
        
        LeftHandRepresentor.position = MySyncData.LeftHandPosition;
        LeftHandRepresentor.rotation = MySyncData.LeftHandRotation;
        
        RightHandRepresentor.position = MySyncData.RightHandPosition;
        RightHandRepresentor.rotation = MySyncData.RightHandRotation;
    }

    private bool AllClientTransformsNotNull()
    {
        if (ClientHead == null || ClientLeftHand == null || ClientRightHand == null)
        {
            return false;
        }
        else
        {
            return true;
        }
        
    }
}