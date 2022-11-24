using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CahracterBase : MonoBehaviour
{
    //speed the character moves at
    [SerializeField]
    float MovementSpeed = 1.0f;

    //the amount of health that the object has
    [SerializeField]
    float HealthPoint = 100.0f;

    //fi the character is still alive or not
    [SerializeField]
    bool alive = true;

    //the speed that there projectile fires at
    [SerializeField]
    float ProjectileSpeed = 1.0f;

    //ammunition count for the character
    [SerializeField]
    float AmmoCount = 30.0f;

    //Fired a projectile
    [SerializeField]
    bool FiredProjectile = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
