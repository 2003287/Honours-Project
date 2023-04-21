using System.Collections;
using System.Collections.Generic;
using FixMath.NET;
using Unity.Netcode;
using UnityEngine;

namespace GameMovement.Network
{

    public struct Playpostest : INetworkSerializable
    {
        //the players position
        public Vector3 _playerpos;
        //players Rotation
        public Vector2 _rotation;
        //is player Grounded
        public bool _grounded;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
              
                reader.ReadValueSafe(out _playerpos);
                reader.ReadValueSafe(out _rotation);
                reader.ReadValueSafe(out _grounded);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
               
                writer.WriteValueSafe(_playerpos);
                writer.WriteValueSafe(_rotation);
                writer.WriteValueSafe(_grounded);
            }
        }
    }

    //Zombie Rollback Container
    public struct ZombieData : INetworkSerializable
    {

       public Enemystate _enemystate;
       public  Vector3 _position;
       public int _id;
       public float _speed;
        public float _timer;
        public bool _alive;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();

                reader.ReadValueSafe(out _enemystate);
                reader.ReadValueSafe(out _position);
                reader.ReadValueSafe(out _id);
                reader.ReadValueSafe(out _speed);
                reader.ReadValueSafe(out _timer);
                reader.ReadValueSafe(out _alive);

            }
            else
            {
                var writer = serializer.GetFastBufferWriter();

                writer.WriteValueSafe(_enemystate);
                writer.WriteValueSafe(_position);
                writer.WriteValueSafe(_id);
                writer.WriteValueSafe(_speed);
                writer.WriteValueSafe(_timer);
                writer.WriteValueSafe(_alive);

            }
        }
    }

    //zombie Enum
    public enum Enemystate { Attacking, Running, Paused };

    public struct TestData : INetworkSerializable
    {
        //player position
        public Vector2 _position;
        //Direction
        public Direction _direction;
        //not used will be for firing
        public bool _attacking;
        //the current tick of the game
        public int _id;
        //player movement state
        public MovementState _playerstate;
        //is the player grounded
        //public bool _grounded;
       
     
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out _position);
                reader.ReadValueSafe(out _direction);
                reader.ReadValueSafe(out _attacking);
                reader.ReadValueSafe(out _id);             
                reader.ReadValueSafe(out _playerstate);             
               // reader.ReadValueSafe(out _grounded);             
                           
                          
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(_position);
                writer.WriteValueSafe(_direction);
                writer.WriteValueSafe(_attacking);
                writer.WriteValueSafe(_id);               
                writer.WriteValueSafe(_playerstate);               
               // writer.WriteValueSafe(_grounded);               
               
               
            }
        }
    }


    public struct FixedVec2
    {
        Fix64 xpos;
        Fix64 ypos;

        public FixedVec2(Fix64 x,Fix64 y)
        {
            xpos = x;
            ypos = y;
        }

        public FixedVec2(float x, float y)
        {
            xpos = (Fix64)x;
            ypos = (Fix64)y;
        }
    }
    public enum MovementState {Moving,Attacking,Reloading,jumping};
    public enum Direction {Unknown,SouthWest,South,SouthEast,West,Idle,East,NorthWest,North,NorthEast};

    public struct ManagerState : INetworkSerializable
    {
        public int _allowance;
        public float _spawntimer;
        public int _killedZombiesReset;
        public int _zombnumber;
        public int _zombKilled;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out _allowance);
                reader.ReadValueSafe(out _spawntimer);
                reader.ReadValueSafe(out _killedZombiesReset);                
                reader.ReadValueSafe(out _zombnumber);                
                reader.ReadValueSafe(out _zombKilled);                
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(_allowance);
                writer.WriteValueSafe(_spawntimer);
                writer.WriteValueSafe(_killedZombiesReset);
                writer.WriteValueSafe(_zombnumber);       
                writer.WriteValueSafe(_zombKilled);       

            }
        }
    }

}

public class CahracterBase : NetworkBehaviour
{
    //speed the character moves at
    [SerializeField]
    protected float normMovementSpeed = 1.0f;

    //speed character runs at
    [SerializeField]
    protected float runningMovementSpeed = 5.0f;

    //the amount of health that the object has
    [SerializeField]
     protected float HealthPoint = 100.0f;

    //fi the character is still alive or not
    [SerializeField]
    protected bool alive;

    //the speed that there projectile fires at
    [SerializeField]
    protected float ProjectileSpeed = 1.0f;

    //ammunition count for the character
    [SerializeField]
    protected float AmmoCount = 30.0f;

    [SerializeField]
    protected bool reloading;

    //Fired a projectile
    [SerializeField]
    bool FiredProjectile;

    //projectile
    [SerializeField]
    protected GameObject projectile;

    [SerializeField]
    protected float ammotimer;
    //position of the gun
    [SerializeField]
    protected GameObject gunpos;

   
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
