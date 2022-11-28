using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    float timer;

    [SerializeField]
    bool alive;
    void Start()
    {
        alive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (alive)
        {
            timer -= Time.deltaTime;

            if (timer <= 0.0f)
            {
                alive = false;
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
       
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            Debug.Log("working");
            Destroy(this.gameObject);
        }
    }
}
