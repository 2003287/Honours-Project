using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : CahracterBase
{
    //enum for the state of the enemy
    protected enum Enemystate { Walking, Following, Attack, Healing, Stationery };
    // the varible used by the enemy
    protected Enemystate m_Enemystate;
    [SerializeField]
    protected GameObject sightobject;

    [SerializeField]
    protected GameObject attackObject;

    protected bool walking;
    protected int currentPosInt;
    //switch statement to controll each state
    protected void EnemySwitchstate()
    {
        switch (m_Enemystate)
        {
            case Enemystate.Walking:
                Walkingstate();
                break;
            case Enemystate.Following:
                Followingstate();
                Debug.Log("followstate");
                break;
            case Enemystate.Attack:
                Attackstate();
                break;
            case Enemystate.Healing:

                break;
            case Enemystate.Stationery:

                break;
        }

    }

    //transistions the state of the enemy when called
    protected void SwitchEnemyState(Enemystate enemy)
    {
        if (m_Enemystate != enemy)
        {
            m_Enemystate = enemy;
        }
        else
        {
            Debug.Log("this should not happen");
            Debug.Log(m_Enemystate);
        }
    }

    //walingstate behaviour
    public virtual void Walkingstate()
    {
        Debug.Log("testing this works");
    }

    //followingstate behaviour
    public virtual void Followingstate()
    {
        Debug.Log("testing this works");
    }

    //attack state behaviour
    public virtual void Attackstate()
    {
        Debug.Log("testing this works");
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
