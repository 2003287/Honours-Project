using System.Collections;
using System.Collections.Generic;
using GameMovement.Network;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Manager : MonoBehaviour
{
    //network manager
    [SerializeField]
    NetworkManager netManager;

    //positions to spawn enemies
    [SerializeField]
    List<Transform> spawnPoints;

    [SerializeField]
    GameObject zombiePrefab;
    
    //player prefab
    [SerializeField]
    GameObject playerPrefab;
    private Player player;

    [SerializeField]
    float timeTillSpawn = 1.0f;

    private bool spawnedin = false;
    //Number of zombies on the screen
    private int maxZombies = 5;
    //time till spawn
    private float spawnTimer = 0.0f;

    private int MaxNumberZombies = 12;
    private int killedZombies = 0;
    private int numberOfZombies = 0;
    private int totalKilledZombs = 0;
    private bool started = false;
    private List<GameObject> zombieList;
    [SerializeField]
    int framedelay;
    //dictionarycontainers
    private Dictionary<int, List<GameObject>> zombieDicList;
    private Dictionary<int, ManagerState> GameStateDic;

    private int tick = 0;    
    private float tickRate = 1.0f / 60.0f;
    private float tickDeltaTime = 0f;

    private bool deathallowed = true;
    private int tickdeath = 0;
    public void GameStart()
    {
        zombieList = new List<GameObject>();
        if (netManager.IsClient)
        {
            GameObject pl = Instantiate(playerPrefab);
            pl.GetComponent<NetworkObject>().Spawn(true);
            
            if (pl.activeSelf)
            {
                player = FindObjectOfType<Player>();
                player.GetFrameDelay = framedelay;
                Debug.Log("this player was found in the scene");
            }

            InstantiateZombie();
            started = true;
        }
        //dictionaries
        GameStateDic = new Dictionary<int, ManagerState>();
        zombieDicList = new Dictionary<int, List<GameObject>>();
    }
    public void RemoveFromList(GameObject prefab)
    {
        if (deathallowed)
        {
            deathallowed = false;
            tickdeath = tick;
            if (zombieList.Contains(prefab))
            {
                zombieList.Remove(prefab);
                Debug.Log("Removed");
            }
            killedZombies++;
            numberOfZombies--;
            totalKilledZombs++;
        }
       
    }

    public void Deload()
    {       
        GameStateDic.Clear();        
    }
    public void JustATest(int ticks)
    {
        //check same number of zombies
        
        if (GameStateDic.ContainsKey(ticks))
        {
            var t = GameStateDic[ticks];
            var zmlist = zombieDicList[ticks];
            var testing = zombieDicList[tick-1];
            if (zombieDicList.ContainsKey(ticks))
            {
                testing = zombieDicList[ticks];
            }
            //Same number of zombies
            if (t._zombnumber == numberOfZombies)
            {
                Debug.Log("A zombie is fine");
                //check number of kills are the same
                //if so wonderful and rollback
                if (t._zombKilled == totalKilledZombs)
                {
                    foreach (var item in zombieList)
                    {                        
                        if (item != null)
                        {
                            item.GetComponent<ZombieScript>().ResetPosition(ticks);
                        }
                        
                    }
                }
                else
                {
                    for (int i = 0; i < zombieList.Count; i++)
                    {
                        var zc = zombieList.Count - 1;
                        if (i != zc)
                        {
                            zombieList[i].GetComponent<ZombieScript>().ResetPosition(ticks);
                        }
                        else
                        {
                            zombieList[i].GetComponent<ZombieScript>().FullReset();
                        }
                    }
                }
            }
            //player has just killed a zombie
            else if (t._zombnumber > numberOfZombies)
            {

                if (t._zombKilled == totalKilledZombs)
                {
                    Debug.Log("the zombie died this frame and hasn't been accounted for yet");
                    //do nothing just ignore it until i ask about it
                }
                else
                {
                    var tcheck = tick - ticks;

                    if (tcheck <= 2)
                    {

                        for (int i = tcheck; i > -1; i--)
                        {
                            var g = tick - i;
                            if (GameStateDic.ContainsKey(g))
                            {
                                var ts = GameStateDic[g];
                                ts._zombnumber = numberOfZombies;
                                ts._zombKilled = totalKilledZombs;
                                GameStateDic[g] = ts;
                            }
                        }
                    }
                    Debug.Log("the server has has just killed the zombie");
                }
                //death inbetween states figureout how to handle it
                foreach (var item in testing)
                {
                    if (zombieList.Contains(item))
                    {
                        Debug.Log("zombie is fine right now");
                        item.GetComponent<ZombieScript>().ResetPosition(ticks);
                    }
                    else
                    {
                        Debug.Log("There is a zombie on the loose");
                    }
                   // 
                }
            }
            else
            {
                Debug.Log("A zombie has spawned fix it");
                //spawned in a new zombie
                //figure this out
                foreach (var item in zombieList)
                {
                   var zom = item.GetComponent<ZombieScript>();
                    if (zom.GetfirstTick() >= ticks)
                    {
                        Debug.Log("A is higher");
                        zom.FullReset();
                    }
                    else
                    {
                        zom.ResetPosition(ticks);                        
                    }                    
                }

            }
           
        }
        else
        {
            Debug.Log("there is something out of sync with the comparor fix it");
        }
        Debug.Log("max zombies" +maxZombies);
        Debug.Log("total kills" +totalKilledZombs);
    }
    public void TestingSwan()
    {
        player.ProjectileSpawn();
    }

    public void PLayerJumping()
    {
        player.FixJumping();
    }
    private void InstantiateZombie()
    {
        spawnedin = true;
        numberOfZombies++;
        Transform basePos = gameObject.transform;
        int firstpos = 0;
        var distance = 0.0f;
        foreach (Transform position in spawnPoints)
        {
            if (firstpos == 0)
            {
                basePos = position;
                firstpos++;
                distance = Vector3.Distance(basePos.position,player.gameObject.transform.position);
            }
            else
            {
                var newDist = Vector3.Distance(position.position, player.gameObject.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    basePos = position;
                }
            }
        }
        GameObject zm = Instantiate(zombiePrefab,basePos.position, Quaternion.identity);
        zombieList.Add(zm);
       
    }
    // Update is called once per frame
    void Update()
    {
        if (started)
        {
            tickDeltaTime += Time.deltaTime;

            while (tickDeltaTime>tickRate)
            {
                if (player == null)
                {
                    Debug.Log("the player is dead boyoy");
                    NetworkManager.Singleton.Shutdown();
                    NetworkManager networkManager = GameObject.FindObjectOfType<NetworkManager>();
                    Destroy(networkManager.gameObject);
                    SceneManager.LoadScene("MainMenu");
                }
                else
                {
                    ZombieSpawning();

                    StateSaving();
                    DeletingStates();
                    tick++;
                    if (!deathallowed)
                    {
                        var t = tickdeath + 2;
                        if (tick > t)
                        {
                            deathallowed = true;
                            tickdeath = tick;
                        }
                    }
                }
               
                tickDeltaTime -= tickRate;
            }
           
        }
        
    }

    private void ZombieSpawning()
    {
        if (zombieList.Count <= maxZombies)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= timeTillSpawn)
            {
                if (!spawnedin)
                {
                    InstantiateZombie();
                }                
                spawnTimer = 0.0f;
                spawnedin = false;
            }
        }

        if (killedZombies >= 5)
        {
            if (maxZombies < MaxNumberZombies)
            {
                maxZombies++;
                killedZombies = 0;
            }
        }
    }
    private void StateSaving()
    {
        ManagerState ms = new ManagerState()
        {
            _allowance = maxZombies,
            _spawntimer = spawnTimer,
            _killedZombiesReset = killedZombies,
            _zombnumber = numberOfZombies,
            _zombKilled = totalKilledZombs,
        };
        GameStateDic.Add(tick,ms);
        zombieDicList.Add(tick,zombieList);
    }

    //removing states from dictionary
    private void DeletingStates()
    {
        if (tick >= framedelay)
        {
            var t = tick - framedelay;
            if (GameStateDic.ContainsKey(tick))
            {
                GameStateDic.Remove(t);
            }
            if (zombieDicList.ContainsKey(t))
            {
                zombieDicList.Remove(t);
            }
            
        }
       
    }
}
