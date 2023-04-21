using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minimapScript : MonoBehaviour
{
    //source at
    //https://www.youtube.com/watch?v=a6GquxRUHD0
    //and brackleys
    //https://www.youtube.com/watch?v=28JTTXqMvOUp-
    private Player player;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            Vector3 newPos = player.transform.position;
            newPos.y = transform.position.y;
            transform.position = newPos;
        }
        else
        {
            player = FindObjectOfType<Player>();
        }
        
    }
}
