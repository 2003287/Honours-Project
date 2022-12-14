using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CahracterBase : MonoBehaviour
{
    //speed the character moves at
    [SerializeField]
    protected float MovementSpeed = 1.0f;

    //the amount of health that the object has
    [SerializeField]
     protected float HealthPoint = 100.0f;

    //fi the character is still alive or not
    [SerializeField]
    bool alive;

    //the speed that there projectile fires at
    [SerializeField]
    protected float ProjectileSpeed = 1.0f;

    //ammunition count for the character
    [SerializeField]
    protected float AmmoCount = 30.0f;

    [SerializeField]
    protected bool reloading;

    //Fired a projectile
    [SerializeField]
    bool FiredProjectile;

    //projectile
    [SerializeField]
    protected GameObject projectile;

    [SerializeField]
    protected float ammotimer;
    //position of the gun
    [SerializeField]
    protected GameObject gunpos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
