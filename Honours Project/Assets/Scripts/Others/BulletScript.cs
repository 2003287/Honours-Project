using System.Collections;
using System.Collections.Generic;
using GameMovement.Network;
using UnityEngine;

//Bullet Noise
//Videvo, 2022 ,Gun Shot Single Fire In PE1097304, The Premiere Edition 10,
//Available at: https://www.videvo.net/sound-effect/gun-shot-single-fire-in-pe1097304/246186/,Accessed on:15/4/23.


//struct for storing the bullets information used during Rollback
//not store with deterministic varible as the bullets position is garentied each frame
internal struct BulletStruct {
   public int _id;
   public Vector3 _position;
   public bool _onscreen;
   public float _timer;
   public Vector3 _velocity;
}

public class BulletScript : MonoBehaviour
{
    // Start is called before the first frame update
    //timer for duration on screen without a hit
    [SerializeField]
    float timer;
    //bool to display the bullet on screen
    [SerializeField]
    bool alive;

    //dictionary to contain all the bulletstates
    private Dictionary <int, BulletStruct> bulletDictionary;
    
    //tick container
    private TickSystem ticks;
    //tick when the bullet died
    private int deadTick = 0;

    //varible disable when supposed to die
    private MeshRenderer mesh;
    private Collider mycollider;
    private Rigidbody rb;

    //what has hit the bullet
    private int hittype = 0;
    private Manager gameManager;
    //used to do multiple frames at once
    private bool speedUp = true;

    //setup the varibles used by the bullet
    void Start()
    {
        alive = true;
        //get information to keep teh ticks relative
        NetworkMovementClass tes = FindObjectOfType<NetworkMovementClass>();
        ticks = new TickSystem(tes.ReturnTick(), tes.ReturnTick());
        //create the dictionary
        bulletDictionary = new Dictionary<int, BulletStruct>();

        //gather compenets attached to the bullet
        mesh = gameObject.GetComponent<MeshRenderer>();
        mycollider = GetComponent<SphereCollider>();
        rb = GetComponent<Rigidbody>();
        
        //get the Game Manager
        gameManager = FindObjectOfType<Manager>();        
    }

    // Update is called once per frame
    void Update()
    {
        //tick system used by the bullet
        ticks.tickDeltaTime += Time.deltaTime;

        //when the next frame occurs 
        while (ticks.tickDeltaTime > ticks.tickRate)      
        {

            //while alive update the timer
            if (alive)
            {
                timer -= Time.deltaTime;
                //if the bullet should depsawn 
                if (timer <= 0.0f)
                {
                    //despawn the bullets
                    deadTick = ticks.tick;
                    BulletDead(1);
                    Debug.Log("Ticksytem is working");
                }
            }
            else
            {              
               //when dead check if the duration for remove has occured
                var test = ticks.tick - 16;        
                if (deadTick < test)
                {            
                    //when it has occured remove the bullet and destroy the object
                    gameManager.BulletRemove(this.gameObject, hittype);
                    Destroy(this.gameObject);
                   
                }
                
            }
            //do A speed up for a few frame acts similarly to skipping the first few frames of an animation
            if (speedUp && ticks.tick == ticks.firsttick)          
            {              
                speedUp = false;
                var player = FindObjectOfType<Player>();
                //gather a distance far infront of teh player
                var dist = player.transform.position + player.transform.forward * gameManager.GetFrameDelay / 10;
                //move towards that distance for a short amount
                transform.position = Vector3.MoveTowards(transform.position, dist, gameManager.GetFrameDelay / 10);               
            }          

            //create bulletstate
            CreateBulletState();           
            
            //update the ticks in the bullet
            ticks.tickDeltaTime -= ticks.tickRate;           
            ticks.tick++;
          
            //update the dictionary to remove frames once at the max number of stored frames
            if (bulletDictionary.Count > 16)
            {
                //get the earliest frame and remove the frame
                int t = ticks.tick- 16;
                if (bulletDictionary.ContainsKey(t))
                {
                    bulletDictionary.Remove(t);
                }
                
            }
        }

       
    }

    //create a Bulletstate and add it to the dictionary
    private void CreateBulletState()
    {
        //create bullet struct
        BulletStruct test = new BulletStruct() { 
            _id = ticks.tick,            
            _position = transform.position,
            _onscreen = alive,
            _timer = timer,
            _velocity = gameObject.GetComponent<Rigidbody>().velocity
        };
        //add to the dictionary
        bulletDictionary.Add(ticks.tick,test);
       
    }

    //disable the behaviour of the bullet
    private void BulletDead(int t)
    {
        mesh.enabled = false;
        mycollider.enabled = false;
        rb.detectCollisions = false;
        alive = false;
        hittype = t;
    }

    //apply Rollback to teh Bullets
    public void BulletRollback(int ticks)
    {
        //check the dictionary contains the tick recieved
        if (bulletDictionary.ContainsKey(ticks))
        {
            //get the bullet state
            var bState = bulletDictionary[ticks];

            //when the bullet is dead ignore Rollback
            if (!bState._onscreen)
            {                
                return;
            }
            else
            {               
                //just do a rollback as nothing will change
                if (alive == bState._onscreen)
                {
                    Rolling(ticks);
                }
                else
                {
                    //right it has hit something and needs to redo movement
                    if (hittype == 1 || hittype == 2)
                    {
                        //redisplay objects
                        Redisplay();
                        Rolling(ticks);
                    }
                }

            }
        }      
    }

    //redisaply the bullet in the scene
    private void Redisplay()
    {
        mesh.enabled = true;
        mycollider.enabled = true;
        rb.detectCollisions = true;
        alive = true;
        hittype = 0;
    }
    //apply Rollback to the bullet
    private void Rolling(int tick)
    {
        //check how many frames need rollback
        int roll = ticks.tick - tick;
        //redo the positioning for each frame of rollback
        for (int i = roll; i > 0; i--)
        {
            if (bulletDictionary.ContainsKey(i))
            {
                transform.position = bulletDictionary[i]._position;
                timer = bulletDictionary[i]._timer;
            }
        }
    }

    //when the bullet collides with something check what it is
    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            //if its the ground
            if (collision.gameObject.tag == "Ground")
            {                
                deadTick = ticks.tick;                
                BulletDead(2);
                
            }
            //if its the enemy change its state before destroying self
            if (collision.gameObject.tag == "Enemy")
            {
                //get teh enemy component and update the enemy
                if (collision.gameObject.TryGetComponent<ZombieScript>(out ZombieScript zm))
                { 
                  if (!zm.GetHitBool())
                    {
                        collision.gameObject.GetComponent<ZombieScript>().SetHitBool();
                    }
                  //decrease the enemies health
                    zm.HpDecrease();
                }
               
         
                deadTick = ticks.tick;
               
                BulletDead(3);             

            }
        }

      
    }
}
