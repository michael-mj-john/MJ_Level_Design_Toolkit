/*
 * UCSC Level Design Toolkit
 * 
 * Aidan Kennell
 * akennell94@gmail.com
 * 8/9/2018
 * 
 * Released under MIT Open Source License
 * 
 * This script handles all of the input to the player character (Jump, attack, sneak, run, walk, turn)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(StatManager))]

public class PlayerController : ControllerBase
{
    Rigidbody rigidBody;
    StatManager statManager;
    public GameObject attackTrigger;
    [Header("SFX Names")]
    public string footStepEvent;
    public string jumpEvent;
    public string attackEvent;
    protected bool isAttacking;
    protected bool isSneaking;
    protected float capsuleRadius;
    protected float sneakSpeedModifier = 0.6f;
    protected bool justJumped = false;
    protected bool climbingSlope;
    protected float maxAngleNoGravity = 15.0f;

    void Start()
    { 
        isAttacking = false;
        isSneaking = false;
        attackTimer = 0.0f;
        distanceToGrounded = .85f;
        rigidBody = transform.GetComponent<Rigidbody>();
        statManager = transform.GetComponent<StatManager>();
        input = new ControllerInput();
        animator = transform.GetComponent<Animator>();
        hasDoubleJumped = false;
        capsuleRadius = transform.GetComponent<CapsuleCollider>().radius;
    }

    void Update()
    {
        updateStates();
        input.read();

        handleJump();

        if (!isSneaking)
        {
            handleAttack();
        }

        if (!isAttacking)
        {
            handleMove();
        }

    }

    private void FixedUpdate()
    {
        /*
         * If the player has gravity acting on them when they are grounded then we run into the issue where they cannot walk up any slopes, so we make sure that gravity
         * is only applied if they are in the air.
        */
        if (!climbingSlope)
        {
            rigidBody.AddForce(0, -9.8f * gravity, 0);
        }
    }

    /*
     * Requires: The character's origin to be set at its feet, and the layer mask needs to be set to any
     *           layers that are considered the ground. The "ground" is defined as anything the player can 
     *           walk on
     * Modifies: input.isGrounded(bool), hasDoubleJumped(bool) and distanceToGrounded(float)
     * Returns: Nothing
     */
    public override void updateStates()
    {
        base.updateStates();

        RaycastHit frontBackHit;
        RaycastHit centralHit;

        if(Physics.Raycast(transform.position + new Vector3(0, 0.55f, 0), -transform.up, out centralHit, distanceToGrounded, groundLayer) && Vector3.Angle(transform.up, centralHit.normal) > maxAngleNoGravity)
        {
            climbingSlope = true;
        }
        else
        {
            climbingSlope = false;
        }

        if (Physics.Raycast(transform.position + new Vector3(0, 0.55f, 0) - transform.forward * capsuleRadius, -transform.up, out frontBackHit, distanceToGrounded, groundLayer) ||
            Physics.Raycast(transform.position + new Vector3(0, 0.55f, 0) + transform.forward * capsuleRadius, -transform.up, out frontBackHit, distanceToGrounded, groundLayer))
        {

            //This makes sure that if a character is trying to walk up a slope that is too big, they just fall instead of making it up the slope
            if (Vector3.Angle(transform.up, frontBackHit.normal) < maxWalkableSlope)
            {       
                input.isGrounded = true;
                hasDoubleJumped = false;

                /*
                 * This is to clear out any forces that were acting on the player while they were in the air. There was a bug where the character would still have the forces applied
                 * to them to even after they had landed. We don't want to clear the forces every frame (the player would not be able to move), so we use the bool justJumped to make 
                 * sure that we only clear the forces once right after landing.
                */
                if (justJumped)
                {
                    rigidBody.velocity = Vector3.zero;
                    justJumped = false;
                }
            }
            else
            {
                input.isGrounded = false;
                justJumped = true;
            }
        }
        else
        {
            input.isGrounded = false;
            justJumped = true;
        }


        animator.SetBool("IsGrounded", input.isGrounded);
    }

    /*
     * Requires: That there be a rigid body attached to the game object
     * Modifies: Applies a force, in the positive y, to a rigidbody component attached to the game object that this script is a component of
     * Returns: Nothing
     */
    public override void handleJump()
    {
        base.handleJump();

        if (input.jump)
        {
            float speedMod = input.throttle * input.throttle * Time.deltaTime * maxSpeed;
            Vector3 jumpForce = new Vector3(0, jumpSpeed * FORCE_MOD, 0);
            jumpForce += transform.forward * speedMod * forwardJumpForce * FORCE_MOD;
            rigidBody.AddForce(jumpForce);
        }

        if (input.doubleJump && !hasDoubleJumped && canDoubleJump)
        {
            float jumpForce = doubleJumpSpeed * FORCE_MOD;

            //We zero out the velocity in the Y to make sure that even if the player is falling they will go the full height of the double jump
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0, rigidBody.velocity.z);
            rigidBody.AddForce(0, jumpForce, 0);
            hasDoubleJumped = true;
            PlayJump();
        }


    }

    /* 
     * Requires: That there is a camera in the scene tagged as main camera
     * Modifies: Rotatates the game object this script is attached to and translates the same game object on the x,z plane
     * Returns: Nothing
     */   
    public override void handleMove()
    {
        base.handleMove();

        /* 
         * Turn the character using Quaternions to avoid gimbal lock and 0/360 wraparound issues that come from using Eular angles.
         * The heading is multiplied by the camera's rotation in order to get the player character to move relative to the camera's 
         * rotation. Zeroing out the x and z components ensures that the character is only moved in relation to the camera's rotaion
         * around the y axis.
         */
        Quaternion targetRotation = Quaternion.AngleAxis(input.heading, new Vector3(0, 1, 0));

        if (input.throttle > input.deadZone)
        {

            float rotationGap = Mathf.Abs(targetRotation.eulerAngles.y - transform.rotation.eulerAngles.y);

            //The less than 190 is to ensure that we are not doing the instant turn on any 0 to 360 wrap around
            if (rotationGap > instantTurnAngle && rotationGap < 190)
            {
                transform.rotation = targetRotation;
            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            transform.rotation = targetRotation;
        }

        float speedMod;

        //if the character is sneaking reduce the max speed to 3/5 it's origional amount
        if (statManager.getStealth())
        {
            speedMod = input.throttle * input.throttle * Time.deltaTime * (maxSpeed * sneakSpeedModifier);
        }
        else
        {
            speedMod = input.throttle * input.throttle * Time.deltaTime * maxSpeed;
        }
        
        animator.SetBool("Sneaking", statManager.getStealth());

        //Move the player character, and set the speed parameter in the animator to make sure the right blend state is played
        if (input.isGrounded)
        {
            transform.Translate(0, 0, speedMod);
        }
        else
        {
            transform.Translate(0, 0, speedMod * airControl);
        }

        animator.SetFloat("Speed", input.throttle);
    }

    /*
     * Requires: nothing
     * Modifies: isAttacking(bool), attackTimer(float), and the player character's position
     * Returns: nothing
     */
    public override void handleAttack()
    {
        base.handleAttack();

        if (input.attack && !isAttacking)
        {
            rigidBody.velocity = Vector3.zero;
            isAttacking = true;

            animator.Play("Attack");
            animator.Play("AttackStep");
            
            attackTimer = 0;
            attackTrigger.SetActive(true);
        }

        if (isAttacking)
        {
            attackTimer += Time.deltaTime;

            if(attackTimer > timeBetweenAttacks * attackWindup && attackTimer < timeBetweenAttacks * (attackEndPause + attackEndPauseMod))
            {
                //This translates the player forward
                transform.position += transform.forward * attackTranslationSpeed * Time.deltaTime;
            }

            if (attackTimer >= timeBetweenAttacks)
            {
                isAttacking = false;
                attackTrigger.SetActive(false);
            }
        }

        //animator.SetBool("Attacking", isAttacking);
    }

    /*
     * Requires: A trigger volume component the size of the attack area
     * Modifies: Nothing, but will call a funtion on any enemies that are inside the trigger volume to kill/damage them
     * Returns: Nothing
     */
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy" && isAttacking)
        {
            //damage or kill enemy
            if(other.gameObject.GetComponentInChildren<EnemyScript>() != null)
            {
                float damage = gameObject.GetComponent<StatManager>().getAttackDamage();
                other.gameObject.GetComponentInChildren<EnemyScript>().takeDamage(damage);
            }
            else
            {
                Destroy(other.gameObject);
            }

        }
    }


    /*
     * Requires: A Collider on the Character
     * Modifies: The position of the character
     * Returns: Nothing
     * 
     * This function handles the "step up" for the character. If a the difference, between the height of the ground a 
     * character is walking on and the height of a surface that the character bumps into, is under a threshold then we 
     * teleport the character up so that it looks like they have stepped up onto the higher surface.
     */
    private void OnCollisionEnter(Collision collision)
    {
        //We need to make sure that we are not stepping up if the character is currently in the air.
        if (input.isGrounded)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.point.y > transform.position.y && contact.point.y - transform.position.y < stepUpHeight)
                {
                    transform.position = new Vector3(transform.position.x, contact.point.y, transform.position.z);
                }
            }
        }
    }

    /*
     * Requires: Nothing
     * Modifies: input.freezeMovement(bool), animator.Speed (float)
     * Returns: Nothing
     * 
     * Disables all input to the character, and makes sure the animator is playing the idle animation
     */
    public override void freezeMovement()
    {
        base.freezeMovement();

        animator.SetFloat("Speed", 0.0f);
    }

    /*
    * Requires: Nothing
    * Modifies: input.freezeMovement(bool)
    * Returns: Nothing
    * 
    * Enables all input to the character
    */
    public override void unFreezeMovement()
    {
        base.unFreezeMovement();
    }

    public void toggleSneak()
    {
        isSneaking = !isSneaking;
        animator.SetBool("Sneaking", isSneaking);
    }

    public bool getSneaking() { return isSneaking; }


    void PlayFootstep()
    {
        AudioManager.PlaySound(footStepEvent, gameObject);
    }

    void PlayJump()
    {
        AudioManager.PlaySound(jumpEvent, gameObject);
    }

    void PlayAttack()
    {
        AudioManager.PlaySound(attackEvent, gameObject);
    }

    /*
     * Requires: A vector3 that is the position that the character should be looking at
     * Modifies: input.heading(float)
     * Returns: nothing
     * 
     * Will turn the character to face the location that is passed via the variable pos(Vector3)
     */
    public override void turnToPos(Vector3 pos)
    {
        base.turnToPos(pos);
    }
}

public class ControllerInput : VirtualInput
{
    Transform mainCamera;

    public ControllerInput() : base()
    {
        mainCamera = GameManagerScript.instance.getMainCamera().transform;
    }

    /*
     * Requires: Nothing
     * Modifies: throttle(float), heading(float), and jump(bool)
     * Returns: Nothing
     * 
     * Reads input and stores any changes, so that the player controller can access them later
     */
    public override void read()
    {
        base.read();

        if (!freezeMovement)
        {
            //Gets the joystick and WASD inputs
            Vector3 move = Vector3.zero;
            move.x = Input.GetAxisRaw("Horizontal");
            move.z = Input.GetAxisRaw("Vertical");

            //Ensure that the joy stick is pushed outside of the deadzone
            if (move.magnitude > deadZone)
            {
                throttle = Mathf.Min(move.magnitude, 1.0f);

                //Calculate heading in relation to the camera
                heading = Mathf.Rad2Deg * Mathf.Atan2(move.x, move.z);


                Quaternion cameraRotation = mainCamera.rotation;
                cameraRotation.x = 0;
                cameraRotation.z = 0;

                Quaternion targetRotation = Quaternion.AngleAxis(heading, new Vector3(0, 1, 0)) * cameraRotation;

                heading = targetRotation.eulerAngles.y;
            }

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                jump = true;
            }

            if (Input.GetButtonDown("Jump") && !isGrounded)
            {
                doubleJump = true;
            }

            if (Input.GetButtonDown("Fire1") && isGrounded)
            {
                attack = true;
            }
        }
    }
}
