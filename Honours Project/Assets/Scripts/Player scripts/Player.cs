using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : CahracterBase
{
    //the body of the player
    [SerializeField]
    Transform PLayer;
    //the movement speed of the mouse
    [SerializeField]
    float MouseSpeed = 50.0f;

    //rotation varibles
    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (PLayer)
        {
            float MouseX = Input.GetAxis("MouseX") * MouseSpeed *Time.deltaTime;
            float MouseY = Input.GetAxis("MouseY") * MouseSpeed * Time.deltaTime;
            //get the change in screenposition
            xRotation -= MouseY;
            xRotation = Mathf.Clamp(xRotation,-90f, 90f);
            yRotation += MouseX;
            //aplly this to the bodya and camera
            transform.localRotation = Quaternion.Euler(xRotation,yRotation,0f);
            
        }
    }
}
