using GameMovement.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ZombieHelper 
{
    public static int GetKey(Dictionary<int, ZombieData> dictionary)
    {
        var t = 0;
        bool first = false;
        foreach (var item in dictionary)
        {
            if (!first)
            {
                t = item.Key;
                first = true;
            }
            else
            {
                if (t > item.Key)
                {
                    t = item.Key;
                }
            }
        }

        return t;
    }

    
}
