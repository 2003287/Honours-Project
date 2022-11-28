using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : CahracterBase
{
    //https://www.youtube.com/watch?v=_QajrabyTJc brackleys video that i sourced the code from
    //the body of the player
    [SerializeField]
    Transform PLayer;
    //the movement speed of the mouse
    [SerializeField]
    float MouseSpeed = 50.0f;

    [SerializeField]
    //character controller
    CharacterController PLayerMovement;


    //gravity and jump varible
    [SerializeField]
    Vector3 velocity;

    //rotation varibles
    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        //locks cursor will be used in final but not currently
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        if (PLayer)
        {
            //get the movements of the mouse
            float MouseX = Input.GetAxis("MouseX") * MouseSpeed *Time.deltaTime;
            float MouseY = Input.GetAxis("MouseY") * MouseSpeed * Time.deltaTime;
            //get the change in screenposition
            xRotation -= MouseY;
            xRotation = Mathf.Clamp(xRotation,-90f, 90f);
            yRotation += MouseX;
            //aplly this to the bodya and camera
            transform.localRotation = Quaternion.Euler(xRotation,yRotation,0f);            
        }

        if (PLayerMovement)
        {
            //get the inputs of the keyboard for now
            float xMove = Input.GetAxis("Horizontal");
            float yMove = Input.GetAxis("Vertical");

            Vector3 movement = transform.right * xMove + transform.forward * yMove;

            PLayerMovement.Move(movement * MovementSpeed * Time.deltaTime);

            //apply gravity to the controller
            velocity.y += -9.81f * Time.deltaTime;
            PLayerMovement.Move(velocity * Time.deltaTime);
        }

        if (AmmoCount >= 1.0f)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Debug.Log("firing");
               
                GameObject bullet = Instantiate(projectile, gunpos.transform.position, transform.rotation);
                bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * ProjectileSpeed, ForceMode.Impulse);
                AmmoCount--;

            }

            if (Input.GetButton("Fire1"))
            {
                Debug.Log("held");
            }
        }
    }
}
