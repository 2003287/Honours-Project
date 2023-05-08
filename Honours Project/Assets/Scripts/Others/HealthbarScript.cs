using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HealthbarScript : MonoBehaviour
{
    //gather the Ui image
    public Image healthbar;

    //change the slider for the healthbar
    public void HealthUpdate(float testing)
    {
        if (healthbar)
        {
            healthbar.fillAmount = testing / 100.0f;
        }
    }
  
}
