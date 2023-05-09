using System.Collections;
using System.Collections.Generic;
using GameMovement.Network;
using UnityEngine;
using FixMath.NET;

//tick system used by the manager, player, bullets, enemies and the network
public struct TickSystem
{
    //current tick
    public int tick;
    //first tick used by enemy and bullets
    public int firsttick;
    //tick rate of the server
    public float tickRate;
    //timer for tick system
    public float tickDeltaTime;
    public TickSystem(int t,int f) {
        tick = t;
        firsttick = f;
        tickRate = 1/60.0f;
        tickDeltaTime = 0;

    }
}

//stats Deaths, Kills and accuracy
public static class DataStorage
{
    //level One Stats
    public static bool level1com = false;
    public static int level1Death = 0;
    public static float level1Score = 0;   
    public static float accuracy1 = 0;

    //Level two Stats
    public static bool level2com = false;
    public static int level2Death = 0;
    public static float level2Score = 0.0f;    
    public static float accuracy2 = 0;

    //level three Stats
    public static bool level3com = false;
    public static int level3Death = 0;
    public static float level3Score = 0.0f;  
    public static float accuracy3 = 0;

    //set the stats for level 1
    public static void Level1StatsCreation(int death, float score,float f)
    {
        level1com = true;
        level1Death = death;
        level1Score = score;     
        accuracy1 = f;
      
    }
    //set the stats for level 2
    public static void Level2StatsCreation(int death, float score,  float f)
    {
        level2com = true;
        level2Death = death;
        level2Score = score;          
        accuracy2 = f;
       
    }
    //set the stats for level 3
    public static void Level3StatsCreation(int death, float score,  float f)
    {
        level3com = true;
        level3Death = death;
        level3Score = score;          
        accuracy3 =f;
       
    }


}
public static class NetworkHelper 
{
    //add rotation to the player
    public static FixedVec2 PlayerRotationVoid(FixedVec2 currot,FixedVec2 newrot)
    {
        //get the movements of the mouse
        
        //get the change in screenposition
        currot.xpos -=newrot.ypos;
        //clamp teh value when to far in one direction
        currot.xpos = (Fix64)Mathf.Clamp((float)currot.xpos, -90f, 90f);
     
        currot.ypos += newrot.xpos;
       
        //aplly this to the body and camera
        return currot;
    }

    //creates a int from floating points
    public static int FloatToInt(float tt)
    {
        int t = 0;
        if (tt != 0)
        {
            if (tt > 0)
            {
                t = 1;
            }
            else
            {
                t = -1;
            }
        }
        return t;
    }


    public static Transform GetClosestTransForm(List<Transform> spawnPoints,Player player)
    {
        int firstpos = 0;
        Transform basePos = spawnPoints[0];
        float distance = 0.0f;
        foreach (Transform position in spawnPoints)
        {
            if (firstpos == 0)
            {
                basePos = position;
                firstpos++;
                distance = Vector3.Distance(basePos.position, player.gameObject.transform.position);
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

        return basePos;

    }


    //apply 
    public static Vector3 Jumping(Vector3 jumping)
    {
       
        jumping.y += 2.0f * 5.0f;       
        return jumping;
    }

    //applies jumping to the player
    public static FixedVec3 JumpingFix(FixedVec3 jumping,float height)
    {
        jumping.yfix += (Fix64)height;         
        return jumping;
    }

    //convertion of fixed point vector 3 to vector 3
    public static Vector3 FixToVec3(FixedVec3 fv)
    {
        Vector3 convert;
        convert = new Vector3((float)fv.xfix, (float)fv.yfix,(float)fv.zfix);
        return convert;
    }

    //applies camera movement to the player
    public static Vector3 CameraMovement(Vector3 right,Vector3 forward,FixedVec2 fv)
    {
        Vector3 rightm;
        rightm = right * (float)fv.xpos + forward * (float)fv.ypos;
        return rightm;
    }
    //vector to fixed vector3
    public static FixedVec3 VecToFix3 (Vector3 v)
    {
        FixedVec3 convert;
        convert = new FixedVec3((Fix64)v.x, (Fix64)v.y, (Fix64)v.z);
        return convert;
    }
    //fixed vector 2 converted to vector 2 //addeed just incase
    public static Vector2 FixToVec2(FixedVec2 fv)
    {
        Vector2 convert;
        convert = new Vector2((float)fv.xpos, (float)fv.ypos);
        return convert;
    }

    //vector 2 converted to fixed vector 2 //addeed just incase
    public static FixedVec2 VecToFix2(Vector2 v)
    {
        FixedVec2 convert;
        convert = new FixedVec2((Fix64)v.x, (Fix64)v.y);
        return convert;
    }
    //change direction into a fixed vector 2
    public static FixedVec2 Vec2Creation(Direction dir)
    {
         var vec = new FixedVec2(0, 0);
        //switch the directions and apply them to the player
        //mapped as follows
        //(-1,1)      (0,1)       (1,1)              
        //(-1,0)      (0,0)       (1,0)              
        //(-1,-1)     (0,-1)      (1,-1)              
        switch (dir)
        {
            case Direction.SouthWest:
                vec = new FixedVec2(-1, -1);
                break;
            case Direction.South:
                vec = new FixedVec2(0, -1);
                break;
            case Direction.SouthEast:
                vec = new FixedVec2(1, -1);
                break;
            case Direction.West:
                vec = new FixedVec2(-1, 0);
                break;
            case Direction.Idle:
                vec = new FixedVec2(0, 0);
                break;
            case Direction.East:
                vec = new FixedVec2(1, 0);
                break;
            case Direction.NorthWest:
                vec = new FixedVec2(-1, 1);
                break;
            case Direction.North:
                vec = new FixedVec2(0, 1);
                break;
            case Direction.NorthEast:
                vec = new FixedVec2(1, 1);
                break;
            default:
                Debug.Log("something went wrong");
                break;
        }

        return vec;
    }
}
