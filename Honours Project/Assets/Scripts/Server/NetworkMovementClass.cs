using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace GameMovement.Network
{
    public class NetworkMovementClass : NetworkBehaviour
    {
        [SerializeField]
        //character controller
        CharacterController PLayerMovement;
        private const int Buffer_size = 16;


        //tickrate /// for id of each msg 
        private int tick = 0;
        private float tickRate = 1.0f / 60.0f;
        private float tickDeltaTime = 0f;

        //helperfunctions
        Vector2 rotation;

        //velocity
        Vector3 velocity;

        //manager
        Manager gameManager;

        //used by the network
       // public NetworkVariable<TestData> ServermovemntState = new NetworkVariable<TestData>();
        //the last state
        public TestData oldMovementState;
        private Dictionary<int, TestData> gameStatesDic;

        //how may frames the project will need to rollback
        private static uint rollback = 0;
        //next to do store states into the array or dictionary
        Playpostest PlayerPositionTest;
        //camera
        new private Camera camera;
        private float oldFloat;
        private bool firing;
        private void OnEnable()
        {
           
            gameStatesDic = new Dictionary<int, TestData>();
            oldMovementState = new TestData()
            {
                _position = new Vector2(0.0f, 0.0f),
                _direction = Direction.Idle,
                _attacking = false,
                _id = 0,
                _playerstate = MovementState.Moving,
               
            };
            rotation = new Vector2(0.0f,0.0f);
           
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
            oldMovementState = new TestData()
            {
                _position = new Vector2(0.0f, 0.0f),
                _direction = Direction.Idle,
                _attacking = false,
                _id = 0,
                _playerstate = MovementState.Moving,

            };
            rotation = new Vector2(0.0f, 0.0f);
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
                    if (rollback <= 0)
                    {
                        var move = Moveplayer(5.0f, oldMovementState);

                        SavePredictedGamestate(tick);
                    }
                    else
                    {
                        Rollback();
                        Debug.Log("testing rollback has happened");
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
      

        
        private Vector3 Moveplayer(float speed, TestData test)
        {
            // Vector2 tes = Vec2Creation(test._direction);
           // Debug.Log("test pos" + test._position);
            Vector3 movement = camera.transform.right * test._position.x + camera.transform.forward * test._position.y;
            PLayerMovement.Move(movement * speed * Time.deltaTime);
            if (velocity.y < 0 && PlayerPositionTest._grounded)
            {
                velocity.y = -1.0f;                
            }

            
            velocity.y -= 9.81f * 2.0f * Time.deltaTime;
            PLayerMovement.Move(velocity * Time.deltaTime);
            RotationMovement();
            return movement;
        }

        private void RotationMovement()
        {
            rotation = NetworkHelper.PlayerRotationVoid(rotation,PlayerPositionTest._rotation);
            PLayerMovement.transform.localRotation = Quaternion.Euler(rotation.x, rotation.y, 0f);
        }

        private void SaveState(TestData test,  int buffer)
        {
            Vector2 tes = NetworkHelper.Vec2Creation(test._direction);
        
            test._position = tes;
            TestData newstate = new TestData()
            {
                _position = tes,
                _direction = test._direction,
                _attacking = false,
                _id = buffer,
                _playerstate = test._playerstate
               
            };
            //add to the array
           
            if (gameStatesDic.ContainsKey(test._id))
            {
                TestData testingdat = gameStatesDic[test._id];
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
            if (oldMovementState._playerstate == MovementState.Moving)
            {
                Debug.Log("new position check" + PLayerMovement.transform.position);
                PLayerMovement.enabled = false;
                PLayerMovement.transform.position = PlayerPositionTest._playerpos;
                PLayerMovement.enabled = true;
                for (int i = (int)rollback; i > 0; i--)
                {
                    var t = tick - i;

                    var move = Moveplayer(5.0f, oldMovementState);
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
                Debug.Log("new position checkReload" +oldMovementState._direction);
                PLayerMovement.enabled = false;
                PLayerMovement.transform.position = PlayerPositionTest._playerpos;
                PLayerMovement.enabled = true;
                for (int i = (int)rollback; i > 0; i--)
                {
                    var t = tick - i;                          
                    
                    SavePredictedGamestate(t);
                }
            }
            else if (oldMovementState._playerstate == MovementState.jumping)
            {
               // Debug.Log("jumping has happened");
                
               velocity = NetworkHelper.Jumping(velocity);
                //Debug.Log("afterjump" +velocity);
                PLayerMovement.enabled = false;
                PLayerMovement.transform.position = PlayerPositionTest._playerpos;
                PLayerMovement.enabled = true;
                for (int i = (int)rollback; i > 0; i--)
                {
                    var t = tick - i;

                    var move = Moveplayer(5.0f, oldMovementState);                  
                    SavePredictedGamestate(t);
                }
                gameManager.PLayerJumping();
            }
            //apply the movement here
            //create a list or something else
            Debug.Log("rollback has happened");
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
            TestData newstate = new TestData()
            {
                _position = oldMovementState._position,
                _direction = oldMovementState._direction,
                _attacking = false,
                _id = t,         
                _playerstate = oldMovementState._playerstate,
            };

            gameStatesDic[t] = newstate;
        }
        public void ProcessPlayerMoevement(float speed, TestData test, Playpostest play)
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
