using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using FixMath.NET;
using UnityEngine;
using Unity.Netcode;
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

    [SerializeField]
    //character controller
    CharacterController PLayerMovement;

    //networkcomponent
    [SerializeField]
    private NetworkMovementClass netMove;
 

   
    public GameObject Text;
    

    //health timer
    private float healthtimer =0;
    //gravity and jump varible
    [SerializeField]
    Vector3 velocity;


    Playpostest playerPosTesting;
    Playpostest playerPosTesting2;
   
    List<Playpostest> Playerpositions;
    //camera
    new private Camera camera;
    

    //Ammo Count
    [SerializeField]
    float ammoMax = 30.0f;  

    private bool isGrounded = false;
    private bool running = false;
    private bool jumped = false;
    //ground position
    [SerializeField]
    Transform groundPos;
    [SerializeField]
    LayerMask groundLayer;

  
    int frameDelay = 0;
    //healthbarscript
    private HealthbarScript healthbar;
    //ammoscript
    private AmmoScript ammotext;
    //tickrate /// for id of each msg 
    private int tick = 0;
    private float tickRate = 1.0f / 60.0f;
    private float tickDeltaTime = 0f;

    private int reloadTick = 0;
    private bool reloadbool = false;
    TestData oldstatetestdata;
    TestData newtestdatata;

    //player speed
    float currentspeed = 0;
    //list
    List<TestData> testDatalist;
    [SerializeField]
    public int framesReload = 3;
    private MovementState playerState;
    bool testingsends = false;
   
    public float GetHeath { get { return HealthPoint; } }
    public int GetFrameDelay { get { return frameDelay; } set { frameDelay = value; } }
    //player spawning in the scene
    public override void OnNetworkSpawn()
    {
        
        //locks cursor will be used in final but not currently
        Cursor.lockState = CursorLockMode.Locked;
        Playerpositions = new List<Playpostest>();
        reloading = false;
        camera = GameObject.FindGameObjectWithTag("Camera").GetComponent<Camera>();
        if (camera)
        {
            camera.transform.SetParent(transform);
            camera.transform.localPosition = Vector3.zero;
            camera.transform.localRotation = Quaternion.identity;           
        }
        playerState = MovementState.Moving;
        testDatalist = new List<TestData>();
        healthbar = FindObjectOfType<HealthbarScript>();
        healthbar.Testing(HealthPoint);
        ammotext = FindObjectOfType<AmmoScript>();
        ammotext.ChangeText(AmmoCount);
    }
    // Update is called once per frame
    void Update()
    {
        if (PLayerMovement)
        {
            tickDeltaTime += Time.deltaTime;

            //  Debug.Log(tickDeltaTime);
            while (tickDeltaTime > tickRate)
            {
                if (!reloading)
                {
                    if (PLayer)
                    {
                        PlayerRotationVoid();
                        
                    }

                    PlayerMovementVoid();


                    //reset tick and add on


                    if (AmmoCount >= 1.0f)
                    {
                        // if the left mouse is clicked spawn a bullet
                        if (Input.GetButtonDown("Fire1"))
                        {
                            Debug.Log("firing");
                            playerState = MovementState.Attacking;                        
                        }

                    }
                    //testingsends
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        if (testingsends)
                        {
                            testingsends = false;
                        }
                        else
                        {
                            testingsends = true;
                        }
                        
                    }

                    //gravity function testing
                    if (!jumped)
                    {
                        GravityFunction();
                    }
                    else
                    {
                        playerState = MovementState.jumping;
                        playerPosTesting2._grounded = false; 
                    }
                    
                    //allow the player to reload
                    if (AmmoCount <= 29.0f)
                    {
                        //when pressed the player will start reloading
                        if (Input.GetKeyDown(KeyCode.F))
                        {
                            RStartReloading();
                        }
                    }
                }
                else
                {
                    ammotimer -= Time.deltaTime;
                    //once the timer reaches 0 the player ahs reloaded
                    if (ammotimer <= 0.0f)
                    {               
                        reloading = false;
                        playerState = MovementState.Moving;
                        ammotext.ChangeText(AmmoCount);
                        reloadbool = false;
                    }
                    var testtick = tick - frameDelay;
                    if (reloadTick < testtick && !reloadbool)
                    {
                        Reloading();
                        reloadbool = true;
                        Debug.Log("reloadhashap" +tick);
                    }                  
                   
                    newtestdatata._id = tick;                      
                }
                if (HealthPoint < 100.0f)
                {
                    HealtingPLayer();
                }
                tickDeltaTime -= tickRate;
                tick++;
                playerPosTesting2._playerpos = transform.position;
              
                CreateSavestates();


                if (Input.GetKeyDown(KeyCode.H))
                {
                    Debug.Log("the H was presed");
                    GetComponent<NetworkObject>().Despawn();
                    Destroy(this);


                }

            }

        }

    }

  
    private void RStartReloading()
    {
        reloading = true;
        ammotimer = 1.0f;
        AmmoCount = ammoMax;
        playerState = MovementState.Reloading;       
        newtestdatata._playerstate = MovementState.Reloading;
        newtestdatata._direction = Direction.Idle;
        Debug.Log("Reloading");
    }

    protected void CreateSavestates()
    {
        Playerpositions.Add(playerPosTesting2);
        testDatalist.Add(newtestdatata);


        
        if (Playerpositions.Count >= frameDelay)
        {
            netMove.ProcessPlayerMoevement(currentspeed, oldstatetestdata, playerPosTesting);
            Playerpositions.RemoveAt(0);
            testDatalist.RemoveAt(0);          
        }        
        playerPosTesting = Playerpositions[0];
        oldstatetestdata = testDatalist[0];
    }
    //void for reloading
    protected void Reloading()
    {
        var t = new Vector2(0, 0);
        var d = Direction.Idle;
       

        oldstatetestdata._position = t;
        oldstatetestdata._direction = d;
        oldstatetestdata._playerstate = MovementState.Reloading;        
        newtestdatata._position = t;
        oldstatetestdata._direction = d;
        for (int i = 0; i < testDatalist.Count; i++)
        {
            TestData ts = testDatalist[i];
           
            ts._playerstate = MovementState.Reloading;            
            ts._direction = d;
            ts._position = t;
            testDatalist[i] = ts;
        }
        oldstatetestdata = testDatalist[0];
        //Input.ResetInputAxes();
        Input.ClearLastPenContactEvent();
        playerPosTesting._rotation = Vector2.zero;       
        netMove.ProcessPlayerMoevement(currentspeed, oldstatetestdata, playerPosTesting);
        Debug.Log("reloading has happened");
    }

    public void StartReloading()
    {
        reloading = true;
    }
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
        var d = netMove.ProcessPlayerDirection(xMove,yMove);

        TestData help = new TestData()
        {
            _position = new Vector2(xMove, yMove),
            _direction = d,
            _attacking = false,
            _id = tick,
            _playerstate = playerState,
           // _grounded = isGrounded,
        };

        
        if (tick >= 2)
        {
            if (!testingsends)
            {
               // oldstatetestdata = testDatalist[0];               
                if (running)
                {
                    currentspeed = 2.0f;
                }
                else
                {
                    currentspeed = 5.0f;
                }                
                newtestdatata = help;                             
            }

        }
        else if (tick == 1)
        {            
            newtestdatata = help;
          //  testDatalist.Add(help);
            Debug.Log("escape" + testDatalist.Count);
         
        }
        else
        {
            newtestdatata = help;
          
         //   testDatalist.Add(help);
            Debug.Log("escape" + testDatalist.Count);
        }   

       
       


       // GravityFunction();

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            running = true;           
        }
        else
        {
            if (running != false)
            {
                running = false;
            }
            //Reset to normal speed
        }
    }

    //void for player rotaion
    protected void PlayerRotationVoid()
    {
        //get the movements of the mouse
        float MouseX = Input.GetAxis("MouseX") * MouseSpeed * Time.deltaTime;
        float MouseY = Input.GetAxis("MouseY") * MouseSpeed * Time.deltaTime;

        playerPosTesting2._rotation = new Vector2(MouseX,MouseY);     
    }

    public void ProjectileSpawn()
    {
         GameObject bullet = Instantiate(projectile, gunpos.transform.position, transform.rotation);
         bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * ProjectileSpeed, ForceMode.Impulse);
        playerState = MovementState.Moving;
        oldstatetestdata._playerstate = MovementState.Moving;        
        for(int i = 0; i<testDatalist.Count;i++)
        {
            if (testDatalist[i]._playerstate == MovementState.Attacking)
            {
                var ts = testDatalist[i];
                ts._playerstate = MovementState.Moving;
                testDatalist[i] = ts;
            }
        }
        AmmoCount--;
        ammotext.ChangeText(AmmoCount);
        netMove.CanFireAgain();
    }
    public void PlayerDamaged()
    {
        HealthPoint -= 20.0f;
        healthbar.Testing(HealthPoint);

    }

    private void HealtingPLayer()
    {
        healthtimer += tickRate;
        if(healthtimer >= 2.0f) 
        {
            HealthPoint += 20.0f;
            healthbar.Testing(HealthPoint);
            healthtimer = 0;
        }
    }
    private void GravityFunction()
    {
        //check if the player is on the ground before collisions
        isGrounded = Physics.CheckSphere(groundPos.position,0.4f,groundLayer);
        if (isGrounded && playerState != MovementState.jumping)
        {
            playerPosTesting2._grounded = true;
        }
        else
        {
          playerPosTesting2._grounded =  false;
        }
        //when jumping change the state to jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {                     
             jumped = true;
        }
     
    }
   
}

