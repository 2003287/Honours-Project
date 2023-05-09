using System.Collections;
using System.Collections.Generic;
using GameMovement.Network;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;

//background music for the level
//Aussens@iter ft kara Square, 2018, Ukulele Space Metal (Instrumental),
//Available at: https://dig.ccmixter.org/files/tobias_weber/57791, Accessed on: 15/4/23. 
//Ukulele Space Metal (Instrumental) by Aussens@iter (c) copyright 2018 Licensed under a Creative Commons Attribution Noncommercial
//(3.0) license.

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
    public int GetFrameDelay => framedelay;
    //addition of randomised delay
    [SerializeField]
    bool artDelay;
    //dictionarycontainers
    private Dictionary<int, List<GameObject>> zombieDicList;
    private Dictionary<int, List<GameObject>> bulletDicList;
    private Dictionary<int, ManagerState> GameStateDic;

    //tick class
    TickSystem ticks;
    //private int tick = 0;    
    //private float tickRate = 1.0f / 60.0f;
   // private float tickDeltaTime = 0f;

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
            }
            //spawn in a zombie on screen
            InstantiateZombie();
            started = true;
        }
        //get the text on screen
        var gm = GameObject.FindGameObjectWithTag("Timer");
     
        Text = gm.GetComponent<TMP_Text>();
       
        var sm = GameObject.FindGameObjectWithTag("Score");
        scoreText = sm.GetComponent<TMP_Text>();
        scoreText.text = "Score: " + score.ToString();

        //dictionaries and lists
        GameStateDic = new Dictionary<int, ManagerState>();
        zombieDicList = new Dictionary<int, List<GameObject>>();
        bulletDicList = new Dictionary<int, List<GameObject>>();
        bulletList = new List<GameObject>();

        ticks = new TickSystem(0,0);
    }
    //Remove a zombie from the list
    public void RemoveFromList(GameObject prefab)
    {
        if (deathallowed)
        {
            //remove the zombie
            deathallowed = false;
            tickdeath = ticks.tick;
            if (zombieList.Contains(prefab))
            {
                zombieList.Remove(prefab);
                Debug.Log("Removed");
            }
            //adapt the varibles
            killedZombies++;
            numberOfZombies--;
            totalKilledZombs++;
            //update score
            score += 20;
            scoreText.text = "Score: " + score.ToString();
        }
       
    }

    //clear all the lists and dictionary
    public void Deload()
    {       
        GameStateDic.Clear();
        bulletDicList.Clear();
        zombieDicList.Clear();
    }
    public void JustATest(int tick)
    {
        //check same number of zombies
        
        if (GameStateDic.ContainsKey(tick))
        {
            var t = GameStateDic[tick];          
            var testing = zombieDicList[ticks.tick-1];
            if (zombieDicList.ContainsKey(tick))
            {
                testing = zombieDicList[tick];
            }
            //Same number of zombies
            if (t._zombnumber == numberOfZombies)
            {              
                //check number of kills are the same
                //if so wonderful and rollback
                if (t._zombKilled == totalKilledZombs)
                {
                    foreach (var item in zombieList)
                    {                        
                        if (item != null)
                        {
                            //aply rollback to the zombie
                            item.GetComponent<ZombieScript>().ResetPosition(tick);
                        }
                        
                    }
                }
                else
                {
                    //apply rollback to all the zombies
                    for (int i = 0; i < zombieList.Count; i++)
                    {
                        var zc = zombieList.Count - 1;
                        if (zombieList[i] != null)
                        {
                            if (i != zc)
                            {

                                zombieList[i].GetComponent<ZombieScript>().ResetPosition(tick);
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
                //has never happened but left in just incase
                if (t._zombKilled == totalKilledZombs)
                {
                    Debug.Log("the zombie died this frame and hasn't been accounted for yet");
                    //do nothing just ignore it until i ask about it
                }
                else
                {
                    //if rollback is more than two frames
                    var tcheck = ticks.tick - tick;
                    //reset the varibles in the level
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
                }
                //death inbetween states only apply rollback to alive zombies
                foreach (var item in testing)
                {
                    if (zombieList.Contains(item))
                    {
                        
                        if (item != null)
                        {
                            item.GetComponent<ZombieScript>().ResetPosition(tick);
                        }                       
                    }                             
                }
            }
            else
            {         
               //a new zombie has spawned in               
                foreach (var item in zombieList)
                {
                    if (item != null)
                    {
                        var zom = item.GetComponent<ZombieScript>();
                        //check if the first ick is after the frame of rollback
                        if (zom.GetfirstTick() >= tick)
                        {
                            //do all the frames inbetween but not the current frame
                            zom.FullReset();
                        }
                        else
                        {
                            //redo rollback for all predicted frames
                            zom.ResetPosition(tick);
                        }
                    }
                  
                }

            }
            //do rollback for the bullets
            var bl= bulletDicList[tick];
            //if there is a bullet apply rollback
            if (bl.Count > 0)
            {
                foreach (var item in bl)
                {
                    if (item != null)
                    {
                        item.GetComponent<BulletScript>().BulletRollback(tick);
                    }
                }
            }
            
            
        }             
    }

    //spawn in a projectile
    public void SpawnProjectile()
    {
        //spawn a bullet and add to list
        if (player != null)
        {
            var t = player.ProjectileSpawn();
            bulletsSpawned++;
            bulletList.Add(t);
            //play audio for the bullet fired
            var forward = t.transform.position + (t.transform.forward * 5);
            AudioSource.PlayClipAtPoint(bulletspawn, forward);
        }
       
    }
    //remove a bullet from the list
    public void BulletRemove(GameObject game,int ht)
    {
        bulletList.Remove(game);     
    }
    //fix the players jumping
    public void PLayerJumping()
    {
        player.FixJumping();
    }
    private void InstantiateZombie()
    {
        spawnedin = true;
        numberOfZombies++;
        //get the spawn position
        Transform basePos = NetworkHelper.GetClosestTransForm(spawnPoints,player);
       

        //spawn the zombie in
        GameObject zm = Instantiate(zombiePrefab,basePos.position, Quaternion.identity);
        //should the zombie spawned be a runner or not
        if (spawnRunningZombie >= 2)
        {
            zm.GetComponent<ZombieScript>().GetRunner = true;
            spawnRunningZombie = 0;
        }
        else
        {
            spawnRunningZombie++;
        }

        //add to the list of zombies and play audio where they spawned
        zombieList.Add(zm);
        AudioSource.PlayClipAtPoint(Zombspawn, zm.transform.position);
    }
    // Update is called once per frame
    void Update()
    {
        if (started)
        {
           //update the timer
            ticks.tickDeltaTime += Time.deltaTime;
            TimerFunction();
            while (ticks.tickDeltaTime > ticks.tickRate)
            {
                //end the level and transition to the home screen
                if (player == null)
                {
                    if (NetworkManager.Singleton != null)
                    {
                        NetworkManager.Singleton.Shutdown();
                    }
                    
                    NetworkManager networkManager = GameObject.FindObjectOfType<NetworkManager>();
                    DataSet();
                    if (networkManager != null)
                    {
                        Destroy(networkManager.gameObject);
                    }
                    SceneManager.LoadScene("MainMenu");
                }
                else
                {
                    //check if a zombie should spawn in
                    ZombieSpawning();
                    //save and delete the states
                    StateSaving();
                    DeletingStates();
                    //update the ticks
                    ticks.tick++;
                    //update the death allowed for two ticks
                    if (!deathallowed)
                    {
                        var t = tickdeath + 2;
                        if (ticks.tick > t)
                        {
                            deathallowed = true;
                            tickdeath = ticks.tick;
                        }
                    }
                }
                //update the tick rate
                ticks.tickDeltaTime -= ticks.tickRate;
            }

           
            //when the level ends nullify the player
            if (timer <= 0.0f)
            {
                player = null;
            }

        }

    }

    //update dataset
    private void DataSet()
    {
        //create the accuracy
        var f = (float)bulletsSpawned;
        var t = (zomHits / f) * 100.0f;
        //store the statistics for each level
        switch (level)
        {
            case 1:           
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
    //the player has died find where to spawn them
    public Vector3 GetSpawnPosition()
    {
        //update deaths and score
        deaths++;
        score -= 50;
        scoreText.text = "Score: " + score.ToString();
        //create distance 
        var d = Vector3.Distance(player.transform.position, Deathpoints[0].position);
        var d2 = Vector3.Distance(player.transform.position, Deathpoints[1].position);
        //check which position the player is further away from
        if (d > d2)
        {
            
            return Deathpoints[0].position;
        }
        else
        {
            
            return Deathpoints[1].position;           
        }       
    }

    //timer funstion used to display time left on screen
    private void TimerFunction()
    {
        timer -= Time.deltaTime;
        var ms = Mathf.Floor(timer / 60);
        int ts = (int)timer % 60;
        Text.text = "Time Left: " + ms.ToString() + "." + ts.ToString() + "s";
    }

    //functions for check number of zombies dead and if one should spawn in 
    private void ZombieSpawning()
    {
        //if there is not enough zombies on screen to meet the maximum
        if (zombieList.Count <= maxZombies)
        {
            //do a timer to spawn in new zombies
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= timeTillSpawn)
            {
                //spawn a sombie and reset the varibles
                if (!spawnedin)
                {
                    InstantiateZombie();
                }                
                spawnTimer = 0.0f;
                spawnedin = false;
            }
        }
        //update the max numbre of zombies on the screen every time 5 die
        if (killedZombies >= 5)
        {
            if (maxZombies < maxNumberZombies)
            {
                maxZombies++;
                killedZombies = 0;
            }
        }
    }

    //basic house keeping

    //save the states for the dictionaries and lists
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
        GameStateDic.Add(ticks.tick,ms);
        zombieDicList.Add(ticks.tick,zombieList);
        bulletDicList.Add(ticks.tick,bulletList);
    }

    //removing states from dictionaries
    private void DeletingStates()
    {
        if (ticks.tick >= framedelay)
        {
            var t = ticks.tick - framedelay;
            if (GameStateDic.ContainsKey(ticks.tick))
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
