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
        //while alive the bullet will fire
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

    //when teh bullet collides with something check what it is
    private void OnCollisionEnter(Collision collision)
    {
        //if its the ground destroy itself
        if (collision.gameObject.tag == "Ground")
        {
            Debug.Log("working");
            Destroy(this.gameObject);
        }
        //if its the enemy change its state before destroying self
        if (collision.gameObject.tag == "Enemy")
        {
            collision.gameObject.GetComponent<PatrollingEnemy>().BulletHit();
            Destroy(this.gameObject);
        }
    }
}
