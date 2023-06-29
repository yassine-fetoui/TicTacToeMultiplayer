using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode ;  
using UnityEngine.UI ; 


public class BoardManager : NetworkBehaviour
{
    // Start is called before the first frame update
  

    Button[,] buttons = new Button[3,3] ;
  
    public override void  OnNetworkSpawn()
    { 
       var cells = GetComponentsInChildren<Button>();
       int n= 0;
        for(int i=0 ; i<3;i++)
            for(int j=0 ; j<3;j++)
            {
                buttons[i,j]=cells[n];
                n++;
                int colomn=i ;
                int row = j ;
                buttons[i,j].onClick.AddListener(delegate{

                    onClickCells(colomn,row);

                });


            }
        
    }
    [SerializeField] private Sprite XSprite ,OSprite ;
private void onClickCells(int row , int colomn){
     // if the button clicked by host, then change button sprite as X
     if(NetworkManager.Singleton.IsHost &&  GameManager.Instance.currentTurn.Value==0)
     {

            buttons[row,colomn].GetComponent<Image>().sprite = XSprite;
            buttons[row,colomn].interactable = false;
            // Also change on Client side
            changeSpriteClientRpc(row,colomn) ;
            checkResult(row,colomn);
            GameManager.Instance.currentTurn.Value = 1;

     }

    // if the button clicked by client , then change button sprite as O
     if(!NetworkManager.Singleton.IsHost && GameManager.Instance.currentTurn.Value==1)
     {

            buttons[row,colomn].GetComponent<Image>().sprite = OSprite;
            buttons[row,colomn].interactable = false;
            checkResult(row,colomn);
            // Also change on host side
            changeSpriteServerRpc(row,colomn);


    
       


     }

    

}
[ClientRpc]
private void changeSpriteClientRpc(int row ,int colomn)
{


        buttons[row,colomn].GetComponent<Image>().sprite=XSprite ;
        buttons[row, colomn].interactable = false;



}
[ServerRpc(RequireOwnership =false)]
private void changeSpriteServerRpc(int row ,int colomn)
    {

        buttons[row, colomn].GetComponent<Image>().sprite = OSprite;
        buttons[row, colomn].interactable = false;
        GameManager.Instance.currentTurn.Value = 0;



    }   
private void checkResult(int r ,int c){
    if(IsWon(r,c))
    GameManager.Instance.ShowMsg("won");
    else 
    {
        if(IsGameDraw())
        {
 GameManager.Instance.ShowMsg("draw");

        }

    }

    
   }
 
    public bool IsWon(int r, int c)
    {
        Sprite clickedButtonSprite = buttons[r, c].GetComponent<Image>().sprite;
        // Checking Column
        if (buttons[0, c].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[1, c].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[2, c].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        // Checking Row

        else if (buttons[r, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[r, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[r, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        // Checking First Diagonal

        else if (buttons[0, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[1, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[2, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        // Checking 2nd Diagonal
        else if (buttons[0, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        buttons[1, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        buttons[2, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        return false;
    }


    private bool IsGameDraw()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (buttons[i, j].GetComponent<Image>().sprite != XSprite &&
                    buttons[i, j].GetComponent<Image>().sprite != OSprite)
                {
                    return false;
                }
            }
        }
        return true;
    }
   
   
}
