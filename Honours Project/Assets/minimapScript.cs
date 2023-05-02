using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapScript : MonoBehaviour
{
    //gamesplusjames (2019) Create a Simple Minimap With No Coding in Unity
    //Available at:https://www.youtube.com/watch?v=a6GquxRUHD0 (Accesed: 6 April 2023)
    //Brackeys (2018) How to make a Minimap in Unity.
    //Available at:https://www.youtube.com/watch?v=28JTTXqMvOUp (Accessed: 6 April 2023) 
    private Player player;


    // Update is called once per frame
    void Update()
    {
        //when there is a player
        if (player != null)
        {
            //get there position on screen
            Vector3 newPos = player.transform.position;         
            //change the Y position to keep it above the player
            newPos.y = transform.position.y;
            //set position and rotation of the camera on screen
            transform.position = newPos;
            transform.rotation = Quaternion.Euler(90f,player.transform.eulerAngles.y,0f);
             
           
        }
        //find teh player in the scene
        else
        {
            player = FindObjectOfType<Player>();
        }
        
    }
}
