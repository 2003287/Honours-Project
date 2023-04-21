using System.Collections;
using System.Collections.Generic;
using GameMovement.Network;
using UnityEngine;

internal struct BulletStruct {
   public int _id;
   public Vector3 _position;
   public bool _onscreen;
   public float _timer;
   public Vector3 _velocity;
}

public enum Bulletstate { Alive,Dead };
public class BulletScript : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    float timer;

    [SerializeField]
    bool alive;

    private Dictionary <int, BulletStruct> bulletDictionary;

    //tick stuff look into fixing it
    private int tick = 0;
    private int fistrtick = 0;
    private float tickRate = 1.0f / 60.0f;
    private float tickDeltaTime = 0f;
    private int deadTick = 0;
    private MeshRenderer mesh;
    private Collider mycollider;
    private Rigidbody rb;
    private int hittype = 0;
    void Start()
    {
        alive = true;
        NetworkMovementClass tes = FindObjectOfType<NetworkMovementClass>();
        fistrtick = tes.ReturnTick();
        tick = tes.ReturnTick();
        bulletDictionary = new Dictionary<int, BulletStruct>();
        mesh = gameObject.GetComponent<MeshRenderer>();
        mycollider = GetComponent<SphereCollider>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //while alive the bullet will fire

        tickDeltaTime += Time.deltaTime;
        //  Debug.Log(tickDeltaTime);
        while (tickDeltaTime > tickRate)
        {


            if (alive)
            {
                timer -= Time.deltaTime;

                if (timer <= 0.0f)
                {
                    deadTick = tick;
                    BulletDead(1);
                    
                }
            }
            else
            {
                
                Debug.Log("bullet is dead" + deadTick +"ticks"+ tick);
                var test = tick - 16;
                if (deadTick < test)
                {
                    Debug.Log("bullet is dead and destroyed");
                    Destroy(this.gameObject);
                   
                }
                
            }

            //create bulletstate
            CreateBulletState();
            Debug.Log(gameObject.GetComponent<Rigidbody>().velocity+"Bullet velocity");
            
            tickDeltaTime -= tickRate;
            tick++;

            if (bulletDictionary.Count > 16)
            {
                int t = tick - 16;
                if (bulletDictionary.ContainsKey(t))
                {
                    bulletDictionary.Remove(t);
                }
                
            }
        }

       
    }


    private void CreateBulletState()
    {
        BulletStruct test = new BulletStruct() { 
            _id = tick,
            _position = transform.position,
            _onscreen = alive,
            _timer = timer,
            _velocity = gameObject.GetComponent<Rigidbody>().velocity
        };
        bulletDictionary.Add(tick,test);
    }

    private void BulletDead(int t)
    {
        mesh.enabled = false;
        mycollider.enabled = false;
        rb.detectCollisions = false;
        alive = false;
        hittype = t;
    }

    public void BulletRollback(int ticks)
    {
        if (bulletDictionary.ContainsKey(ticks))
        {
            var bState = bulletDictionary[ticks];

            if (!bState._onscreen)
            {
                //skip as nothing will change
                Debug.Log("off screen do nothing");
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
                        Redisplay();
                        Rolling(ticks);
                    }
                }

            }
        }      
    }

    private void Redisplay()
    {
        mesh.enabled = true;
        mycollider.enabled = true;
        rb.detectCollisions = true;
        alive = true;
        hittype = 0;
    }
    private void Rolling(int ticks)
    {
        int roll = tick - ticks;

        for (int i = roll; i > 0; i--)
        {
            if (bulletDictionary.ContainsKey(i))
            {
                transform.position = bulletDictionary[i]._position;
            }
        }
    }

    //when teh bullet collides with something check what it is
    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            //if its the ground destroy itself
            if (collision.gameObject.tag == "Ground")
            {
                Debug.Log("working");
                // Destroy(this.gameObject);
                deadTick = tick;
                BulletDead(2);
                
            }
            //if its the enemy change its state before destroying self
            if (collision.gameObject.tag == "Enemy")
            {
                if (collision.gameObject.TryGetComponent<ZombieScript>(out ZombieScript zm))
                { 
                  if (!zm.GetHitBool())
                    {
                        collision.gameObject.GetComponent<ZombieScript>().SetHitBool();
                    }
                    zm.HpDecrease();
                }
               
                Debug.Log("hit enemy");
                deadTick = tick;
                BulletDead(3);
                
               // Destroy(this.gameObject);

            }
        }

      
    }
}
