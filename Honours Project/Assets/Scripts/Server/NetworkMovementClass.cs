using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace GameMovement.Network
{
    ///https://github.com/zacpeelyates/unityrollback
     //implementation of rollback taking GGPO and implementing it in C#
    //Github Inc 2022 HouraiTeahouse Backroll(2021) available at: https://github.com/HouraiTeahouse/Backroll
    //(Accessed: 28 Novemeber 2022)
    public class NetworkMovementClass : NetworkBehaviour
    {
        [SerializeField]
        //character controller
        CharacterController PLayerMovement;
        private const int Buffer_size = 16;

        [SerializeField]
        Player player;
        //tickrate /// for id of each msg 
        private int tick = 0;
        private float tickRate = 1.0f / 60.0f;
        private float tickDeltaTime = 0f;
        private float speed = 5.0f;
        //helperfunctions
        FixedVec2 rotation;

        //velocity
        Vector3 velocity;

        //manager
        Manager gameManager;

        //used by the network
       // public NetworkVariable<TestData> ServermovemntState = new NetworkVariable<TestData>();
        //the last state
        public PlayerData oldMovementState;
        private Dictionary<int, PlayerData> gameStatesDic;

        //how may frames the project will need to rollback
        private static uint rollback = 0;
        //next to do store states into the array or dictionary
        Playpostest PlayerPositionTest;
        //camera
        new private Camera camera;
        private int deadtick = 0;
        private bool playerDead = false;
        private bool firing;
        private void OnEnable()
        {
           
            gameStatesDic = new Dictionary<int, PlayerData>();
            oldMovementState = new PlayerData()
            {
                _position = new FixedVec2(0, 0),
                _direction = Direction.Idle,               
                _id = 0,
                _playerstate = MovementState.Moving,
               
            };
            rotation = new FixedVec2(0.0f,0.0f);
           
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            HelpMe();
        }

        public void HelpMe()
        {
            tick = 0;
            camera = GameObject.FindGameObjectWithTag("Camera").GetComponent<Camera>();
            
            firing = false;
            gameManager = FindObjectOfType<Manager>();
            oldMovementState = new PlayerData()
            {
                _position = new FixedVec2(0, 0),
                _direction = Direction.Idle,                
                _id = 0,
                _playerstate = MovementState.Moving,

            };
            rotation = new FixedVec2(0.0f, 0.0f);
        }
        //Update to control movement

        public void PLsDestroy()
        {
            gameStatesDic.Clear();
            gameManager.Deload();          

        }
        void Update()
        {
            
            tickDeltaTime += Time.deltaTime;
            //  Debug.Log(tickDeltaTime);
            while (tickDeltaTime > tickRate)
            {
                if (IsServer)
                {
                    if (!playerDead)
                    {
                        if (rollback <= 0)
                        {
                            Moveplayer( oldMovementState);

                            SavePredictedGamestate(tick);
                        }
                        else
                        {
                            Rollback();
                            //Debug.Log("testing rollback has happened");
                            SavePredictedGamestate(tick);
                        }
                    }
                    else
                    {
                        var deadcheck = deadtick + 12;
                        if (deadcheck < tick)
                        {
                            playerDead = false;
                        }
                        SavePredictedGamestate(tick);
                    }
                    

                    if (player != null)
                    {
                        if (!player.GetAlive)
                        {
                            var ts = gameManager.GetSpawnPosition();
                            PLayerMovement.enabled = false;
                            PLayerMovement.transform.position = ts;
                            PLayerMovement.enabled = true;
                            player.PlayerReset();
                            playerDead = true;
                            deadtick = tick;
                        }
                    }
                    else
                    {
                        player = FindObjectOfType<Player>();
                    }
                    
                    //reset tick and add on
                    tickDeltaTime -= tickRate;
                    tick++;
                    if (tick >= 16)
                    {
                        int remove = tick - Buffer_size;
                        gameStatesDic.Remove(remove);                      

                    }
                }
            }
        }
      

        
        private Vector3 Moveplayer( PlayerData test)
        {         
           // Vector3 movement = camera.transform.right * (float)test._position.xpos + camera.transform.forward * (float)test._position.ypos;
            Vector3 movement = NetworkHelper.CameraMovement(camera.transform.right,camera.transform.forward,test._position);
            PLayerMovement.Move(movement * speed * Time.deltaTime);
            if (velocity.y < 0 && PlayerPositionTest._grounded)
            {
                velocity.y = -1.0f;                
            }

            
            velocity.y -= 9.81f * 2.0f * Time.deltaTime;
            PLayerMovement.Move(velocity * Time.deltaTime);
            if (test._playerstate != MovementState.Reloading)
            {
                RotationMovement();
            }
            
            return movement;
        }

        private void RotationMovement()
        {
            rotation = NetworkHelper.PlayerRotationVoid(rotation,PlayerPositionTest._rotation);
            PLayerMovement.transform.localRotation = Quaternion.Euler((float)rotation.xpos, (float)rotation.ypos, 0f);
        }

        private void SaveState(PlayerData test,  int buffer)
        {
            
            //check the array           
            if (gameStatesDic.ContainsKey(test._id))
            {
                PlayerData testingdat = gameStatesDic[test._id];
                if (!test.Equals(testingdat))
                {
                    rollback = (uint)(tick - test._id);
                    Debug.Log("rollback needed" + rollback);            
                }
            }
            
            gameStatesDic[buffer] = test;           
            oldMovementState = test;           
            
        }


        private void Rollback()
        {
            gameManager.JustATest(tick - (int)rollback);
            PLayerMovement.enabled = false;
            PLayerMovement.transform.position = NetworkHelper.FixToVec3(PlayerPositionTest._playerpos);
            PLayerMovement.enabled = true;
            if (oldMovementState._playerstate == MovementState.Moving)
            {
              
                for (int i = (int)rollback; i > 0; i--)
                {
                    var t = tick - i;

                    Moveplayer( oldMovementState);
                    Debug.Log("direction" + oldMovementState._direction);
                    Debug.Log("new position check" + PLayerMovement.transform.position);
                    //save the new prediction
                    SavePredictedGamestate(t);
                }
            }
            else if (oldMovementState._playerstate == MovementState.Attacking)
            {
                Debug.Log("Im firing out a bullet");
                if (!firing)
                {
                    firing = true;
                    gameManager.TestingSwan();                   
                }
                
            }
            else if (oldMovementState._playerstate == MovementState.Reloading)
            {
               
               
                //oldMovementState._position = new FixedVec2(0, 0);
                //oldMovementState._direction = Direction.Idle;
                for (int i = (int)rollback; i > 0; i--)
                {
                    var t = tick - i;                          
                    
                    SavePredictedGamestate(t);
                }
            }
            else if (oldMovementState._playerstate == MovementState.jumping)
            {
               velocity = NetworkHelper.Jumping(velocity);
               
                for (int i = (int)rollback; i > 0; i--)
                {
                    var t = tick - i;

                    Moveplayer( oldMovementState);                  
                    SavePredictedGamestate(t);
                }
                Debug.Log("Jumpig2");
                gameManager.PLayerJumping();
            }
            //apply the movement here
            //create a list or something else          
            rollback = 0;
        }

        public void CanFireAgain()
        {
            if (firing)
            {
                firing = false;
                Debug.Log("I can now fire another bullet");
            }
        }
        private void SavePredictedGamestate(int t)
        {           
            oldMovementState._id = t;
            gameStatesDic[t] = oldMovementState;
        }
        public void ProcessPlayerMoevement( PlayerData test, Playpostest play)
        {              
                if (IsServer)
                {                       
                  SaveState(test,test._id);
                  PlayerPositionTest = play;
                }               
        }


        public int ReturnTick()
        {
            return tick;
        }
        
        //taken from here
        public Direction ProcessPlayerDirection(float xpos, float ypos)
        {
            var xmove = NetworkHelper.Testing(xpos);
            var ymove = NetworkHelper.Testing(ypos);
            int v = 5 + xmove + (3 * ymove);
            Direction d = (Direction)v;           
            return d;
        }

    }
}
