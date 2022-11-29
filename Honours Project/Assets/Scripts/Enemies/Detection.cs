using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detection : MonoBehaviour
{
    private bool touching;
    // Start is called before the first frame update
    void Start()
    {
        touching = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //the player has collided with the object
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player")
        {
            touching = true;
        }
    }
    //if the player leaves stop touching
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Player")
        {
            touching = false;
        }
    }
    //allow other scripts to get touches
    public bool GetTouch()
    {
        return touching;
    }
}
