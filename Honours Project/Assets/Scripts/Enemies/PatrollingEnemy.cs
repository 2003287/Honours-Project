using System.Collections.Generic;
using UnityEngine;

public class PatrollingEnemy : EnemyBase
{
    //positions for the enemy to move to
    [SerializeField]
    List<Transform> walkPositions;
    //curent position that the enemy will move to
    Transform walkToPosition;

    Transform resetPosition;

    private GameObject playerObject;
    private bool touch;
    private bool atkTouch;
    private bool resetpos;
    // Start is called before the first frame update
    void Start()
    {
        m_Enemystate = Enemystate.Walking;
        NewPosition();
        touch = false;
        atkTouch = false;
        resetpos = false;

        playerObject = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        EnemySwitchstate();

       
    }

    private void NewPosition()
    {
        // get the first position
        if (walkToPosition == null)
        {

            Debug.Log("walktoposition is null");
            int pos = Random.Range(0, 2);
            Debug.Log(pos);
            //set the varibles
            currentPosInt = pos;
            walkToPosition = walkPositions[pos];
            Debug.Log(walkToPosition.position);
            //look at the correct place and start walking
            transform.LookAt(walkToPosition);
            walking = true;
        }
        else
        {
            //get a random int
            int pos = Random.Range(0, 2);
            //if your allowed to move there then swap teh position to move to
            if (currentPosInt != pos)
            {
                currentPosInt = pos;
                walkToPosition = walkPositions[currentPosInt];
                transform.LookAt(walkToPosition);
                walking = true;
            }
            //redo the loop
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
        if (!resetpos)
        {
            //walk towards the next position
            if (walking)
            {
                var step = MovementSpeed * Time.deltaTime; // calculate distance to move
                transform.position = Vector3.MoveTowards(transform.position, walkToPosition.position, step);
                //when touching the new position switch positions
                if (Vector3.Distance(transform.position, walkToPosition.position) < 0.001f)
                {
                    walking = false;
                }
            }
            else
            {
                //get the new position
                NewPosition();
            }
        }
        else
        {
            //move towards teh orginal position to go back to walking
            var step = MovementSpeed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, resetPosition.position, step);
            //when touching the new position switch positions
            if (Vector3.Distance(transform.position, resetPosition.position) < 0.001f)
            {
                transform.LookAt(walkToPosition);
                resetpos = false;
            }
        }
       

        // if the player has been sighted follow them
        if (sightobject.GetComponent<Detection>().GetTouch() && !touch)
        {
            touch = true;
            resetPosition = transform;
            SwitchEnemyState(Enemystate.Following);
        }


    }


    //followstae override
    public override void Followingstate()
    {
        //while the player is still in there sights 
        if (touch)
        {

            if (!atkTouch)
            {
                //move towards the player
                transform.LookAt(playerObject.transform.position);
                Movetoward(playerObject.transform);
            }
            else
            {
                //give a timer for teh enemy to reload will be replaced later
                timer -= Time.deltaTime;

                if (timer <= 0.0f)
                {
                    Debug.Log("reloaded");
                    atkTouch = false;
                }

            }

            //when the player isnt in sight anymore stop moving
            if (!sightobject.GetComponent<Detection>().GetTouch())
            {
                if (!atkTouch)
                {
                    touch = false;
                    timer = 5.0f;
                    Debug.Log("Player out of sight");
                }
               
            }
            //for when the enemy should fire at the player will be changed later
            if (attackObject.GetComponent<Detection>().GetTouch())
            {
                if (!atkTouch)
                {
                    atkTouch = true;
                    timer = 2.0f;
                    Debug.Log("fireing at the player");
                }
               
            }

        }
        else
        {
            //timer to switch the state back to walking 
            if (!resetpos)
            {
                //switch state and allow movement back to the original position
                if (timer <= 0.0f)
                {
                    resetpos = true;
                    SwitchEnemyState(Enemystate.Walking);
                }

                //the player has been sighted again
                if (sightobject.GetComponent<Detection>().GetTouch())
                {
                    touch = true;
                    Debug.Log("Player found");

                }

                timer -= Time.deltaTime;
            }

        }
    }

    //used as a hurt state at teh moment will be changed later
    public override void Attackstate()
    {
        timer -= Time.deltaTime;

        if (timer <= 0.0f)
        {
            //reseting the bool and swithing state
            if (touch)
            {
                resetpos = true;
                touch = false;
            }

            SwitchEnemyState(Enemystate.Walking);
            Debug.Log("healed");
        }
    }

    //when hit by the bullet
    public void BulletHit()
    {
        timer = 2.0f;
        SwitchEnemyState(Enemystate.Attack);
        Debug.Log("enemy hurt");
    }


    // function to be used instead of the same lines of code over and over
    private void Movetoward(Transform move)
    {
        var step = MovementSpeed * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, move.position, step);
    }

}
