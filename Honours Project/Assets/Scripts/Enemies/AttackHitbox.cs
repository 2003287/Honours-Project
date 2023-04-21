using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    //make it frame accurate but this is done 
    //testing int
    private int tick = 0;
    private bool attack = false;
    private bool sendHit = false;
    ZombieScript parent;
    // Start is called before the first frame update
    void Start()
    {
        parent = gameObject.GetComponentInParent<ZombieScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (attack)
        {
            if (tick >= 16 && !sendHit)
            {
                Attacking();
            }

            Debug.Log("the enemy is attacking" + tick);
            tick++;
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
