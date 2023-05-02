using System.Collections;
using System.Collections.Generic;
using GameMovement.Network;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;


public class Manager : MonoBehaviour
{
    //network manager
    [SerializeField]
    NetworkManager netManager;

    //positions to spawn enemies
    [SerializeField]
    List<Transform> spawnPoints;

    //spawnpoints
    [SerializeField]
    List<Transform> Deathpoints;
    
    //zombie prefab
    [SerializeField]
    GameObject zombiePrefab;
    
    //player prefab
    [SerializeField]
    GameObject playerPrefab;
    
    //the spawning zombie noise
    [SerializeField]
    AudioClip Zombspawn;

    //the spawning bullet sound
    [SerializeField]
    AudioClip bulletspawn;

    //the player in the scene
    private Player player;   

    //timer to sawpn in zombies
    [SerializeField]
    float timeTillSpawn = 1.0f;

    //which level the player is on
    [SerializeField]
    int level;
    //music in the scene
    [SerializeField]
    AudioSource backingSong;

    //text varibles
    private TMP_Text Text;
    private TMP_Text scoreText;

    //when a zombie has just been spawned
    private bool spawnedin = false;
    //Number of zombies on the screen
    private int maxZombies = 5;
    //time till spawn
    private float spawnTimer = 0.0f;
    // varibles to do with zombies on screen
    private int maxNumberZombies = 12;
    private int killedZombies = 0;
    private int numberOfZombies = 0;
    private int totalKilledZombs = 0;
    private int spawnRunningZombie = 0;
    //level has started
    private bool started = false;

    //for collection of accuracy data //sue to an error
   


    //list of zombies and bullets
    private List<GameObject> zombieList;
    private List<GameObject> bulletList;
    // the delay encountered in the scene
    [SerializeField]
    int framedelay;
    //addition of randomised delay
    [SerializeField]
    bool artDelay;
    //dictionarycontainers
    private Dictionary<int, List<GameObject>> zombieDicList;
    private Dictionary<int, List<GameObject>> bulletDicList;
    private Dictionary<int, ManagerState> GameStateDic;

    //tick class
    private int tick = 0;    
    private float tickRate = 1.0f / 60.0f;
    private float tickDeltaTime = 0f;
    private float timer = 240.0f;
    private bool deathallowed = true;
    private int tickdeath = 0;

    //data to be gathered
    private float score = 0.0f;
    private int deaths = 0;
    private int zomHits = 0;
    private int bulletsSpawned = 0;

    public int SetZombHit { get { return zomHits; } set { zomHits = value; } }
    public void GameStart()
    {
        //zombie list initiated here due to need in the function below
        zombieList = new List<GameObject>();
        //while on the server and is a client get the player
        if (netManager.IsClient)
        {
            GameObject pl = Instantiate(playerPrefab);
            pl.GetComponent<NetworkObject>().Spawn(true);
            //if there is a player set up the scene
            if (pl.activeSelf)
            {
                player = FindObjectOfType<Player>();
                player.GetFrameDelay = framedelay;
                player.ArtificalDelay = artDelay;
                Debug.Log("this player was found in the scene");
            }
            //spawn in a zombie on screen
            InstantiateZombie();
            started = true;
        }
        //get the text on screen
        var gm = GameObject.FindGameObjectWithTag("Timer");
        Debug.Log("Varname"+ gm);
        Text = gm.GetComponent<TMP_Text>();
        Debug.Log("Varname" + Text);
        var sm = GameObject.FindGameObjectWithTag("Score");
        scoreText = sm.GetComponent<TMP_Text>();
        scoreText.text = "Score: " + score.ToString();

        //dictionaries and lists
        GameStateDic = new Dictionary<int, ManagerState>();
        zombieDicList = new Dictionary<int, List<GameObject>>();
        bulletDicList = new Dictionary<int, List<GameObject>>();
        bulletList = new List<GameObject>();
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
            score += 20;
            scoreText.text = "Score: " + score.ToString();
        }
       
    }

    public void Deload()
    {       
        GameStateDic.Clear();
        bulletDicList.Clear();
        zombieDicList.Clear();
    }
    public void JustATest(int ticks)
    {
        //check same number of zombies
        
        if (GameStateDic.ContainsKey(ticks))
        {
            var t = GameStateDic[ticks];          
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
                        if (zombieList[i] != null)
                        {
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
                        if (item != null)
                        {
                            item.GetComponent<ZombieScript>().ResetPosition(ticks);
                        }
                       
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
                    if (item != null)
                    {
                        var zom = item.GetComponent<ZombieScript>();
                        if (zom.GetfirstTick() >= ticks)
                        {
                            zom.FullReset();
                        }
                        else
                        {
                            zom.ResetPosition(ticks);
                        }
                    }
                  
                }

            }

            var bl= bulletDicList[ticks];
            if (bl.Count > 0)
            {
                foreach (var item in bl)
                {
                    if (item != null)
                    {
                        item.GetComponent<BulletScript>().BulletRollback(ticks);
                    }
                }
            }
            //bullets
            
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
        var t = player.ProjectileSpawn();
        bulletsSpawned++;
        bulletList.Add(t);
        var forward = t.transform.position + (t.transform.forward * 5);
        AudioSource.PlayClipAtPoint(bulletspawn, forward);
    }
    public void BulletRemove(GameObject game,int ht)
    {
        bulletList.Remove(game);     
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

        if (spawnRunningZombie >= 2)
        {
            zm.GetComponent<ZombieScript>().GetRunner = true;
            spawnRunningZombie = 0;
        }
        else
        {
            spawnRunningZombie++;
        }
        zombieList.Add(zm);
        AudioSource.PlayClipAtPoint(Zombspawn, zm.transform.position);
    }
    // Update is called once per frame
    void Update()
    {
        if (started)
        {
            Debug.Log("JustForNow" + spawnRunningZombie);
            tickDeltaTime += Time.deltaTime;
            TimerFunction();
            while (tickDeltaTime > tickRate)
            {
                if (player == null)
                {
                    Debug.Log("the player is dead boyoy");
                    NetworkManager.Singleton.Shutdown();
                    NetworkManager networkManager = GameObject.FindObjectOfType<NetworkManager>();
                    DataSet();
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

           
          
            if (timer <= 0.0f)
            {
                player = null;
            }

        }

    }

    private void DataSet()
    {
        var f = (float)bulletsSpawned;
        var t = (zomHits / f) * 100.0f;
        switch (level)
        {
            case 1:
               
                Debug.Log("t varibble is" + t);
                DataStorage.Level1StatsCreation(deaths, score,t);
                break;
            case 2:
                DataStorage.Level2StatsCreation(deaths, score,  t);
                break;
            case 3:
                DataStorage.Level3StatsCreation(deaths, score, t);
                break;         
        }
    }

    public Vector3 GetSpawnPosition()
    {
        deaths++;
        score -= 50;
        scoreText.text = "Score: " + score.ToString();
        var d = Vector3.Distance(player.transform.position, Deathpoints[0].position);
        var d2 = Vector3.Distance(player.transform.position, Deathpoints[1].position);
        if (d > d2)
        {
            Debug.Log("dist1 is higher");
            return Deathpoints[0].position;
        }
        else
        {
            Debug.Log("dist2 is higher");
            return Deathpoints[1].position;           
        }       
    }

    private void TimerFunction()
    {
        timer -= Time.deltaTime;
        var ms = Mathf.Floor(timer / 60);
        int ts = (int)timer % 60;
        Text.text = "Time Left: " + ms.ToString() + "." + ts.ToString() + "s";
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
            if (maxZombies < maxNumberZombies)
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
        bulletDicList.Add(tick,bulletList);
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
            if (bulletDicList.ContainsKey(t))
            {
                bulletDicList.Remove(t);
            }
        }       
    }
}
