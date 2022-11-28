using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrollingEnemy : EnemyBase
{

    [SerializeField]
    List<Transform> walkPositions;
    Transform walkToPosition;

    private bool touch;
    // Start is called before the first frame update
    void Start()
    {
        m_Enemystate = Enemystate.Walking;
        NewPosition();
        touch = false;
    }

    // Update is called once per frame
    void Update()
    {
        EnemySwitchstate();
    }

    private void NewPosition()
    {
        if (walkToPosition == null)
        {
            Debug.Log("walktoposition is null");
            int pos = Random.Range(0, 2);
            Debug.Log(pos);
            currentPosInt = pos;
            walkToPosition = walkPositions[pos];
            Debug.Log(walkToPosition.position);
            transform.LookAt(walkToPosition);
            walking = true;
        }
        else
        {
            int pos = Random.Range(0, 2);
            if (currentPosInt != pos)
            {
                currentPosInt = pos;
                walkToPosition = walkPositions[currentPosInt];
                transform.LookAt(walkToPosition);
                walking = true;
            }
            else
            {
                NewPosition();
            }
            

        }
    }
    public override void Walkingstate()
    {

        // Debug.Log("new state test");
        // Move our position a step closer to the target.
        if (walking)
        {
            var step = MovementSpeed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, walkToPosition.position, step);
            if (Vector3.Distance(transform.position, walkToPosition.position) < 0.001f)
            {
                walking = false;
            }
        }
        else
        {
            NewPosition();
        }

        if (sightobject.GetComponent<Detection>().GetTouch())
        {
            touch = true;
            SwitchEnemyState(Enemystate.Following);
        }
        
    }

}
