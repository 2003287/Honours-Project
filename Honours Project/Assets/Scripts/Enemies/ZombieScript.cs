using System.Collections;
using System.Collections.Generic;
using GameMovement.Network;
using UnityEngine.AI;
using UnityEngine;

public class ZombieScript : CahracterBase
{
    private GameObject goalDestination;
   
    private Enemystate enemyState;
    private NavMeshAgent agent;
    private bool hitBool;
   

    private Dictionary<int, ZombieData> zomDic;     
    private Manager gm;

    [SerializeField]
    AudioSource attackClip;
    //ticks system
    private int tick = 0;
    private int fistrtick = 0;
    private float tickRate = 1.0f / 60.0f;
    private float tickDeltaTime = 0f;
    private float deadTick = 0;

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
    public bool GetRunner { set { runnerZombie = value; } }
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        goalDestination = FindObjectOfType<Player>().gameObject;
        gm = FindObjectOfType<Manager>();
        enemyState = Enemystate.Running;
        agent.destination = goalDestination.transform.position;
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
        tick = tes.ReturnTick();
        fistrtick = tes.ReturnTick();
        alive = true;
        couldAttack = false;
        mesh = gameObject.GetComponent<MeshRenderer>();
        capCollider = gameObject.GetComponent<CapsuleCollider>();   
    }
    // Update is called once per frame
    void Update()
    {
        tickDeltaTime += Time.deltaTime;
        //  Debug.Log(tickDeltaTime);
        while (tickDeltaTime > tickRate)
        {
            if (alive)
            {
                if (goalDestination != null)
                {
                    SwitchEnemyState();
                    ZombiePositionUpdate();
                }
               
                if (HealthPoint <= 0)
                {
                    deadTick = tick;
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
                var ts = tick - 16;
                if (deadTick < ts)
                {
                    if (gm != null)
                    {
                        gm.RemoveFromList(this.gameObject);
                        Destroy(this.gameObject);
                    }
                }              
            }
            
            tick++;
            tickDeltaTime -= tickRate;
            if (zomDic.Count >= 16)
            {
                int re = tick - 16;
                zomDic.Remove(re);
            }
        }
    }

    private void ZombiePositionUpdate()
    {
        ZombieData zom = new ZombieData()
        {
            _enemystate = enemyState,
            _position = transform.position,
            _id = tick,
            _speed = agent.speed,
            _timer = ammotimer,
            _alive = alive,
        };

        zomDic.Add(tick,zom);
    }

    public void FullReset()
    {
        if (alive)
        {
            if (zomDic != null)
            {
                int t = ZombieHelper.GetKey(zomDic);
                if (zomDic.ContainsKey(t))
                {
                    var st = zomDic[t];

                    transform.position = st._position;
                    var roll = tick - st._id;
                    for (int i = (int)roll; i > 0; i--)
                    {
                        var test = tick - i;
                        if (zomDic.ContainsKey(test))
                        {
                            ZombieRollback(test);
                        }
                    }
                }
            }
        }     
    }


    public void ResetPosition(int ticks)
    {
        if (alive)
        {
            bool testingbool = false;
            if (zomDic != null)
            {
                int tickbetweeen = tick - ticks;
                Debug.Log("this is just a test" + tickbetweeen);               
                //while the zombie is alive do a positional rollback
                //if its dead just ignore as there is no point in it
                if (zomDic.ContainsKey(ticks))
                {
                    Debug.Log("Zombieposition" + transform.position);
                    testingbool = true;
                    Debug.Log("zombie has this value" + transform.position);
                }
                if (testingbool)
                {
                    for (int i = tickbetweeen; i>0;i--)
                    {
                        var ts = tick - i;

                        if (zomDic.ContainsKey(ts))
                        {
                           GetRollState(ts);
                        }
                    }
                }
            }
        }

    }

    private void GetRollState(int t)
    {    
        ammotimer = zomDic[t]._timer;
        transform.position = zomDic[t]._position;
        agent.speed = zomDic[t]._speed;
        if (enemyState != zomDic[t]._enemystate)
        {
            if (enemyState != Enemystate.Attacking)
            { 
                 enemyState = zomDic[t]._enemystate;           
            }           
        }        
        agent.destination = goalDestination.transform.position;
        ZombieRollback(t);
    }
    private void ZombieRollback(int t)
    {
        if (enemyState == Enemystate.Running)
        {
            if (zomDic.ContainsKey(t))
            {
                ZombieMovement();
                Debug.Log("this posistion" + transform.position);
                var dic = zomDic[t];
                dic._position = transform.position;
                zomDic[t] = dic;
            }
        }
        
    }    
    
    private void ZombieMovement()
    {
        if (goalDestination != null)
        {
            if (!agent.isStopped)
            {
                if (hitBool || runnerZombie)
                {
                    transform.position = Vector3.MoveTowards(transform.position, goalDestination.transform.position, runningMovementSpeed * Time.deltaTime);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, goalDestination.transform.position, normMovementSpeed * Time.deltaTime);
                }
            }
        }
        
    }

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

    public int GetfirstTick()
    {
        return fistrtick;
    }

    public void Attacking()
    {
        if (alive)
        {
            if (enemyState != Enemystate.Attacking)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.red;
                enemyState = Enemystate.Attacking;
                ammotimer = 0.0f;
                if (goalDestination)
                {
                    if (!goalDestination.GetComponent<Player>().GetDamage)
                    {
                        goalDestination.GetComponent<Player>().PlayerDamaged();
                    }
                   
                }
                if (agent.isActiveAndEnabled)
                {
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                }

                attackClip.Play();
            }
        }
       
        
    }
    private void AttackState()
    {
        Debug.Log("Attacking Player");
        ammotimer += Time.deltaTime;
        couldAttack = false;
        if (ammotimer >= 0.75f)
        {
            ammotimer = 0.0f;
            enemyState = Enemystate.Paused;
        }
        

    }

    private void PausedState()
    {
        ammotimer += Time.deltaTime;

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
    public void SetHitBool()
    {
        hitBool = true;
        gameObject.GetComponent<Renderer>().material.color = Color.grey;
        agent.speed = runningMovementSpeed;

    }
    public bool GetHitBool()
    {
        return hitBool;
    }

    public void HpDecrease()
    {
        HealthPoint -= 25.0f;
        var t = gm.SetZombHit + 1;
        gm.SetZombHit = t ;
    }

    private void RunState()
    {
        ammotimer += Time.deltaTime;
        if (agent.isStopped)
        {
            agent.isStopped = false;
        }

        if (ammotimer >= 0.3f)
        {
            if (agent.isActiveAndEnabled)
            {
                agent.destination = goalDestination.transform.position;
            }          
            ammotimer = 0.0f;
        }

        if (couldAttack)
        {
            gameObject.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            if (hitBool)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.grey;
            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = Color.blue;
            }
        }

    }
}
