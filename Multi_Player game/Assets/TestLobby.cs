using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Lobbies ;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication ;

public class TestLobby : MonoBehaviour
{   private Lobby joinLobby ;
    private string playerName ;
    private Lobby hostLobby ; 
    private float heartBeatTimer;

    private float lobbyupdateTimer ;
    // Start is called before the first frame update
    private async void  Start()
    {
       await UnityServices.InitializeAsync();
       AuthenticationService.Instance.SignedIn+=()=>{Debug.Log("Sign In"+AuthenticationService.Instance.PlayerId);};
       await AuthenticationService.Instance.SignInAnonymouslyAsync() ; 
       playerName="yassine Fetoui"+UnityEngine.Random.Range(10,100) ;
        
    }
    private void  Update(){
        handleLobbyHeartbeat() ;
        handleLobbyPollForUpdates();
    }

   
    private async void handleLobbyHeartbeat(){
        if(hostLobby!=null){

            heartBeatTimer-= Time.deltaTime ; 

            if (heartBeatTimer<0f)
            {
                float heatBeatTimerMax=15;
                heartBeatTimer=heatBeatTimerMax;
          await   LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id); 
            }
        } 

    }
    private async void handleLobbyPollForUpdates(){
        if(joinLobby!=null){

                    lobbyupdateTimer-= Time.deltaTime ; 

                    if (lobbyupdateTimer<0f)
                    {
                        float lobbyupdateTimerMax=1.1f;
                        lobbyupdateTimer=lobbyupdateTimerMax;
                    Lobby lobby =await LobbyService.Instance.GetLobbyAsync(joinLobby.Id); 
                    joinLobby=lobby ;
                    }
                } 


            }

    // Update is called once per frame
   private async void  CreateLobby(){
   try{ string lobbyName="My Lobby"; 
    int maxPlayer=4 ;
    CreateLobbyOptions createLobbyOptions =new CreateLobbyOptions{

             IsPrivate=false ,

            Player = GetPlayer(),
            Data=new Dictionary<string, DataObject>{
                {"GameMode",new DataObject(DataObject.VisibilityOptions.Private,"CaptureTheFlag" )},
                {"Map",new DataObject(DataObject.VisibilityOptions.Public ,"de_dust2")}
            }
    };
 
Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName,maxPlayer) ;
hostLobby=lobby;
   PrinterPlayers(hostLobby);
Debug.Log("create Lobby!"+lobby.Name+"  "+lobby.MaxPlayers) ;
   }
   catch(LobbyServiceException e )
   {
       Debug.Log(e) ;
   }
   }

private async void ListLobbies(){
   try{
    
    QueryLobbiesOptions queryLobbiesOptions=new QueryLobbiesOptions{
        Count=25,
        Filters=new List<QueryFilter>{
           new QueryFilter( QueryFilter.FieldOptions.AvailableSlots,"0",QueryFilter.OpOptions.GT)
        },
        Order=new List<QueryOrder>{
            new QueryOrder(false ,QueryOrder.FieldOptions.Created) 
        }
    } ;
    
    QueryResponse  queryResponse = await Lobbies.Instance.QueryLobbiesAsync(); 
    Debug.Log("Lobbies found: "+ queryResponse.Results.Count);
    foreach(Lobby lobby in queryResponse.Results)
    {
        Debug.Log(lobby.Name+" "+lobby.MaxPlayers) ;
    }
    }
   catch(LobbyServiceException e )
   {
       Debug.Log(e) ;
   }
}
private async void joinLobbyByLobbyCode(string lobbyCode){
  try
    { JoinLobbyByCodeOptions joinLobbyByCodeOptions =new JoinLobbyByCodeOptions{
         Player=GetPlayer() 


    } ;
     // QueryResponse  queryResponse = await Lobbies.Instance.QueryLobbiesAsync(); 
      Lobby joinedLobby =await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode ,joinLobbyByCodeOptions);
      Debug.Log("Joined Lobby with  code: "+lobbyCode) ;
      PrinterPlayers(joinedLobby) ;
    }
   catch(LobbyServiceException e )
   {
       Debug.Log(e) ;
   }
}
private Player GetPlayer()
{return  new Player{

Data=new Dictionary<string,PlayerDataObject>{


    {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)} 
}
} ;


}

   private async void QuickJoinedLobby(){

   await  LobbyService.Instance.QuickJoinLobbyAsync(); 
   }


   private void PrinterPlayers(Lobby lobby){


    Debug.Log("Player in Lobby "+lobby.Name+" "+lobby.Data["GameMode"].Value+" "+lobby.Data["Map"].Value);
    foreach(Player player in lobby.Players)
    {
        Debug.Log(player.Id +" "+player.Data["PlayerName"].Value) ;
    }
   }
   private async void updateLobbyGameMode(string gameMode)
   {
    try{

        hostLobby=await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id , new UpdateLobbyOptions{


            Data =new Dictionary<string, DataObject>{{
                "Gamemode",new DataObject(DataObject.VisibilityOptions.Public,gameMode)}


            }
        }) ;
        joinLobby=hostLobby ;
        PrinterPlayers(hostLobby) ;
    
   } catch(LobbyServiceException e )
   {
       Debug.Log(e) ;
   }

}
private  async void UpdatePlyerName(string newPlayerName)
{
    try{
newPlayerName=playerName ;

await LobbyService.Instance.UpdatePlayerAsync(joinLobby.Id , AuthenticationService.Instance.PlayerId, new  UpdatePlayerOptions{
    Data= new Dictionary<string, PlayerDataObject>{
        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member ,newPlayerName )}
    }
}) ;

 
   } catch(LobbyServiceException e )
   {
       Debug.Log(e) ;
   }

}
private async void leaveLobby(){

    try{
    await LobbyService.Instance.RemovePlayerAsync(joinLobby.Id,AuthenticationService.Instance.PlayerId)   ;
    
     
   } catch(LobbyServiceException e )
   {
       Debug.Log(e) ;
   }




}
private async void kickPlayer(){


    try{

        await LobbyService.Instance.RemovePlayerAsync(joinLobby.Id,joinLobby.Players[1].Id)   ;
     
    }  
   catch(LobbyServiceException e )
   {
       Debug.Log(e) ;
   }
}
private async void MigrateLobbyHost(){

    try{


        hostLobby=await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id , new UpdateLobbyOptions{
           HostId =joinLobby.Players[1].Id 
           
        }) ;
        joinLobby=hostLobby ;
        PrinterPlayers(hostLobby);

     
    }catch(LobbyServiceException e )
   {
       Debug.Log(e) ;
   }
}
private async void deleteLobby(){
try{
    await LobbyService.Instance.DeleteLobbyAsync(joinLobby.Id)  ;
}
catch(LobbyServiceException e )
   {
       Debug.Log(e) ;
   }
}
}