using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using FixMath.NET;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;
using GameMovement.Network;

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

    //start position
    [SerializeField]
    Vector3 spawnPosition;

    [SerializeField]
    //character controller
    CharacterController PLayerMovement;

    //networkcomponent
    [SerializeField]
    private NetworkMovementClass netMove;
   


    //health timer
    private float healthtimer =0;
    //gravity and jump varible
    [SerializeField]
    Vector3 velocity;

    //player position states
    Playpostest oldPlayerPosState;
    Playpostest newPlayerPosState;
   
    List<Playpostest> Playerpositions;
    //camera
    new private Camera camera;
    

    //Ammo Count
    [SerializeField]
    float ammoMax = 12.0f;  

    //jumping varibles
    private bool isGrounded = false;

    private bool jumped = false;


    //private jump timer due to bug that i cannot replicate, no idea what happened or why just adding this in incase
    private float touchGroundTimer = 0.0f;
    //ground position
    [SerializeField]
    Transform groundPos;
    [SerializeField]
    LayerMask groundLayer;

    //latency
    int frameDelay = 0;
    //healthbarscript
    private HealthbarScript healthbar;
    //ammoscript
    private TMP_Text ammotext;
    //tickrate /// for id of each msg 
    TickSystem ticks;  
  
    private bool reloadbool = false;

    //states for input
    PlayerData oldInputState;
    PlayerData newInputState;

    //artifical delay
    private bool artdelay;
    private float delayTimer = 0.0f;
    private int delayCount = 0;
    public bool ArtificalDelay { set { artdelay = value; } }
 
    //states for input and packetloss 
    List<PlayerData> inputStates;
    [SerializeField]
    public int framesReload = 3;
    private MovementState playerState;
    bool randomPacketLoss = false;

    //invincibility frames
    private bool noDamage;
    public bool GetDamage => noDamage;
    private int damageTick = 0;

    //check the player is alive
    public bool GetAlive => alive;

    public float GetHeath => HealthPoint;
    //get frame delay
    public int GetFrameDelay { get { return frameDelay; } set { frameDelay = value; } }
    //player spawning in the scene
    public override void OnNetworkSpawn()
    {
        alive = true;
        //locks cursor will be used in final but not currently
        Cursor.lockState = CursorLockMode.Locked;
        Playerpositions = new List<Playpostest>();

        reloading = false;
        //camera setting
        camera = GameObject.FindGameObjectWithTag("Camera").GetComponent<Camera>();
        if (camera)
        {
            camera.transform.SetParent(transform);
            camera.transform.localPosition = Vector3.zero;
            camera.transform.localRotation = Quaternion.identity;           
        }
        //player state and Input state
        playerState = MovementState.Moving;
        inputStates = new List<PlayerData>();

        //health varibles and ammo varibles
        healthbar = FindObjectOfType<HealthbarScript>();
        healthbar.HealthUpdate(HealthPoint);
        AmmoCount = ammoMax;
        var gm = GameObject.FindGameObjectWithTag("Ammo");
        ammotext = gm.GetComponent<TMP_Text>();
        ammotext.text = "Ammo Count: " + AmmoCount.ToString()+ "/"+ ammoMax.ToString();

        //reset position at the start of the level
        PLayerMovement.enabled = false;
        PLayerMovement.transform.position = spawnPosition;
        PLayerMovement.enabled = true;

        ticks = new TickSystem(0,0);
    }
    // Update is called once per frame
    void Update()
    {
        //when can move
        if (PLayerMovement)
        {
            ticks.tickDeltaTime += Time.deltaTime;

           //if the new frame has occured
            while (ticks.tickDeltaTime > ticks.tickRate)
            {
                //while not reloading
                if (!reloading)
                {
                    //check there is a player and get rotation
                    if (PLayer)
                    {
                        PlayerRotationVoid();

                    }

                    //move the player
                    PlayerMovementVoid();


                    //update the player if a bullet should spawn
                    if (AmmoCount >= 1.0f)
                    {
                        // if the left mouse is clicked spawn a bullet
                        if (Input.GetButtonDown("Fire1"))
                        {                          
                            playerState = MovementState.Attacking;
                        }

                    }
                    //when the there is a delay method
                    DelayMethod();
                    
                    //inviciblity frames 40 frames of invinciblity
                    if(noDamage)
                    {
                        var ts = damageTick + 40;
                        if (ticks.tick > ts)
                        {
                            noDamage = false;
                        }
                    }

                    //if the player is not jumping check if they are on the ground
                    if (!jumped)
                    {
                        GravityFunction();
                    }
                    else
                    {
                        //the player is now jumping
                        playerState = MovementState.jumping;
                        newPlayerPosState._grounded = false;
                    }

                    //allow the player to reload
                    if (AmmoCount < ammoMax)
                    {
                        //when pressed the player will start reloading
                        if (Input.GetButtonDown("Fire2"))
                        {
                            RStartReloading();
                        }
                    }
                }
                else
                {
                    ammotimer -= Time.deltaTime;
                    //once the timer reaches 0 the player has reloaded
                    if (ammotimer <= 0.0f)
                    {
                        //allow the player to move again
                        reloading = false;
                        playerState = MovementState.Moving;
                        ammotext.text = "Ammo Count: " + AmmoCount.ToString() + "/" + ammoMax.ToString();
                        reloadbool = false;
                    }
                    //the player is realing
                    if (!reloadbool)
                    {
                        Reloading();
                        reloadbool = true;

                    }

                    //update the tick for the state
                    newInputState._id = ticks.tick;
                }

                //check if the player needs to heal or if they are dead
                HealthMethod();

                //update ticks
                ticks.tickDeltaTime -= ticks.tickRate;
                ticks.tick++;
                
                //save the state and save to the server
                CreateSavestates();        

            }

        }

    }

    //check if the player needs to heal
    private void HealthMethod()
    {
        //heal the player if hurt
        if (HealthPoint < 100.0f)
        {
            HealtingPLayer();
        }

        //the player has died
        if (HealthPoint <= 0)
        {
            alive = false;
        }
    }

    //delays the player input if the artifical delay is set
    private void DelayMethod()
    {
        if (artdelay && !randomPacketLoss)
        {
            delayTimer += Time.deltaTime;
            //once the timer is complete create a random amount of packets losts
            if (delayTimer >= 2.0f)
            {
                randomPacketLoss = true;
                int t = Random.Range(8, 23);
                delayCount = ticks.tick + t;
            }
        }
        //stop packet loss once the number of loss is complete
        if (randomPacketLoss)
        {           
            if (ticks.tick >= delayCount)
            {
                randomPacketLoss = false;
                delayTimer = 0;
            }
        }
    }

    //the player has started reloading
    private void RStartReloading()
    {
        reloading = true;
        ammotimer = 1.0f;
        AmmoCount = ammoMax;
        playerState = MovementState.Reloading;       
        newInputState._playerstate = MovementState.Reloading;
        newInputState._direction = Direction.Idle;
        newInputState._position = new FixedVec2(0,0);
    }

    //save the player state
    protected void CreateSavestates()
    {
        //position to save
        FixedVec3 t = new FixedVec3(transform.position.x, transform.position.y, transform.position.z);
        newPlayerPosState._playerpos = t;
        //save the playerposition state and input state
        Playerpositions.Add(newPlayerPosState);
        inputStates.Add(newInputState);        
        //send to the server once inital delay has happened
        if (Playerpositions.Count >= frameDelay)
        {
            if (!randomPacketLoss)
            {
                netMove.ProcessPlayerMoevement(oldInputState, oldPlayerPosState);
            }
            
            //remove teh earliest element
            Playerpositions.RemoveAt(0);
            inputStates.RemoveAt(0);          
        }        
        //set the last state to the earliest element for both position and input states
        oldPlayerPosState = Playerpositions[0];
        oldInputState = inputStates[0];
    }
    //void for reloading
    protected void Reloading()
    {
        var t = new FixedVec2(0, 0);
        var d = Direction.Idle;        
        //update the state sent to the server
        oldInputState._position = t;
        oldInputState._direction = d;
        oldInputState._playerstate = MovementState.Reloading;        
        oldInputState._direction = d;
        for (int i = 0; i < inputStates.Count; i++)
        {
            PlayerData ts = inputStates[i];
           
            ts._playerstate = MovementState.Reloading;            
            ts._direction = d;
            ts._position = t;
            inputStates[i] = ts;
        }
        //get teh first saved element
        oldInputState = inputStates[0];
        
        // used due to rotation happening during reloading
        Input.ClearLastPenContactEvent();
        oldPlayerPosState._rotation = new FixedVec2(0,0);      
       
    }

    public void StartReloading()
    {
        reloading = true;
    }
    //the player has jumped in the air
    public void FixJumping()
    {
        playerState = MovementState.Moving;
        jumped = false;
       
    }

    //void for player movement 
protected void PlayerMovementVoid() 
    {
        //get the inputs of the keyboard for now
        float xMove = Input.GetAxis("Horizontal");
        float yMove = Input.GetAxis("Vertical");

        //create the direction
        var d = netMove.ProcessPlayerDirection(xMove,yMove);
        var check = NetworkHelper.Vec2Creation(d);    
       
        //create a state to save
        PlayerData help = new PlayerData()
        {
            _position = check,
            _direction = d,           
            _id = ticks.tick,
            _playerstate = playerState,           
        };

        newInputState = help;
    }

    //gather the rotation of the player and save it
    protected void PlayerRotationVoid()
    {
        //get the movements of the mouse
        float MouseX = Input.GetAxis("MouseX") * MouseSpeed * Time.deltaTime;
        float MouseY = Input.GetAxis("MouseY") * MouseSpeed * Time.deltaTime;     
        newPlayerPosState._rotation = new FixedVec2(MouseX,MouseY);     
    }

    //spawn in a projectile
    public GameObject ProjectileSpawn()
    {
        //spawn the bullet
        GameObject bullet = Instantiate(projectile, gunpos.transform.position, transform.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * ProjectileSpeed, ForceMode.Impulse);        
        //reset the players state
        playerState = MovementState.Moving;
        oldInputState._playerstate = MovementState.Moving;        
        for(int i = 0; i<inputStates.Count;i++)
        {
            if (inputStates[i]._playerstate == MovementState.Attacking)
            {
                var ts = inputStates[i];
                ts._playerstate = MovementState.Moving;
                inputStates[i] = ts;
            }
        }
        //update ammo
        AmmoCount--;
        ammotext.text = "Ammo Count: " + AmmoCount.ToString() + "/" + ammoMax.ToString();
        netMove.CanFireAgain();
        return bullet;
    }

    //damage the player when hurt
    public void PlayerDamaged()
    {
        // if the player can be hurt, hurt the player
        if (!noDamage)
        {
            HealthPoint -= 20.0f;
            healthbar.HealthUpdate(HealthPoint);
        }
        

    }

    //reset the players postion and varibles after death
    public void PlayerReset()
    {
        //update health
        HealthPoint = 100.0f;
        healthbar.HealthUpdate(HealthPoint);
        healthtimer = 0;
        //reset ammo
        AmmoCount = ammoMax;
        ammotext.text = "Ammo Count: " + AmmoCount.ToString() + "/" + ammoMax.ToString();
        ammotimer = 0;
        //fix the state and the ticks
        playerState = MovementState.Moving;
        alive = true;
        noDamage = true;
        damageTick = ticks.tick;
    }

    //heal the player if they are hurt
    private void HealtingPLayer()
    {
        healthtimer += ticks.tickRate;
        
        //timer check
        if(healthtimer >= 2.0f) 
        {
            //update the players health and the ui element
            HealthPoint += 20.0f;
            healthbar.HealthUpdate(HealthPoint);
            healthtimer = 0;
        }
    }
    private void GravityFunction()
    {
        //check if the player is on the ground before collisions
        isGrounded = Physics.CheckSphere(groundPos.position,0.4f,groundLayer);
        //allow the player to jump again
        if (isGrounded && playerState != MovementState.jumping)
        {
            newPlayerPosState._grounded = true;
            touchGroundTimer = 0;
        }
        else
        {
            //the player is not grounded
            newPlayerPosState._grounded =  false;
            touchGroundTimer += ticks.tickDeltaTime;
            //check the player is not on the ground for a long duration
            if (touchGroundTimer >= 8.0f)
            {
                PLayerMovement.enabled = false;
                PLayerMovement.transform.position = spawnPosition;
                PLayerMovement.enabled = true;
                touchGroundTimer = 0;
            }

        }
        
        //when jumping change the state to jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            
             jumped = true;
        }
     
    }
   
}

