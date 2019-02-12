using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(NavMeshAgent))]

public class AIController : MonoBehaviour {

   
    public GameObject idleNotifier;
    public GameObject alertNotifier;
    public GameObject attackNotifier;
 
    public GameObject startArea;
    public float moveSpeed = 6;
    public float runSpeedThreshold;
    [Space(10)]
    public bool returnToStartAreaAfterDisengage = true;
    public bool canHangAround = false;
    public float hangTime = 4;
    public GameObject hangAroundPoint;
    public float hangAroundRadius;

    [Space(10)]
    public bool canPatrol = false;
    public List<GameObject> patrolWaypoints = new List<GameObject>();
    public float waitTime = 0;

    [Space(10)]
    public bool canAttackPlayer = true;
    public float attackDamage;
    public float attackDuration;
    public float attackHitDelay;
    public bool canChase = true;
    [Space(10)]
    public PlayerDetectScript playerDetectTrigger;
    public AttackTriggerScript attackTrigger;
    [Range(0, 360)]
    public float fieldOfView ;
    public float frontDetectRange ;
    public float backDetectRange ;
    public float chaseRange ;
    public float chaseSpeed ;
    public float attackRange;

    private GameObject chasingTarget;
    private GameObject nearTarget;
    private NavMeshAgent navMeshAgent;
    private AnimatorControllerScript animatorController;
    private AIState currentState = AIState.Idle;
    private AIState lastState = AIState.Idle;
    private int currentWaypoint = 0;
    private float waitTimer;
    private float hangTimer = 0;
    private float attackHitDelayTimer = 0;
    private float attackTimer = 0;

    private enum AIState
    {
        Idle,
        Alert,
        Attacking,
        Dead
    }


    void Start()
    {
        navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        animatorController = gameObject.GetComponent<AnimatorControllerScript>();
        playerDetectTrigger.setRadius(frontDetectRange);
        attackTrigger.setAttackDamage(attackDamage);
        dealingDamage(false);
    }

    private void FixedUpdate()
    {

        switch (currentState)
        {
            case AIState.Dead:
                return;
            case AIState.Idle:
                if (canPatrol && patrolWaypoints.Count > 0)
                {
                    handlePatrolling();
                    break;
                }
                else if ((startArea != null) && returnToStartAreaAfterDisengage && (distanceToMe(startArea) > 0.2) && hangTimer == 0)
                {
                    walkTowardsTarget(startArea);
                    break;
                }
                else if (canHangAround)
                {
                    handleHangAround(null);
                    break;
                }
                break;
            case AIState.Alert:
                if (chasingTarget != null && isTargetStillInChaseRange())
                {
                    runTowardsTarget(chasingTarget);
                    float angle = getAngleToTarget(chasingTarget);
                    if (canAttackPlayer && (navMeshAgent.remainingDistance < attackRange) && (angle < 45))
                    {
                        currentState = AIState.Attacking;
                        break;
                    }
                   
                }
                else if (chasingTarget != null && !isTargetStillInChaseRange())
                {
                    chasingTarget = null;
                    navMeshAgent.stoppingDistance = 0f;
                    currentState = AIState.Idle;

                }else if(chasingTarget == null)
                {
                    navMeshAgent.stoppingDistance = 0f;
                    currentState = AIState.Idle;
                }
                break;
            case AIState.Attacking:
                navMeshAgent.speed = 0;
                if(attackTimer == 0)
                {
                    attackTimer = attackDuration;
                    animatorController.playAttack1();
                    //animator.SetBool("Punch Attack", true);
                }else if(attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;
                    if( attackTimer < (attackDuration - attackHitDelay))
                    {
                        dealingDamage(true);
                    }
                }else if (attackTimer < 0)
                {
                    currentState = AIState.Alert;
                    navMeshAgent.speed = chaseSpeed;
                    attackTimer = 0;
                    dealingDamage(false);
                }
                break;
        }

        if(currentState != AIState.Dead)
        {
            updateAlertModifier(currentState);
            updateAnimator();
            handleDeath();
        }

    }

    private bool isTargetStillInChaseRange()
    {
        float distance = distanceToMe(chasingTarget);
        return distance < chaseRange;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit hit;

        //    if (Physics.Raycast(ray, out hit))
        //    {
        //        navMeshAgent.SetDestination(hit.point);
        //    }
        //}
    }

    public void PlayerDetectTriggerEnter(Collider col)
    {
        Debug.DrawRay(transform.position, (col.gameObject.transform.position - transform.position), Color.green);
        GameObject target = col.gameObject;
        if (target.tag.Equals("Player") && canChase && !isPlayerStealth(target))
        {
            //Vector3 targetDir = target.transform.position - transform.position;
            //float angle = Vector3.Angle(targetDir, transform.forward);
            float angle = getAngleToTarget(col.gameObject);
            if ((angle > getFOV()) && (nearTarget == null))
            {
                if (distanceToMe(target) < backDetectRange)
                {
                    makeAIChaseTarget(target);
                    return;
                }
                nearTarget = target;
                return;
            }
            makeAIChaseTarget(target);          
        }
    }

    public void PlayerDetectTriggerStay(Collider col)
    {
       if(col.gameObject.tag.Equals("Player") && nearTarget != null)
        {
            Debug.DrawRay(transform.position, (col.gameObject.transform.position - transform.position), Color.red);
            float angle = getAngleToTarget(col.gameObject);
            if (angle < getFOV() || (angle > getFOV() && distanceToMe(col.gameObject) < backDetectRange) )
            {
                nearTarget = null;
                makeAIChaseTarget(col.gameObject);
            }
        }
    }

    public void PlayerDetectTriggerExit(Collider other)
    {
        if(other.gameObject.tag.Equals("Player") && nearTarget != null)
        {
            nearTarget = null;
        }
    }

    private void runTowardsTarget(GameObject chasingTarget)
    {
        navMeshAgent.stoppingDistance = 3f;
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.SetDestination(chasingTarget.transform.position);
    }

    private void walkTowardsPoint(Vector3 target)
    {
        navMeshAgent.stoppingDistance = 0;
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.SetDestination(target);
    }

    private void walkTowardsTarget(GameObject chasingTarget)
    {
        walkTowardsPoint(chasingTarget.transform.position);  
    }

    private void updateAlertModifier(AIState state)
    {
        switch (currentState)
        {
            case AIState.Idle:
                idleNotifier.SetActive(true);
                attackNotifier.SetActive(false);
                alertNotifier.SetActive(false);
                break;
            case AIState.Alert:
                idleNotifier.SetActive(false);
                attackNotifier.SetActive(false);
                alertNotifier.SetActive(true);
                break;
            case AIState.Attacking:
                idleNotifier.SetActive(false);
                attackNotifier.SetActive(true);
                alertNotifier.SetActive(false);
                break;
        }
    }

    private void handlePatrolling()
    {
        if (!navMeshAgent.destination.Equals(patrolWaypoints[currentWaypoint].transform.position)) {
            navMeshAgent.SetDestination(patrolWaypoints[currentWaypoint].transform.position);
        }
        if(navMeshAgent.remainingDistance < 0.1f)
        {
            if(waitTime > 0 && waitTimer == 0)
            {
                waitTimer = waitTime;
            }
            if(waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
                if (canHangAround)
                {
                    handleHangAround(patrolWaypoints[currentWaypoint]);
                }
            }
            else
            {
                waitTimer = 0;
                walkTowardsPoint(getNextWaypoint());
            }
           
        }
    }

    private void handleHangAround(GameObject hangOrigin)
    {
        if(hangOrigin == null)
        {
            hangOrigin = startArea;
        }
        if(hangTime > 0 && hangTimer == 0)
        {
            hangTimer = hangTime;
        }
        if(hangTimer > 0)
        {
            hangTimer -= Time.deltaTime;
        }
        else
        {
            hangTimer = hangTime;
            walkTowardsPoint(getNextHangPoint(hangOrigin));
        }
    }

    private Vector3 getNextHangPoint(GameObject origin)
    {
        Vector3 nextPoint = origin.transform.position;
        nextPoint += UnityEngine.Random.insideUnitSphere * hangAroundRadius;
        nextPoint.y = gameObject.transform.position.y;
        return nextPoint;
    }

    private Vector3 getNextWaypoint()
    {
        currentWaypoint = (currentWaypoint + 1) % patrolWaypoints.Count;
        return patrolWaypoints[currentWaypoint].transform.position;
    }

    private float getFOV()
    {
        return fieldOfView / 2f;
    }

    private void makeAIChaseTarget(GameObject target)
    {
        nearTarget = null;
        chasingTarget = target;
        currentState = AIState.Alert;
        runTowardsTarget(chasingTarget);
    }

    private float distanceToMe(GameObject target)
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }

    private bool checkTargetStealth(GameObject target)
    {
        if(target.GetComponent<PlayerController>() != null)
        {
           //Check for stealth on
        }
        return false;
    }

    private bool isPlayerStealth(GameObject target)
    {
        if (target.GetComponent<StatManager>() != null)
        {
            return target.GetComponent<StatManager>().getStealth();
        }
        else return false;
    }

    private void updateAnimator()
    {
        Debug.DrawRay(gameObject.transform.position, (navMeshAgent.destination - gameObject.transform.position), Color.red);
        if (navMeshAgent.remainingDistance < 0.1)
        {
            animatorController.stopMovement();
            //setAnimatorIdle();
        }
        else if(navMeshAgent.speed > 3)
        {
            animatorController.startRunning();
            //setAnimatorRun();
        }
        else
        {
            animatorController.startWalking();
            //setAnimatorWalk();
        }
    } 


    private float getAngleToTarget(GameObject target)
    {
        Vector3 targetDir = target.transform.position - transform.position;
        float angle = Vector3.Angle(targetDir, transform.forward);
        return angle;
    }

    private bool handleDeath()
    {
        if (GetComponent<EnemyScript>().isDead())
        {
            //setAnimatorIdle();
            animatorController.stopMovement();
            animatorController.die();
            //playDeathAnimation();
            navMeshAgent.isStopped = true;
            GetComponent<CapsuleCollider>().enabled = false;
            currentState = AIState.Dead;
            return true;
        }
        else return false;
    }

    private void dealingDamage(bool isDealingDamage)
    {
        attackTrigger.gameObject.SetActive(isDealingDamage);
    }


}
