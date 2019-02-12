/*
 * UCSC Level Design Toolkit
 * 
 * Aidan Kennell
 * akennell94@gmail.com
 * 8/9/2018
 * 
 * Released under MIT Open Source License
 * 
 * This script is a framework for any controller be it enemy contorllers, character controllers, ect.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerBase : MonoBehaviour
{
#region Member Variables
    protected VirtualInput input;
    protected Animator animator;
    protected const float FORCE_MOD = 100.0f;
    protected float distanceToGrounded;

    [Header("Ground Movement")]
    public float rotationSpeed;
    [Range(20.0f, 180.0f)]
    public float instantTurnAngle = 160.0f;
    public float maxSpeed;
    [Range(0.0f, 2.0f)]
    public float stepUpHeight = 0.6f;
    [Range(10.0f, 90.0f)]
    public float maxWalkableSlope = 45.0f;

    [Header("Jump Settings")]
    public bool canDoubleJump = true;
    public LayerMask groundLayer;
    public float jumpSpeed;
    public float doubleJumpSpeed;
    public float gravity;
    [Range(0.0f, 1.0f)]
    public float airControl;
    [Range(1.0f, 100.0f)]
    public float forwardJumpForce;
    protected bool hasDoubleJumped;
    public bool fallDamage;

    [Header("Attack Settings")]
    public float timeBetweenAttacks = 1.0f;
    public float attackTranslationSpeed = 10.0f;
    [Range(0.0f, 0.5f)]
    public float attackWindup = 0.2f;
    [Range(0.0f, 0.5f)]
    public float attackEndPause = 0.3f;
    protected float attackEndPauseMod = 0.5f;
    protected float attackTimer;
    #endregion

    public ControllerBase()
    {
    }

    public virtual void handleJump()
    {
    }

    public virtual void handleMove()
    {
    }

    public virtual void handleAttack()
    {
    }

    public virtual void updateStates()
    {
    }

    public virtual void freezeMovement()
    {
        input.freezeMovement = true;
    }

    public virtual void unFreezeMovement()
    {
        input.freezeMovement = false;
    }

    public virtual void turnToPos(Vector3 pos)
    {
        Vector3 look = pos - transform.position;
        input.heading = Mathf.Atan2(look.x, look.z) * Mathf.Rad2Deg;
    }
}
