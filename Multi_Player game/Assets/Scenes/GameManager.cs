using UnityEngine;
using Unity.Netcode ;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication ;
using Unity.Services.Relay ;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class GameManager : NetworkBehaviour
{
    // Start is called before the first frame update
      public NetworkVariable<int> currentTurn = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone);
     public static GameManager Instance ;


    private void Awake()
    {
        if(Instance!=null && Instance !=this)
        {

            Destroy(gameObject) ;
        }else
        {

            Instance=this ;
        }

    }
    private async void Start()
    { Screen.SetResolution(1080/2 , 2280/2,FullScreenMode.MaximizedWindow);
        NetworkManager.Singleton.OnClientConnectedCallback+=(clientId)=>{
            Debug.Log(clientId+"Joined");
            if(NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClients.Count==2)
            {
                SpawnBoard() ;
                Debug.Log("Spawn Tic Tac Toe Board") ;
            }

        } ;
        await UnityServices.InitializeAsync() ; 
        await AuthenticationService.Instance.SignInAnonymouslyAsync() ; 

    }
    [SerializeField] private TextMeshProUGUI joinCodeText ; 

   public  async void StartHost()
        {  
            try{
                Allocation allocation= await   RelayService.Instance.CreateAllocationAsync(1) ; // host 1 
                string joinCode=await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId) ;
                joinCodeText.text=joinCode ;
                RelayServerData relayServerData = new RelayServerData(allocation,"dtls") ;
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData) ;
                NetworkManager.Singleton.StartHost();   
            }catch(RelayServiceException e) 
            {

                Debug.Log(e) ;
            }
             
          
        }
[SerializeField] private GameObject BoardPrefab ;
private GameObject newBoard ;
private void  SpawnBoard(){


    newBoard =Instantiate (BoardPrefab) ;
    newBoard.GetComponent<NetworkObject>().Spawn()  ;
}

[SerializeField] private TMP_InputField joinCodeInput ;
    public  async void StartClient()
        {  try{
            
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCodeInput.text);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            }catch(RelayServiceException e)
            {
                Debug.Log(e) ; 
            }
        }

    // Update is called once per frame
    [SerializeField] private GameObject gameEndPanel   ;
    [SerializeField] private TextMeshProUGUI msgText ; 
   public void ShowMsg(string msg)
   {
        if(msg.Equals("won"))
        {

            msgText.text="7chitou" ;
            gameEndPanel.SetActive(true) ;
            showOpponentMsg("ta7chalek") ;
            


        }
        else if (msg.Equals("draw")){


            msgText.text="Game Draw" ; 
            gameEndPanel.SetActive(true) ; 
            showOpponentMsg("Game Draw") ;
        }

   }




   private void showOpponentMsg(string msg){

    if(IsHost)
    {
            // then use ClientRpc to show  Message at Client Side 
              showOpponentClientRpc(msg);


    }else 
    {

        showOpponentServerRpc(msg) ;

    }
   }
   [ClientRpc]
   private void showOpponentClientRpc(string msg)
   {    if(IsHost) return ; 
        msgText.text=msg ; 
        gameEndPanel.SetActive(true) ;

   }

   [ServerRpc(RequireOwnership =false)]
   private void showOpponentServerRpc(string msg)
   {
        msgText.text=msg ; 
        gameEndPanel.SetActive(true) ;  
   }

   public  void Restart(){
    // if this client , then call ServerRpc to destroy current board and create new board 
    if(!IsHost)
        {
            RestartServerRpc();
            gameEndPanel.gameObject.SetActive(false);
            
        }else 
        {
            Destroy(newBoard) ;
            SpawnBoard();
            RestartClientRpc() ;
            gameEndPanel.gameObject.SetActive(false);



        }
    //Destroy the current Game Board 
    //Spawn a new board 
    //Hide the result Panel 



   }
[ServerRpc(RequireOwnership =false)]
private void RestartServerRpc()
    {
        Destroy(newBoard) ;
        SpawnBoard();
        gameEndPanel.gameObject.SetActive(false);



    }

[ClientRpc]
private void RestartClientRpc()
{

    gameEndPanel.gameObject.SetActive(false);
}
}
