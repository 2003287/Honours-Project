using System.Collections;
using System.Collections.Generic;
using GameMovement.Network;
using UnityEngine;

public static class NetworkHelper 
{
    public static Vector2 PlayerRotationVoid(Vector2 currot,Vector2 newrot)
    {
        //get the movements of the mouse
        
        //get the change in screenposition
       currot.x -=newrot.y;
        currot.x = Mathf.Clamp(currot.x, -90f, 90f);
        currot.y += newrot.x;
        //aplly this to the bodya and camera
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
        Debug.Log(jumping);
        jumping.y += 2.0f * 5.0f;
        Debug.Log(jumping);
        return jumping;
    }

    public static Vector2 Vec2Creation(Direction dir)
    {
        Vector2 vec = new Vector2(0, 0);
        // Debug.Log(dir);
        switch (dir)
        {
            case Direction.SouthWest:
                vec = new Vector2(-1, -1);
                break;
            case Direction.South:
                vec = new Vector2(0, -1);
                break;
            case Direction.SouthEast:
                vec = new Vector2(1, -1);
                break;
            case Direction.West:
                vec = new Vector2(-1, 0);
                break;
            case Direction.Idle:
                vec = new Vector2(0, 0);
                break;
            case Direction.East:
                vec = new Vector2(1, 0);
                break;
            case Direction.NorthWest:
                vec = new Vector2(-1, 1);
                break;
            case Direction.North:
                vec = new Vector2(0, 1);
                break;
            case Direction.NorthEast:
                vec = new Vector2(1, 1);
                break;
            default:
                Debug.Log("something went wrong");
                break;
        }

        return vec;
    }
}
