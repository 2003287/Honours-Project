using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using GameMovement.Network;
using UnityEngine;
using Unity.Netcode;


public class BaseServer : NetworkManager
{
    
   //varibles for the two game Managers 
    private Manager gameManager;
    private NetworkMovementClass netManager;
    // Start is called before the first frame update
    void Start()
    {
        //redundent code not removed as scared it will break something
        if (Singleton.IsServer)
        {            
            gameManager = FindObjectOfType<Manager>();
            gameManager.GameStart();
          //  netm = FindObjectOfType<NetworkMovementClass>();
          //  netm.HelpMe();
        }
        else
        {          
            
            //force the application to target 60 frames
            if (Application.targetFrameRate != 60)
            {
                Application.targetFrameRate = 60;
            }
           
            //create a host server
            var testing = NetworkManager.Singleton.StartHost();
            //if there is a host server start the level
            if (testing)
            {
                Debug.Log("testing server has started");
                //Access and start the elements in the level
                gameManager = FindObjectOfType<Manager>();
                gameManager.GameStart();
                netManager = FindObjectOfType<NetworkMovementClass>();
                netManager.WakeupVaribles();
            }//write in a bit of code to go back to the main menu

        }

    }
    
}
