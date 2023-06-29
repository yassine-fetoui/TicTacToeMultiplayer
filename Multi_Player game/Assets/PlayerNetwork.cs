using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

[System.Diagnostics.DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private Transform  spawnObjectPrefab ;
    [SerializeField] private Transform  spawnObjectTransform;
    private NetworkVariable<MyCustomData> m_NetworkVariable = new NetworkVariable<MyCustomData>(
        new MyCustomData{
            _int=50,
            _bool=true 

        }
        
        
        ,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner
    ); 
    // Start is called before the first frame update
    // Update is called once per frame
   public struct MyCustomData :INetworkSerializable{
     public int _int ; 
     public bool _bool ;
     public FixedString128Bytes  message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool) ;
            serializer.SerializeValue(ref message) ;        }
    } 
    private void Start(){


    }
    public override void OnNetworkSpawn()   
    {
        m_NetworkVariable.OnValueChanged +=(MyCustomData previousValue , MyCustomData newValue )=>{
             Debug.Log(OwnerClientId + "random number" + newValue._int+";"+newValue._bool+";"+newValue.message);
        };
    }
    void Update()
    {
       
        if (!IsOwner) return;
        if (Input.GetKeyUp(KeyCode.T))
        { //TestServerRpc(new ServerRpcParams()) ;
          //  m_NetworkVariable.Value=new MyCustomData{_bool=false , _int=10 ,message="All your base are belong to us!" } ; 
          //TestClientRpc(new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>{1}} });  
           spawnObjectTransform= Instantiate(spawnObjectPrefab) ;
           spawnObjectTransform.GetComponent<NetworkObject>().Spawn(true);
        }
        Vector3 moveDir= new Vector3(0,0,0);    
        if(Input.GetKey(KeyCode.Z)) moveDir.z =+1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.Q)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;
        float speedMove = 3f; 
        transform.position += moveDir*speedMove*Time.deltaTime;
        if(Input.GetKeyDown(KeyCode.Y)){

            Destroy(spawnObjectTransform.gameObject) ;
        }

    }


    [ServerRpc]
    private void TestServerRpc(ServerRpcParams serverRpcParams){

Debug.Log($"<color=green> hay my name is haha</color><br> <color=orange>"+serverRpcParams.Receive.SenderClientId+ "</color>") ;
    }
    [ClientRpc]
   public void  TestClientRpc(ClientRpcParams   clientRpcParams){
    Debug.Log("TestClientRpc ") ;
   }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}
