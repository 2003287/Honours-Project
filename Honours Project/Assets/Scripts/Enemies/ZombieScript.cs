using System.Collections;
using System.Collections.Generic;
using GameMovement.Network;
using UnityEngine.AI;
using UnityEngine;

public class ZombieScript : CahracterBase
{
    //spawn and proximity zombie sound
    //Joseph Sardin, Zombie #7, CREAHmn,
    //Available at: https://bigsoundbank.com/sound-2112-zombie-7.html ,Accessed on:15/4/23.

    //attacking the player noise    
    //Fesliyanstudios, Zombie Short Attack A3 Sound effect,
    //Available at: https://www.fesliyanstudios.com/play-mp3/3171 Accessed on: 15/4/23.	


    //player
    private GameObject player;
   
    //varibles used by the enemy for the state, pathfinding and if hit
    private Enemystate enemyState;

    public Enemystate GetEnemyState => enemyState;
    private NavMeshAgent agent;
    private bool hitBool;

    //varibles used to play sound when close to the player
    [SerializeField]
    AudioSource distanceGrowl;
    float growlTimer;
    private float distance;
    //dictionary
    private Dictionary<int, ZombieData> zomDic;     
    //game manager
    private Manager gameManager;

    [SerializeField]
    AudioSource attackClip;
    //ticks system
    TickSystem ticks;   
    private float deadTick = 0;


    //check if the enemy is attacking the player
    private bool couldAttack = false;
    public bool getCouldAttack { get { return couldAttack; } set { couldAttack = value; } }

    //varible for death
    private MeshRenderer mesh;
    [SerializeField]
    MeshRenderer noseMesh;
    [SerializeField]
    BoxCollider attackbox;
    private CapsuleCollider capCollider;


    private bool runnerZombie = false;
    //reference to set running zombie
    public bool GetRunner { set { runnerZombie = value; } }
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<Player>().gameObject;
        gameManager = FindObjectOfType<Manager>();
        enemyState = Enemystate.Running;
        //set player and speed of enemy
        agent.destination = player.transform.position;
        if (runnerZombie)
        {
            agent.speed = runningMovementSpeed;
        }
        else
        {
            agent.speed = normMovementSpeed;
        }
        
        hitBool = false;
        gameObject.GetComponent<Renderer>().material.color = Color.blue;
        NetworkMovementClass tes = FindObjectOfType<NetworkMovementClass>();
        zomDic = new Dictionary<int, ZombieData>();
        //ticks system
        ticks = new TickSystem(tes.ReturnTick(), tes.ReturnTick());
        //bools for attacking and alive
        alive = true;
        couldAttack = false;
        //compenets
        mesh = gameObject.GetComponent<MeshRenderer>();
        capCollider = gameObject.GetComponent<CapsuleCollider>();
        growlTimer = 0;
    }
    // Update is called once per frame
    void Update()
    {
        ticks.tickDeltaTime += Time.deltaTime;
     
        while (ticks.tickDeltaTime > ticks.tickRate)
        {
            if (alive)
            {
                //if there is a player update the enemy based on nehaviour and add the state to the dictionary
                if (player != null)
                {
                    SwitchEnemyState();
                    ZombiePositionUpdate();
                }

                //when the enemy is dead disable behaviour             
                if (HealthPoint <= 0)
                {
                    deadTick = ticks.tick;
                    alive = false;
                    mesh.enabled = false;
                    agent.enabled = false;
                    noseMesh.enabled = false;
                    attackbox.enabled = false;
                    capCollider.enabled = false;
                }
            }
            else
            {
                //check when to remove the enemy on the screen
                var ts = ticks.tick - 16;
                if (deadTick < ts)
                {
                    if (gameManager != null)
                    {
                        gameManager.RemoveFromList(this.gameObject);
                        Destroy(this.gameObject);
                    }
                }
            }
            //gather distance between the player and the enemy
            if (player != null)
            {
                distance = Vector3.Distance(transform.position, player.transform.position);
                DistanceNoise();
            }
           
            //increase ticks
            ticks.tick++;
            ticks.tickDeltaTime -= ticks.tickRate;
            //update dictionary to remove the earliest frame once at max amount
            if (zomDic.Count >= 16)
            {
                int re = ticks.tick - 16;
                zomDic.Remove(re);
            }
        }
    }

    //play sound if the player is in close proximity after duration of time
    private void DistanceNoise()
    {
        //check player is close to teh enemy
        if (distance <= 100)
        {           
            growlTimer += Time.deltaTime;
            //when the timer is complete play the sound
            if (growlTimer >= 8.0)
            {
                growlTimer = 0;
                distanceGrowl.Play();
            }
        }
    }
    //update the dictionary for this frame
    private void ZombiePositionUpdate()
    {
        //create the struct
        ZombieData zom = new ZombieData()
        {
            _enemystate = enemyState,
            _position = transform.position,
            _id = ticks.tick,
            _speed = agent.speed,
            _timer = ammotimer,
            _alive = alive,
        };
        //add struct to dictionary
        zomDic.Add(ticks.tick,zom);
    }

    //full Reset all movement for the zombie applied to zombies spawned after the tick for Rollback
    public void FullReset()
    {
        //check the zombie is alive
        if (alive)
        {
            if (zomDic != null)
            {
                //get the first key from the zombie
                int t = ZombieHelper.GetFirstKey(zomDic);
                //check the dictionary contains the tick
                if (zomDic.ContainsKey(t))
                {
                    var st = zomDic[t];
                    //get positions and the number of frames of Rollback
                    transform.position = st._position;
                    var roll = ticks.tick - st._id;
                    for (int i = (int)roll; i > 0; i--)
                    {
                        var test = ticks.tick - i;
                        //before applying rollback check the dictionary contains this frame
                        if (zomDic.ContainsKey(test))
                        {
                            ZombieRollback(test);
                        }
                    }
                }
            }
        }     
    }

    //Start the Rollback process for the enemy
    public void ResetPosition(int tick)
    {
        //check enemy is alive
        if (alive)
        {
            bool testingbool = false;
            //check there is a dictionary
            if (zomDic != null)
            {
                //get the Rollback frames
                int tickbetweeen = ticks.tick - tick;                           
                //while the zombie is alive do a positional rollback
                //if its dead just ignore as there is no point in it
                //check teh dictionary contains the tick
                if (zomDic.ContainsKey(tick))
                {                    
                    testingbool = true;
                   
                }

                if (testingbool)
                {
                    //Redo every Rollback Frame
                    for (int i = tickbetweeen; i>0;i--)
                    {
                        var ts = ticks.tick - i;

                        if (zomDic.ContainsKey(ts))
                        {
                           GetRollState(ts);
                        }
                    }
                }
            }
        }

    }
    //gather the state for rollback from the dictionary
    private void GetRollState(int t)
    {    //get varibles for tick
        ammotimer = zomDic[t]._timer;
        transform.position = zomDic[t]._position;
        agent.speed = zomDic[t]._speed;
        //check the enemy state and switch if not attacking teh player
        if (enemyState != zomDic[t]._enemystate)
        {
            if (enemyState != Enemystate.Attacking)
            { 
                 enemyState = zomDic[t]._enemystate;           
            }           
        }        
        //reset the goal destination 
        agent.destination = player.transform.position;
        //Rollback the enemy for this frame
        ZombieRollback(t);
    }
    //apply Rollback to the zombie
    private void ZombieRollback(int t)
    {
        //only apply Rollback during movement state
        if (enemyState == Enemystate.Running)
        {
            //check the key is valid
            if (zomDic.ContainsKey(t))
            {
                //move the enemy and overwite the state
                ZombieMovement();                
                var dic = zomDic[t];
                dic._position = transform.position;
                zomDic[t] = dic;
            }
        }
        
    }    
    //apply movement during Rollback
    private void ZombieMovement()
    {
        //when there is a player 
        if (player != null)
        {
            if (!agent.isStopped)
            {
                //move towards the player
                //using different rates for slow and fast zombies
                //fast
                if (hitBool || runnerZombie)
                {
                    transform.position = Vector3.MoveTowards(transform.position, player.transform.position, runningMovementSpeed * Time.deltaTime);
                }
                //slow
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, player.transform.position, normMovementSpeed * Time.deltaTime);
                }
            }
        }
        
    }

    //switch for the Enemystates
    private void SwitchEnemyState()
    {
        switch (enemyState)
        {
            case Enemystate.Attacking:
                AttackState();
                break;
            case Enemystate.Running:
                RunState();
                break;

            case Enemystate.Paused:
                PausedState();
                break;

            default:
                Debug.Log("something went wrong");
                break;

        }
    }
    //get the first tick of the enemy
    public int GetfirstTick()
    {
        return ticks.firsttick;
    }
    //attacking the player
    public void Attacking()
    {
        if (alive)
        {
            //check teh enemy is not already attacking
            if (enemyState != Enemystate.Attacking)
            {
                //change colour and state
                gameObject.GetComponent<Renderer>().material.color = Color.red;
                enemyState = Enemystate.Attacking;
                ammotimer = 0.0f;
                //tell the player they have been attacked
                if (player)
                {
                    if (!player.GetComponent<Player>().GetDamage)
                    {
                        player.GetComponent<Player>().PlayerDamaged();
                    }
                   
                }
                //disable movement in the enemy
                if (agent.isActiveAndEnabled)
                {
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                }
                //play audio 
                attackClip.Play();
            }
        }
       
        
    }
    //attackstate behviour 
    private void AttackState()
    {
        Debug.Log("Attacking Player");
        ammotimer += Time.deltaTime;
        couldAttack = false;
        //when teh timer is abouve value switch to pauesed state
        if (ammotimer >= 0.75f)
        {
            ammotimer = 0.0f;
            enemyState = Enemystate.Paused;
        }
        

    }
    //pauesed state behaviour
    private void PausedState()
    {
        ammotimer += Time.deltaTime;
        //when the timer is above 2 switch state
        if (ammotimer >= 2.0f)
        {            
            enemyState = Enemystate.Running;
            if (hitBool)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.grey;
            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = Color.blue;
            }
            agent.isStopped = false;
        }
    }
    
    //set the enemy to hit
    public void SetHitBool()
    {
        hitBool = true;
        gameObject.GetComponent<Renderer>().material.color = Color.grey;
        agent.speed = runningMovementSpeed;

    }
    //return the hit bool
    public bool GetHitBool()
    {
        return hitBool;
    }

    //decrease the health of the enemy
    public void HpDecrease()
    {
        HealthPoint -= 25.0f;
        //update accuracy hit varible
        var t = gameManager.SetZombHit + 1;
        gameManager.SetZombHit = t ;
    }

    //runstate for the enemy
    private void RunState()
    {
        ammotimer += Time.deltaTime;
        //restart movement of the agent
        if (agent.isStopped)
        {
            agent.isStopped = false;
        }

        //reset the destination of the object
        if (ammotimer >= 0.3f)
        {
            if (agent.isActiveAndEnabled)
            {
                agent.destination = player.transform.position;
            }          
            ammotimer = 0.0f;
        }

        //change the zombies colour
        ZombieHelper.ChangeColourInRunState(couldAttack,hitBool,gameObject.GetComponent<Renderer>());       

    }
}
