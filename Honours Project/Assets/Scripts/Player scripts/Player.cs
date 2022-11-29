using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Player : CahracterBase
{
    // Brackleys (2019) FIRST PERSON MOVEMENT in Unity - FPS Controller.
    //Available at: https://www.youtube.com/watch?v=_QajrabyTJc (Accessed: 25 November 2022)

    //the body of the player
    [SerializeField]
    Transform PLayer;
    //the movement speed of the mouse
    [SerializeField]
    float MouseSpeed = 50.0f;

    [SerializeField]
    //character controller
    CharacterController PLayerMovement;

    
    public Text AmmoText;
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
        AmmoText.text = AmmoCount.ToString() + "/ 30";
        reloading = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!reloading)
        {
            if (PLayer)
            {
                //get the movements of the mouse
                float MouseX = Input.GetAxis("MouseX") * MouseSpeed * Time.deltaTime;
                float MouseY = Input.GetAxis("MouseY") * MouseSpeed * Time.deltaTime;
                //get the change in screenposition
                xRotation -= MouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);
                yRotation += MouseX;
                //aplly this to the bodya and camera
                transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
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
                // if the left mouse is clicked spawn a bullet
                if (Input.GetButtonDown("Fire1"))
                {
                    Debug.Log("firing");

                    GameObject bullet = Instantiate(projectile, gunpos.transform.position, transform.rotation);
                    bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * ProjectileSpeed, ForceMode.Impulse);
                    AmmoCount--;
                    AmmoText.text = AmmoCount.ToString() + "/ 30";
                }

            }

            //allow the player to reload
            if (AmmoCount <= 29.0f)
            {
                //when pressed the player will start reloading
                if (Input.GetKeyDown(KeyCode.F))
                {
                    reloading = true;
                    ammotimer = 1.0f;
                    Debug.Log("Reloading");
                }
            }
        }
        else
        {
            ammotimer -= Time.deltaTime;
            //once the timer reaches 0 the player ahs reloaded
            if (ammotimer <= 0.0f)
            {
                AmmoCount = 30.0f;
                reloading = false;
                AmmoText.text = AmmoCount.ToString() + "/ 30";
            }
        }
       
    }
}
