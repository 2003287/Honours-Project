using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
 
    //number of ticks the collision box is touching the player
    private int tick = 0;
    //bools to update collision
    private bool attack = false;
    private bool sendHit = false;
    //gather the parent of the object
    ZombieScript parent;
    
    void Start()
    {
        //create the parent for the collision box
        parent = gameObject.GetComponentInParent<ZombieScript>();
    }

    // Update is called once per frame
    void Update()
    {
        //while colliding update teh tick
        if (attack)
        {
            //after 16 frames update the parent so the player has been hit
            if (tick >= 16 && !sendHit)
            {
                Attacking();
            }

            
            tick++;
            //after 24 frames update to signify the player can be hit again
            //done due to error that froze the enemy after Rollback  
            if (tick >= 24)
            {
                sendHit = false;
                attack = false;
                tick = 0;
                if (parent != null)
                {
                    parent.getCouldAttack = false;
                }
            }
        }
    }

    //when the player enters the collision box start the check for collision
    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            if (!attack)
            {
                if (other.gameObject.tag == "Player")
                {
                    SetAttacking();
                    attack = true;
                }
            }
            
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
           //check the player is still colliding with the enemy
            if (!attack)
            {
                if (parent != null)
                {
                    //if the enemy can attack, attack the player
                    if (parent.GetEnemyState == GameMovement.Network.Enemystate.Running)
                    {
                        SetAttacking();
                        attack = true;
                    }
                }                
            }           
        }
    }
    //when the player exits the collision box reset collision varibles
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            sendHit = false;
            attack = false;
            tick = 0;
            if (parent != null)
            {
                parent.getCouldAttack = false;
            }
        }
    }

    //update the parent as they might be able to attack
    private void SetAttacking()
    {       
        if (parent != null)
        {
            if (!parent.getCouldAttack)
            {
                parent.getCouldAttack = true;
            }
        }
    }
    //update the parent as they are attacking the player
    private void Attacking()
    {       
        if (parent != null)
        {           
            parent.Attacking();
            sendHit = true;
            parent.getCouldAttack = false;
        }
    }
}
