using GameMovement.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//helper class with functions used by the Zombie class
public static class ZombieHelper 
{
    //gather the first key from the dictionary
    public static int GetFirstKey(Dictionary<int, ZombieData> dictionary)
    {
        var t = 0;
        bool first = false;
        //loop through the dictionary
        foreach (var item in dictionary)
        {
            //first item in the dictionary
            if (!first)
            {
                t = item.Key;
                first = true;
            }
            else
            {
                //when the old key is higher than the new item key
                //swap it over 
                if (t > item.Key)
                {
                    t = item.Key;
                }
            }
        }
        //return teh earliest key in the dictionary
        return t;
    }

    public static void ChangeColourInRunState(bool couldAttack, bool hitBool, Renderer render)
    {
        //if teh enemy is about to attack set colour
        if (couldAttack)
        {
            if(render.material.color != Color.green)
            render.material.color = Color.green;
        }
        else
        {
            //if teh enemy is hit set colour
            if (hitBool)
            {
                if (render.material.color != Color.grey)
                    render.material.color = Color.grey;
            }
            //setdefault colour
            else
            {
                if (render.material.color != Color.blue)
                    render.material.color = Color.blue;
            }
        }
    }
}
