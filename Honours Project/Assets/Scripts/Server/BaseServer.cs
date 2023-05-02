using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using GameMovement.Network;
using UnityEngine;
using Unity.Netcode;


public class BaseServer : NetworkManager
{
    //https://www.youtube.com/watch?v=leL6MdkJEaE client prediction
    //server cient reconciliation https://www.youtube.com/watch?v=3SILT4-vZWE 
    //carl Boisvert dev
    // Start is called before the first frame update
    private Manager gm;
    private NetworkMovementClass netm;
    void Start()
    {
        if (Singleton.IsServer)
        {            
            gm = FindObjectOfType<Manager>();
            gm.GameStart();
          //  netm = FindObjectOfType<NetworkMovementClass>();
          //  netm.HelpMe();
        }
        else
        {          
            if (Application.targetFrameRate != 60)
            {
                Application.targetFrameRate = 60;
            }
           
            var testing = NetworkManager.Singleton.StartHost();

            if (testing)
            {
                Debug.Log("testing server has started");
                gm = FindObjectOfType<Manager>();
                gm.GameStart();
                netm = FindObjectOfType<NetworkMovementClass>();
                netm.HelpMe();
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Singleton.IsServer)
        {
           
        }
        
    }
}
