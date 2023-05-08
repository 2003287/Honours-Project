using System.Collections;
using System.Collections.Generic;
using FixMath.NET;
using Unity.Netcode;
using UnityEngine;

namespace GameMovement.Network
{

    //Unity, 2023, Custom Serialization,
    //Available at https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/custom-serialization
    //Accessed on: (29 March 2023)
    //makes Fixed points seralisable across the network
    public static class SerializationFix64
    {
        public static void ReadValueSafe(this FastBufferReader reader, out Fix64 url)
        {
            reader.ReadValueSafe(out Fix64 val);
            url = val;
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in Fix64 url)
        {
            writer.WriteValueSafe(url.RawValue);
        }
    }
    //makes allowances to make the Fixed Vector 2 seralisable
    public static class SerializationVec264
    {
        public static void ReadValueSafe(this FastBufferReader reader, out FixedVec2 url)
        {
            reader.ReadValueSafe(out FixedVec2 val);
            url = val;
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in FixedVec2 url)
        {
            writer.WriteValueSafe(url);
        }
    }

    //makes allowances to make the Fixed Vector 3 seralisable across the network
    public static class SerializationVec364
    {
        public static void ReadValueSafe(this FastBufferReader reader, out FixedVec3 url)
        {
            reader.ReadValueSafe(out FixedVec3 val);
            url = val;
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in FixedVec3 url)
        {
            writer.WriteValueSafe(url);
        }
    }
    public struct Playpostest : INetworkSerializable
    {
        //the players position
        //public Vector3 _playerpos;
        public FixedVec3 _playerpos;
        //players Rotation
        public FixedVec2 _rotation;
        //is player Grounded
        public bool _grounded;
        //makes tehm seralisable
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
        //the sate of the enemy
       public Enemystate _enemystate;
        //position of the zombie, used internally so floats are detministic
       public  Vector3 _position;
        //the tick identifier
       public int _id;
        //speed teh zombie is treaveling at
       public float _speed;
        //timer incase paused or waiting
        public float _timer;
        //zombie is alive 
        public bool _alive;
        //makes them seralisable
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

    //packet for teh player 
    public struct PlayerData : INetworkSerializable
    {
        //player position
        public FixedVec2 _position;
        //Direction
        public Direction _direction;      
        //the current tick of the game
        public int _id;
        //player movement state
        public MovementState _playerstate;    
       
        //makes the packet seralisable
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out _position);
                reader.ReadValueSafe(out _direction);               
                reader.ReadValueSafe(out _id);             
                reader.ReadValueSafe(out _playerstate);                    
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(_position);
                writer.WriteValueSafe(_direction);               
                writer.WriteValueSafe(_id);               
                writer.WriteValueSafe(_playerstate);              
            }
        }
    }

    //containers for vector 2s into fixed points
    public struct FixedVec2 : INetworkSerializable
    {
       public Fix64 xpos;
       public Fix64 ypos;

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

        //make them seralisable
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out xpos);
                reader.ReadValueSafe(out ypos);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(xpos);
                writer.WriteValueSafe(ypos);
            }
        }
    }

    //public fixed vector 3 storage for vector 3s
    public struct FixedVec3 : INetworkSerializable
    {
        public Fix64 xfix;
        public Fix64 yfix;
        public Fix64 zfix;

        public FixedVec3(Fix64 x, Fix64 y,Fix64 z)
        {
            xfix = x;
            yfix = y;
            zfix = z;
        }

        public FixedVec3(float x, float y, float z)
        {
            xfix = (Fix64)x;
            yfix = (Fix64)y;
            zfix = (Fix64)z;
        }

        //make them seralisable
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out xfix);
                reader.ReadValueSafe(out yfix);
                reader.ReadValueSafe(out zfix);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(xfix);
                writer.WriteValueSafe(yfix);
                writer.WriteValueSafe(zfix);
            }
        }
    }
    //player movement states
    public enum MovementState {Moving,Attacking,Reloading,jumping};
    //player directions
    public enum Direction {Unknown,SouthWest,South,SouthEast,West,Idle,East,NorthWest,North,NorthEast};

    //managerState struct
    public struct ManagerState : INetworkSerializable
    {

        public int _allowance;
        //how long left on the spawn timer
        public float _spawntimer;
        //number of kills till increase spawn amount
        public int _killedZombiesReset;
        //number of zombies on screen
        public int _zombnumber;
        //number of zombies killed
        public int _zombKilled;

        //network seralisation for each of the varibles
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
    protected float normMovementSpeed = 2.0f;

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
    protected float AmmoCount = 12.0f;

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

   
}
