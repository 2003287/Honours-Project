using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using FixMath.NET;
using UnityEngine;


namespace GameMovement.Network
{
   
    //basics of prediction and recoconiliation used as guidance for networking 
    //Carl Boisvert Dev (2022), Unity Netcode For Gameobject - Client Prediction, available at: https://www.youtube.com/watch?v=leL6MdkJEaE
    //accessed on: 27th February 2023

    //basics of prediction used as guidance for networking 
    //Carl Boisvert Dev (2023), Unity Netcode For Gameobject - Client Reconiliation, available at: https://www.youtube.com/watch?v=3SILT4-vZWE
    //accessed on: 27th February 2023


    //number notation and guidance for rollback 
    // Zacpeelyates (2022),unityrollback, available at:https://github.com/zacpeelyates/unityrollback
    //accessed on 15th february 2023


    //implementation of rollback taking GGPO and implementing it in C#
    //Github Inc 2022 HouraiTeahouse Backroll(2021) available at: https://github.com/HouraiTeahouse/Backroll
    //(Accessed: 28 Novemeber 2022)
    public class NetworkMovementClass : NetworkBehaviour
    {
        [SerializeField]
        //character controller
        CharacterController PLayerMovement;

        //size of containers
        private const int Buffer_size = 16;

        //player in teh level
        [SerializeField]
        Player player;

        //tick system
        TickSystem ticks;
        
      //speed the player travels at 
        private float speed = 5.0f;
        //helperfunctions
        FixedVec2 rotation;

        //velocity
       
        FixedVec3 jumpvelocity;

        //manager
        Manager gameManager;

      
        //the last state
        public PlayerData oldMovementState;
        private Dictionary<int, PlayerData> gameStatesDic;

        //how may frames the project will need to rollback
        private static uint rollback = 0;
        //next to do store states into the array or dictionary
        Playpostest PlayerPositionTest;
        //camera
        new private Camera camera;
        //varibles used during the class
        private int deadtick = 0;
        private bool playerDead = false;
        private bool firing;
      
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            WakeupVaribles();
        }

        //basically start that occurs during a call by the server
        public void WakeupVaribles()
        {
            //set the camera
            camera = GameObject.FindGameObjectWithTag("Camera").GetComponent<Camera>();
          
            //create the dictionary
            gameStatesDic = new Dictionary<int, PlayerData>();
           
            //get the game manager
            gameManager = FindObjectOfType<Manager>();
            
            //create the old movement state
            oldMovementState = new PlayerData()
            {
                _position = new FixedVec2(0, 0),
                _direction = Direction.Idle,                
                _id = 0,
                _playerstate = MovementState.Moving,

            };
            //set varibles up
            firing = false;
            rotation = new FixedVec2(0.0f, 0.0f);
            jumpvelocity = new FixedVec3(0,0,0);
            ticks = new TickSystem(0,0);
        }
        
        //for destuction may be obsolete leaving just incase
        public void PLsDestroy()
        {
            gameStatesDic.Clear();
            gameManager.Deload();          

        }
        void Update()
        {
            
            ticks.tickDeltaTime += Time.deltaTime;
            
            //update for the new frame
            while (ticks.tickDeltaTime > ticks.tickRate)
            {
                //check the player is host
                if (IsServer)
                {
                    //while the player is aive
                    if (!playerDead)
                    {
                        //check if rollback should happen
                        if (rollback <= 0)
                        {
                            //move player and save predicted state
                            Moveplayer( oldMovementState);

                            SavePredictedGamestate(ticks.tick);
                        }
                        else
                        {
                            //rollback and seve the next state
                            Rollback();
                            
                            SavePredictedGamestate(ticks.tick);
                        }
                    }
                    else
                    {
                        //when the player is dead stop movement and rollback to give them invincibility frames
                        var deadcheck = deadtick + 12;
                        if (deadcheck < ticks.tick)
                        {
                            playerDead = false;
                        }
                        SavePredictedGamestate(ticks.tick);
                    }
                    
                    //if there is a player find the player
                    if (player != null)
                    {
                        //respawn the player when dead
                        if (!player.GetAlive)
                        {
                            var ts = gameManager.GetSpawnPosition();
                            PLayerMovement.enabled = false;
                            PLayerMovement.transform.position = ts;
                            PLayerMovement.enabled = true;
                            player.PlayerReset();
                            playerDead = true;
                            deadtick = ticks.tick;
                        }
                    }
                    else
                    {
                        //find the player if they are null
                        player = FindObjectOfType<Player>();
                    }
                    
                    //reset tick and add on
                    ticks.tickDeltaTime -= ticks.tickRate;
                    ticks.tick++;

                    //update buffer to remove the earliest state
                    if (ticks.tick >= 16)
                    {
                        int remove = ticks.tick - Buffer_size;
                        gameStatesDic.Remove(remove);                      

                    }
                }
            }
        }
      

        //move the player
        private Vector3 Moveplayer( PlayerData test)
        {         
            //get the movement vecter and apply it //converting a fixed point to a vector after calculation
            Vector3 movement = NetworkHelper.CameraMovement(camera.transform.right,camera.transform.forward,test._position);
            PLayerMovement.Move(movement * speed * Time.deltaTime);
       
            //when grounded reset the y position
            if (jumpvelocity.yfix < 0 && PlayerPositionTest._grounded)
            {
                jumpvelocity.yfix = (Fix64)(-1);
            }

            //apply gravity to the player
            jumpvelocity.yfix -= (Fix64)(9.81*2.0* Time.deltaTime);
            //jump the player only happens when jumping
            var jump = new Vector3(0,(float)jumpvelocity.yfix,0);
          
            PLayerMovement.Move(jump * Time.deltaTime);
            //apply rotation so long as the player is not reloading
            if (test._playerstate != MovementState.Reloading)
            {
                RotationMovement();
            }
            
            return movement;
        }

        //Rotate the player
        private void RotationMovement()
        {
            //gather the rotation from the players rotation
            rotation = NetworkHelper.PlayerRotationVoid(rotation,PlayerPositionTest._rotation);
            //apply the rotation to the player
            PLayerMovement.transform.localRotation = Quaternion.Euler((float)rotation.xpos, (float)rotation.ypos, 0f);
        }

        //asve the state recieved from the player
        private void SaveState(PlayerData test,  int buffer)
        {
            
            //check the array           
            if (gameStatesDic.ContainsKey(test._id))
            {
                //get the predicted state
                PlayerData testingdat = gameStatesDic[test._id];
                //check the varibles are the same
                //if not find the number of frames of Rollback
                if (!test.Equals(testingdat))
                {
                    rollback = (uint)(ticks.tick - test._id);                              
                }
            }
            //store the state and update the last known state
            gameStatesDic[buffer] = test;           
            oldMovementState = test;           
            
        }

        //Rollback is applied
        private void Rollback()
        {
            
            //moving stat
            if (oldMovementState._playerstate == MovementState.Moving)
            {
                //reset the players position
                PLayerMovement.enabled = false;
                PLayerMovement.transform.position = NetworkHelper.FixToVec3(PlayerPositionTest._playerpos);
                PLayerMovement.enabled = true;
                for (int i = (int)rollback; i > 0; i--)
                {
                    var t = ticks.tick - i;
                    //move the player for every incorrect predicted state
                    Moveplayer( oldMovementState);
                   
                    //save the new prediction
                    SavePredictedGamestate(t);
                }
            }           
            else if (oldMovementState._playerstate == MovementState.Attacking)
            {
                //check if the direction has changed
                if (oldMovementState._direction != gameStatesDic[oldMovementState._id]._direction)
                {
                    PLayerMovement.enabled = false;
                    PLayerMovement.transform.position = NetworkHelper.FixToVec3(PlayerPositionTest._playerpos);
                    PLayerMovement.enabled = true;
                    for (int i = (int)rollback; i > 0; i--)
                    {
                        var t = ticks.tick - i;
                        //move the player for every incorrect predicted state
                        Moveplayer(oldMovementState);

                        //save the new prediction
                        SavePredictedGamestate(t);
                    }
                }
               
               //fire out a projectile
                if (!firing)
                {
                    firing = true;
                    gameManager.SpawnProjectile();                   
                }
               
            }
            else if (oldMovementState._playerstate == MovementState.Reloading)
            {
                //reset the position of the player
                PLayerMovement.enabled = false;
                PLayerMovement.transform.position = NetworkHelper.FixToVec3(PlayerPositionTest._playerpos);
                PLayerMovement.enabled = true;
              
                //update predicted states to Idle
                for (int i = (int)rollback; i > 0; i--)
                {
                    var t = ticks.tick - i;                          
                    
                    SavePredictedGamestate(t);
                }
            }
            else if (oldMovementState._playerstate == MovementState.jumping)
            {
                //reset position
                PLayerMovement.enabled = false;
                PLayerMovement.transform.position = NetworkHelper.FixToVec3(PlayerPositionTest._playerpos);
                PLayerMovement.enabled = true;
                
                //create jump vector
                jumpvelocity = NetworkHelper.JumpingFix(jumpvelocity,10);
                //reaply movement and save teh states
                for (int i = (int)rollback; i > 0; i--)
                {
                    var t = ticks.tick - i;

                    Moveplayer( oldMovementState);                  
                    SavePredictedGamestate(t);
                }
                //the player is jumping
                gameManager.PLayerJumping();
            }

            //apply Rollback to game manager adn its elements
            gameManager.JustATest(ticks.tick - (int)rollback);
            //apply the movement here
            //create a list or something else          
            rollback = 0;
        }

        //the player is allowed to fire a new bullet
        public void CanFireAgain()
        {
            if (firing)
            {
                firing = false;               
            }
        }
        //save the predicted state and update last known state
        private void SavePredictedGamestate(int t)
        {           
            oldMovementState._id = t;
            gameStatesDic[t] = oldMovementState;
        }

        //send packets to the server
        public void ProcessPlayerMoevement( PlayerData test, Playpostest play)
        {              
                if (IsServer)
                {                       
                  SaveState(test,test._id);
                  PlayerPositionTest = play;
                }               
        }

        //get the servers tick
        public int ReturnTick()
        {
            return ticks.tick;
        }
        
        //Create a direction for the player
        public Direction ProcessPlayerDirection(float xpos, float ypos)
        {
            var xmove = NetworkHelper.FloatToInt(xpos);
            var ymove = NetworkHelper.FloatToInt(ypos);
            int v = 5 + xmove + (3 * ymove);
            Direction d = (Direction)v;           
            return d;
        }

    }
}
