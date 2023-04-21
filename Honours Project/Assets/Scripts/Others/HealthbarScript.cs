using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HealthbarScript : MonoBehaviour
{
    public Image healthbar;

    // Start is called before the first frame update

    public void Testing(float testing)
    {
        if (healthbar)
        {
            healthbar.fillAmount = testing / 100.0f;
        }
    }
  
}
