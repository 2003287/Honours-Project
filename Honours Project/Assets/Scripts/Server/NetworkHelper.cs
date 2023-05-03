using System.Collections;
using System.Collections.Generic;
using GameMovement.Network;
using UnityEngine;
using FixMath.NET;

//stats
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

    public static void Level1StatsCreation(int death, float score,float f)
    {
        level1com = true;
        level1Death = death;
        level1Score = score;     
        accuracy1 = f;
      
    }
    public static void Level2StatsCreation(int death, float score,  float f)
    {
        level2com = true;
        level2Death = death;
        level2Score = score;          
        accuracy2 = f;
       
    }
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
    //was a floats before
    public static FixedVec2 PlayerRotationVoid(FixedVec2 currot,FixedVec2 newrot)
    {
        //get the movements of the mouse
        
        //get the change in screenposition
        currot.xpos -=newrot.ypos;
        //currot.x -=newrot.y;
        currot.xpos = (Fix64)Mathf.Clamp((float)currot.xpos, -90f, 90f);
      //  currot.x = Mathf.Clamp(currot.x, -90f, 90f);
        currot.ypos += newrot.xpos;
       // currot.y += newrot.x;
        //aplly this to the body and camera
        return currot;
    }

    //creates a int from floating points
    public static int Testing(float tt)
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

    public static Vector3 Jumping(Vector3 jumping)
    {
       // Debug.Log(jumping);
        jumping.y += 2.0f * 5.0f;
       // Debug.Log(jumping);
        return jumping;
    }
    public static FixedVec3 JumpingFix(FixedVec3 jumping,float height)
    {
        jumping.yfix += (Fix64)height;         
        return jumping;
    }

    public static Vector3 FixToVec3(FixedVec3 fv)
    {
        Vector3 convert;
        convert = new Vector3((float)fv.xfix, (float)fv.yfix,(float)fv.zfix);
        return convert;
    }

    public static Vector3 CameraMovement(Vector3 right,Vector3 forward,FixedVec2 fv)
    {
        Vector3 rightm;
        rightm = right * (float)fv.xpos + forward * (float)fv.ypos;
        return rightm;
    }
    public static FixedVec3 VecToFix3 (Vector3 v)
    {
        FixedVec3 convert;
        convert = new FixedVec3((Fix64)v.x, (Fix64)v.y, (Fix64)v.z);
        return convert;
    }
    public static Vector2 FixToVec2(FixedVec2 fv)
    {
        Vector2 convert;
        convert = new Vector2((float)fv.xpos, (float)fv.ypos);
        return convert;
    }
    public static FixedVec2 VecToFix2(Vector2 v)
    {
        FixedVec2 convert;
        convert = new FixedVec2((Fix64)v.x, (Fix64)v.y);
        return convert;
    }
    public static FixedVec2 Vec2Creation(Direction dir)
    {
         var vec = new FixedVec2(0, 0);
        // Debug.Log(dir);
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
