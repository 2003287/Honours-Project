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
    //  public GameObject Text;


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
    float ammoMax = 12.0f;  

    private bool isGrounded = false;

    private bool jumped = false;


    //private jump timer due to bug that i cannot replicate, no idea what happened or why just adding this in incase
    private float touchGroundTimer = 0.0f;
    //ground position
    [SerializeField]
    Transform groundPos;
    [SerializeField]
    LayerMask groundLayer;

  
    int frameDelay = 0;
    //healthbarscript
    private HealthbarScript healthbar;
    //ammoscript
    private TMP_Text ammotext;
    //tickrate /// for id of each msg 
    private int tick = 0;
    private readonly float tickRate = 1.0f / 60.0f;
    private float tickDeltaTime = 0f;
  
    private bool reloadbool = false;
    PlayerData oldstatetestdata;
    PlayerData newtestdatata;
    private bool artdelay;
    private float delayTimer = 0.0f;
    private int delayCount = 0;
    public bool ArtificalDelay { set { artdelay = value; } }
 
    //list
    List<PlayerData> testDatalist;
    [SerializeField]
    public int framesReload = 3;
    private MovementState playerState;
    bool testingsends = false;
    private bool noDamage;
    public bool GetDamage => noDamage;
    private int damageTick = 0;
    public bool GetAlive => alive;

    public float GetHeath => HealthPoint;
    public int GetFrameDelay { get { return frameDelay; } set { frameDelay = value; } }
    //player spawning in the scene
    public override void OnNetworkSpawn()
    {
        alive = true;
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
        testDatalist = new List<PlayerData>();
        healthbar = FindObjectOfType<HealthbarScript>();
        healthbar.Testing(HealthPoint);
        AmmoCount = ammoMax;
        var gm = GameObject.FindGameObjectWithTag("Ammo");
        ammotext = gm.GetComponent<TMP_Text>();
        ammotext.text = "Ammo Count: " + AmmoCount.ToString()+ "/"+ ammoMax.ToString();
        PLayerMovement.enabled = false;
        PLayerMovement.transform.position = spawnPosition;
        PLayerMovement.enabled = true;
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
                    DelayMethod();
                    if(noDamage)
                    {
                        var ts = damageTick + 40;
                        if (tick > ts)
                        {
                            noDamage = false;
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
                    //once the timer reaches 0 the player ahs reloaded
                    if (ammotimer <= 0.0f)
                    {
                        reloading = false;
                        playerState = MovementState.Moving;
                        ammotext.text = "Ammo Count: " + AmmoCount.ToString() + "/" + ammoMax.ToString();
                        reloadbool = false;
                    }

                    if (!reloadbool)
                    {
                        Reloading();
                        reloadbool = true;

                    }

                    newtestdatata._id = tick;
                }
                HealthMethod();

                tickDeltaTime -= tickRate;
                tick++;
                

                CreateSavestates();
                
                //for debuging purposes
              /*  if (Input.GetKeyDown(KeyCode.H))
                {
                    Debug.Log("the H was presed");
                    camera.transform.SetParent(null);
                    if (GetComponent<NetworkObject>() != null)
                    {
                        GetComponent<NetworkObject>().Despawn();
                    }
                    
                    Destroy(this);
                }*/

            }

        }

    }

    private void HealthMethod()
    {
        if (HealthPoint < 100.0f)
        {
            HealtingPLayer();
        }
        if (HealthPoint <= 0)
        {
            alive = false;
        }
    }

    private void DelayMethod()
    {
        if (artdelay && !testingsends)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer >= 2.0f)
            {
                testingsends = true;
                int t = Random.Range(8, 23);
                delayCount = tick + t;
            }
        }
        if (testingsends)
        {           
            if (tick >= delayCount)
            {
                testingsends = false;
                delayTimer = 0;
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
        newtestdatata._position = new FixedVec2(0,0);
        Debug.Log("Reloading");
    }

    protected void CreateSavestates()
    {
        FixedVec3 t = new FixedVec3(transform.position.x, transform.position.y, transform.position.z);
        playerPosTesting2._playerpos = t;
        Playerpositions.Add(playerPosTesting2);
        testDatalist.Add(newtestdatata);        
        if (Playerpositions.Count >= frameDelay)
        {
            if (!testingsends)
            {
                netMove.ProcessPlayerMoevement(oldstatetestdata, playerPosTesting);
            }
            
            Playerpositions.RemoveAt(0);
            testDatalist.RemoveAt(0);          
        }        
        playerPosTesting = Playerpositions[0];
        oldstatetestdata = testDatalist[0];
    }
    //void for reloading
    protected void Reloading()
    {
        var t = new FixedVec2(0, 0);
        var d = Direction.Idle;        
        oldstatetestdata._position = t;
        oldstatetestdata._direction = d;
        oldstatetestdata._playerstate = MovementState.Reloading;        
        oldstatetestdata._direction = d;
        for (int i = 0; i < testDatalist.Count; i++)
        {
            PlayerData ts = testDatalist[i];
           
            ts._playerstate = MovementState.Reloading;            
            ts._direction = d;
            ts._position = t;
            testDatalist[i] = ts;
        }
        oldstatetestdata = testDatalist[0];
        //Input.ResetInputAxes();
        Input.ClearLastPenContactEvent();
        playerPosTesting._rotation = new FixedVec2(0,0);      
       
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
        var check = NetworkHelper.Vec2Creation(d);    
       

        PlayerData help = new PlayerData()
        {
            _position = check,
            _direction = d,           
            _id = tick,
            _playerstate = playerState,           
        };

        newtestdatata = help;
    }

    //void for player rotaion
    protected void PlayerRotationVoid()
    {
        //get the movements of the mouse
        float MouseX = Input.GetAxis("MouseX") * MouseSpeed * Time.deltaTime;
        float MouseY = Input.GetAxis("MouseY") * MouseSpeed * Time.deltaTime;
      //  Debug.Log("thisrotation" + MouseX + MouseY);
        playerPosTesting2._rotation = new FixedVec2(MouseX,MouseY);     
    }

    public GameObject ProjectileSpawn()
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
        ammotext.text = "Ammo Count: " + AmmoCount.ToString() + "/" + ammoMax.ToString();
        netMove.CanFireAgain();
        return bullet;
    }
    public void PlayerDamaged()
    {
        if (!noDamage)
        {
            HealthPoint -= 20.0f;
            healthbar.Testing(HealthPoint);
        }
        

    }

    public void PlayerReset()
    {
        HealthPoint = 100.0f;
        healthbar.Testing(HealthPoint);
        healthtimer = 0;
        AmmoCount = ammoMax;
        ammotext.text = "Ammo Count: " + AmmoCount.ToString() + "/" + ammoMax.ToString();
        ammotimer = 0;
        playerState = MovementState.Moving;
        alive = true;
        noDamage = true;
        damageTick = tick;
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
            touchGroundTimer = 0;
        }
        else
        {
          playerPosTesting2._grounded =  false;
            touchGroundTimer += Time.deltaTime;
            if (touchGroundTimer >= 4.0f)
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

