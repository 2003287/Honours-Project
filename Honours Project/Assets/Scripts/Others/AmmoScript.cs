using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class AmmoScript : MonoBehaviour
{
    public Text test;
    public int num;
    public void ChangeText( float ammo)
    {
        test.text = "Ammo count: " + ammo.ToString()+" /"+num.ToString();
    }
}
