using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// This script for activate a trigger event.
/// </summary>
public class TriggerReceiver : MonoBehaviour
{
    // All trigger events here. If you want to add more events, add here first.
    public enum TriggerEvents
    {
        PlayAnimation,
        DestroyGameObject,
        StartMovement,
        ChangeCharacterHealth,
        TimeChallenge,
        PlayAudio,
        InstantiateAnObject,
        MoveBox,
        ToggleObject,
        UseItem,
        ChangePlayerStatus
    };
    public TriggerEvents selectedEvent = TriggerEvents.PlayAnimation;
    public string soundActivationEventName = "SFX/ActivateTrigger";

    #region Animation
    public Animator animator;
    public string stateName;
    public int m_layer;
    [Tooltip("Animation play speed.")]
    public int playSpeed = 1;
    #endregion Animation

    #region DestroyObject
    public float secondsBeforeDestroy = 0;
    #endregion DestroyObject

    #region PlatformMovement
    private bool startMovePlatform = false;
    [Tooltip("Platform will automatically move back to start position when player fall off.")]
    public bool ezPlatform = false;
    public enum MoveBehaviors
    {
        OneTime,
        Infinite,
        PingPong
    };
    public MoveBehaviors selectedMoveBehavior = MoveBehaviors.OneTime;
    public List<Transform> waypoints;
    private int currentWpIndex = 0;
    private Transform nextWp;
    private bool reverseDir = false;
    public float delaySecondsToNextWp = 0.1f;
    [Tooltip("If true, the target's X and Z axis will be always same to the current waypoint's X&Z.")]
    public bool isAligned = false;
    [Tooltip("If true, the target's Y axis will be always same to the current waypoint's Y.")]
    public bool isRotate = false;
    [Tooltip("If true, the target will always use the same move speed.")]
    public bool useConsistentSpeed = true;
    [Tooltip("Time of from the current waypoint to the next waypoint.")]
    public float timeToNextWp = 2.0f;
    public float moveSpeed = 10.0f;
    public string soundMovementLoop = "SFX/ActivateTrigger";
    #endregion PlatformMovement

    #region Character Health Changes
    public GameObject playerStatManager;
    public float healthChange = 0;
    #endregion Character Health Changes

    #region Time Challenge
    public float delayBeforeChallengeStart = 3.0f;
    public float timeLimit = 10.0f;
    public bool challengeSuccess = false;
    private bool startTimeChallenge = false;
    #endregion Time Challenge

    #region Play Audio Clip
    public AudioManager audioManager;
    public float delayAudioPlay = 0;
    #endregion Play Audio Clip

    #region Instantiate An Object
    public GameObject prefab;
    public enum ObjType { Camera, OtherGameObject };
    public ObjType selectedObjType = ObjType.OtherGameObject;
    public bool baseOnTargetPos = false;
    public bool SetTargetAsParent = false;
    [Tooltip("Instantiate an object on a specified position.")]
    public Vector3 instantiatePos;
    private GameObject prefabRef;
    #endregion Instantiate An Object

    #region Cutscene
    [Tooltip("Camera focus will moving when check this.")]
    public Transform cameraFocus;
    public float cutsceneDuration = 10.0f;
    public bool useFadeEffect = false;
    public Canvas uiCanvas;
    public GameObject fadeMask;
    public float fadeSpeed = 0.1f;
    public bool useMovingFocus = false;
    public List<Transform> focusWaypoints;
    public float focusMovespeed = 1.0f;
    public bool focusUseConsistentSpeed = true;
    public float f_timeToNextWaypoint = 5.0f;
    [Tooltip("Camera will moving when check this.")]
    public bool useMovingCamera = false;
    public List<Transform> cameraWaypoints;
    public float cameraMovespeed = 1.0f;
    public bool cameraUseConsistentSpeed = true;
    public float c_timeToNextWaypoint = 5.0f;
    private bool startCutscene = false;
    private GameObject fadeRef;
    private Transform focusNextWp;
    private Transform cameraNextWp;
    private int focusCurrentWpIndex;
    private int cameraCurrentWpIndex;
    #endregion Cutscene

    #region MoveBox Values
    public float moveDistance = 5.0f;
    public bool canBePushed = true;
    public bool canBePulled = true;
    [Tooltip("If true, the box can be move along X-axis(Left/Right), otherwise it will move along Z-axis(Forward/Backward)")]
    public bool moveAlongX_Axis = false;
    private bool startMoveBox = false;
    private bool enterMoveBoxMode = false;
    // 1: Push, -1: Pull.
    private int moveBoxDirection = 1;
    private Vector3 boxDistination;
    public string soundMoveBox = "SFX/ActivateTrigger";
    #endregion MoveBox Values

    #region Toggle An Object
    public bool toggleOnOrOff = true;
    #endregion Toggle An Object

    #region Use Items
    [Tooltip("The input item names must matched the item database.")]
    public List<string> requiredItem = new List<string>();
    public Inventory inventory;
    #endregion

    #region Change Player Status
    public StatManager statManager;
    public enum PlayerStatus { isStealth, canDoubleJump }
    public PlayerStatus selectedStatus = PlayerStatus.isStealth;
    public bool enableOrDisable = true;
    #endregion Change Player Status
    // General properties
    private GameObject target;
    private Vector3 targetInitPos;
    private Vector3 targetInitRot;
    private float timer = 0;

    // Use this for initialization
    void Start()
    {
        if (GetComponent<GenericTrigger>().targetObject)
        {
            target = GetComponent<GenericTrigger>().targetObject;
            targetInitPos = target.transform.position;
            targetInitRot = target.transform.eulerAngles;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (startMovePlatform)
        {
            if (ezPlatform)
            {
                if (CheckCharacterOnPlatform())
                    PlatformMovement();
                else
                    BackToStartPosition();
            }
            else
                PlatformMovement();
        }

        getPlayerTransform().GetComponent<PlayerController>().enabled = !enterMoveBoxMode;
        if (startMoveBox)
            MoveBox();

        if (startTimeChallenge)
            TimeChallenge();

        if (startCutscene)
        {
            PlayCutscene();
        }
        else
        {
            if (fadeRef != null)
                Destroy(fadeRef, 1);
        }

    }

    public void OnEnterTrigger()
    {
        Debug.Log(selectedEvent.ToString());

        switch (selectedEvent)
        {
            case TriggerEvents.PlayAnimation:
                PlayAnimation();
                break;
            case TriggerEvents.DestroyGameObject:
                DestroyGameObject();
                break;
            case TriggerEvents.StartMovement:
                TurnOnPlatformMovement();
                break;
            case TriggerEvents.ChangeCharacterHealth:
                ChangeHealth();
                break;
            case TriggerEvents.TimeChallenge:
                StartCoroutine(TimeChallengeCountdown());
                break;
            case TriggerEvents.PlayAudio:
                StartCoroutine(DelayAudioPlay());
                break;
            case TriggerEvents.InstantiateAnObject:
                InstantiaAnObject();
                break;
            case TriggerEvents.MoveBox:
                ToggleMoveBox();
                break;
            case TriggerEvents.ToggleObject:
                ToggleObject();
                break;
            case TriggerEvents.UseItem:
                UseItem();
                break;
            case TriggerEvents.ChangePlayerStatus:
                ChangeStatus();
                break;
        }

    }

    public void OnStayTrigger()
    {
        switch (selectedEvent)
        {
            case TriggerEvents.StartMovement:
                TurnOnPlatformMovement();
                break;
            case TriggerEvents.MoveBox:
                ToggleMoveBox();
                break;
        }
    }

    public void OnExitTrigger()
    {
        switch (selectedEvent)
        {
            case TriggerEvents.ChangePlayerStatus:
                ChangeStatus();
                break;
        }
    }

    private void PlayAnimation()
    {
        if (animator)
        {
            animator.Play(stateName, m_layer);
            animator.speed = playSpeed;
            // TODO: Start to play animations.
        }
        else
            print("Animator not set.");
    }

    private void ToggleObject()
    {
        if (toggleOnOrOff)
            target.SetActive(true);
        else
            target.SetActive(false);
        AudioManager.PlaySound(soundActivationEventName, gameObject);
    }

    private void DestroyGameObject()
    {
        AudioManager.PlaySound(soundActivationEventName, gameObject);
        Destroy(target, secondsBeforeDestroy);
    }

    #region Move Platform Methods
    private void TurnOnPlatformMovement()
    {
        AudioManager.PlaySound(soundActivationEventName, gameObject);
        AudioManager.PlaySound(soundMovementLoop, gameObject);

        if (target.GetComponent<M_Platform>() == null)
            target.AddComponent<M_Platform>();

        // Add start waypoint to waypoints list.
        if (target.transform.position != waypoints[0].position)
        {
            GameObject startPos = new GameObject();
            startPos.transform.position = target.transform.position;
            waypoints.Insert(0, startPos.transform);
        }

        startMovePlatform = true;
        currentWpIndex = 0;
        nextWp = waypoints[1];
        if (!useConsistentSpeed)
            moveSpeed = (nextWp.position - target.transform.position).magnitude / timeToNextWp;
    }

    private void PlatformMovement()
    {
        // scale the test with speed to avoid artifacts at faster speeds
        double distanceTestVal = .01 * moveSpeed;

        //detect if platform is at its goal
        if (Mathf.Abs(target.transform.position.x - nextWp.position.x) < distanceTestVal
        && Mathf.Abs(target.transform.position.y - nextWp.position.y) < distanceTestVal
        && Mathf.Abs(target.transform.position.z - nextWp.position.z) < distanceTestVal)

        {
            if (timer < delaySecondsToNextWp)
                timer += Time.deltaTime;
            else
            {
                timer = 0;

                if (reverseDir)
                    currentWpIndex -= 1;
                else
                    currentWpIndex += 1;

                switch (selectedMoveBehavior)
                {
                    case MoveBehaviors.OneTime:
                        if (currentWpIndex == waypoints.Count - 1)
                        {
                            startMovePlatform = false;
                            AudioManager.StopSound(soundMovementLoop);
                        }
                        else
                            nextWp = waypoints[currentWpIndex + 1];
                        break;
                    case MoveBehaviors.Infinite:
                        if (currentWpIndex == waypoints.Count - 1)
                        {
                            currentWpIndex = -1;
                            nextWp = waypoints[0];
                            break;
                        }
                        else
                            nextWp = waypoints[currentWpIndex + 1];
                        break;
                    case MoveBehaviors.PingPong:
                        {
                            if (currentWpIndex == waypoints.Count - 1)
                            {
                                nextWp = waypoints[waypoints.Count - 2];
                                reverseDir = !reverseDir;
                            }
                            else if (currentWpIndex == 0)
                            {
                                nextWp = waypoints[1];
                                reverseDir = !reverseDir;
                            }
                            else
                            {
                                if (reverseDir)
                                {
                                    nextWp = waypoints[currentWpIndex - 1];
                                }
                                else
                                {
                                    nextWp = waypoints[currentWpIndex + 1];
                                }
                            }
                        }
                        break;
                }

                if (!useConsistentSpeed)
                    moveSpeed = (nextWp.position - target.transform.position).magnitude / timeToNextWp;

            }
        }

        else if (target.transform.position != nextWp.position)
        {
            Vector3 dir = (nextWp.position - target.transform.position).normalized;

            if (isAligned && isRotate)
            {
                target.transform.LookAt(nextWp);
            }
            else if (isRotate)
            {
                target.transform.eulerAngles = new Vector3(target.transform.eulerAngles.x, nextWp.eulerAngles.y, target.transform.eulerAngles.z);
            }
            else if (isAligned)
            {
                target.transform.eulerAngles = new Vector3(nextWp.transform.eulerAngles.x, target.transform.eulerAngles.y, nextWp.transform.eulerAngles.z);
            }


            target.transform.position += new Vector3(dir.x * moveSpeed * Time.deltaTime,
                dir.y * moveSpeed * Time.deltaTime,
                dir.z * moveSpeed * Time.deltaTime);
        }
    }

    private bool CheckCharacterOnPlatform()
    {
        for (int i = 0; i < target.transform.childCount; i++)
            if (target.transform.GetChild(i).CompareTag("Player"))
                return true;

        return false;
    }

    private void BackToStartPosition()
    {
        if (Mathf.Abs(target.transform.position.x - waypoints[0].position.x) < .5f
        && Mathf.Abs(target.transform.position.y - waypoints[0].position.y) < .5f
        && Mathf.Abs(target.transform.position.z - waypoints[0].position.z) < .5f)
        {
            currentWpIndex = 0;
            nextWp = waypoints[1];
        }
        else if (target.transform.position != waypoints[0].position)
        {
            Vector3 dir = (waypoints[0].position - target.transform.position).normalized;

            if (isAligned && isRotate)
            {
                target.transform.LookAt(waypoints[0]);
            }
            else if (isRotate)
            {
                target.transform.eulerAngles = new Vector3(target.transform.eulerAngles.x, waypoints[0].eulerAngles.y, target.transform.eulerAngles.z);
            }
            else if (isAligned)
            {
                target.transform.eulerAngles = new Vector3(waypoints[0].transform.eulerAngles.x, target.transform.eulerAngles.y, waypoints[0].transform.eulerAngles.z);
            }

            target.transform.position += new Vector3(dir.x * moveSpeed * Time.deltaTime,
                dir.y * moveSpeed * Time.deltaTime,
                dir.z * moveSpeed * Time.deltaTime);
        }
    }
    #endregion

    private void ChangeHealth()
    {
        getPlayer().GetComponent<StatManager>().ChangeHealth(healthChange);
        AudioManager.PlaySound(soundActivationEventName, gameObject);
    }

    #region Time Challenge Methods
    private IEnumerator TimeChallengeCountdown()
    {
        yield return new WaitForSeconds(delayBeforeChallengeStart);
        transform.GetComponent<Collider>().enabled = false;
        startTimeChallenge = true;
    }

    private void TimeChallenge()
    {
        if (timer < timeLimit)
        {
            timer += Time.deltaTime;
            if (Mathf.Abs(getPlayerTransform().position.x - target.transform.position.x) < .5f
                && Mathf.Abs(getPlayerTransform().position.y - target.transform.position.y) < .5f
                && Mathf.Abs(getPlayerTransform().position.z - target.transform.position.z) < .5f)
            {
                challengeSuccess = true;
                startTimeChallenge = false;
                return;
            }
        }
        else
        {
            timer = 0;
            transform.GetComponent<Collider>().enabled = true;
            challengeSuccess = false;
            startTimeChallenge = false;
        }
    }
    #endregion

    private void InstantiaAnObject()
    {
        if (!baseOnTargetPos)
        {
            prefabRef = Instantiate(prefab, instantiatePos, Quaternion.identity);
        }
        if (baseOnTargetPos)
        {
            prefabRef = Instantiate(prefab, target.transform.position, Quaternion.identity);
            if (SetTargetAsParent)
            {
                prefabRef.transform.SetParent(target.transform);
            }
        }

        if (selectedObjType == ObjType.Camera && prefabRef)
        {
            StartCutscene();
        }
    }

    private void StartCutscene()
    {
        if (useMovingFocus)
        {
            if (cameraFocus.position != focusWaypoints[0].position)
            {
                GameObject startPos = new GameObject();
                startPos.transform.position = cameraFocus.position;
                focusWaypoints.Insert(0, startPos.transform);
            }

            focusCurrentWpIndex = 0;
            focusNextWp = focusWaypoints[1];
            if (!focusUseConsistentSpeed)
                focusMovespeed = (focusNextWp.position - cameraFocus.position).magnitude / f_timeToNextWaypoint;
        }

        if (useMovingCamera)
        {
            if (prefabRef.transform.position != cameraWaypoints[0].position)
            {
                GameObject startPos = new GameObject();
                startPos.transform.position = prefabRef.transform.position;
                cameraWaypoints.Insert(0, startPos.transform);
            }

            cameraCurrentWpIndex = 0;
            cameraNextWp = cameraWaypoints[1];
            if (!cameraUseConsistentSpeed)
                focusMovespeed = (cameraNextWp.position - prefabRef.transform.position).magnitude / c_timeToNextWaypoint;
        }

        if (useFadeEffect)
        {
            fadeRef = Instantiate(fadeMask);
            if (uiCanvas)
                fadeRef.transform.SetParent(uiCanvas.transform, false);
            else
                print("Canvas not found.");
            fadeRef.GetComponent<Animator>().speed = fadeSpeed;
            fadeRef.GetComponent<Animator>().SetBool("startFading", true);
        }
        timer = 0;
        getPlayer().GetComponent<PlayerController>().enabled = false;
        startCutscene = true;
    }

    private void PlayCutscene()
    {
        if (timer < cutsceneDuration)
        {
            prefabRef.transform.LookAt(cameraFocus);

            // Focus movement
            if (useMovingFocus)
            {
                if (Mathf.Abs(cameraFocus.position.x - focusNextWp.position.x) < .5f
                && Mathf.Abs(cameraFocus.position.y - focusNextWp.position.y) < .5f
                && Mathf.Abs(cameraFocus.position.z - focusNextWp.position.z) < .5f)
                {
                    focusCurrentWpIndex += 1;

                    if (focusCurrentWpIndex + 1 < focusWaypoints.Count)
                        focusNextWp = focusWaypoints[focusCurrentWpIndex + 1];
                    if (!focusUseConsistentSpeed)
                        focusMovespeed = (focusNextWp.position - cameraFocus.position).magnitude / f_timeToNextWaypoint;
                }

                else if (cameraFocus.position != focusNextWp.position)
                {
                    Vector3 dir = (focusNextWp.position - cameraFocus.position).normalized;

                    cameraFocus.position += new Vector3(dir.x * focusMovespeed * Time.deltaTime,
                        dir.y * focusMovespeed * Time.deltaTime,
                        dir.z * focusMovespeed * Time.deltaTime);
                }
            }

            if (useMovingCamera)
            {
                // Camera movement
                if (Mathf.Abs(prefabRef.transform.position.x - cameraNextWp.position.x) < .5f
                && Mathf.Abs(prefabRef.transform.position.y - cameraNextWp.position.y) < .5f
                && Mathf.Abs(prefabRef.transform.position.z - cameraNextWp.position.z) < .5f)
                {
                    cameraCurrentWpIndex += 1;
                    if (cameraCurrentWpIndex + 1 < cameraWaypoints.Count)
                        cameraNextWp = cameraWaypoints[cameraCurrentWpIndex + 1];
                    if (!cameraUseConsistentSpeed)
                        cameraMovespeed = (cameraNextWp.position - prefabRef.transform.position).magnitude / c_timeToNextWaypoint;
                }

                else if (prefabRef.transform.position != cameraNextWp.position)
                {
                    Vector3 dir = (cameraNextWp.position - prefabRef.transform.position).normalized;

                    prefabRef.transform.position += new Vector3(dir.x * cameraMovespeed * Time.deltaTime,
                        dir.y * cameraMovespeed * Time.deltaTime,
                        dir.z * cameraMovespeed * Time.deltaTime);
                }
            }

            timer += Time.deltaTime;
        }
        else
        {
            getPlayer().GetComponent<PlayerController>().enabled = true;
            fadeRef.GetComponent<Animator>().SetBool("startFading", true);
            startCutscene = false;
            Destroy(prefabRef);
        }
    }

    #region Play Audio Methods
    private IEnumerator DelayAudioPlay()
    {
        yield return new WaitForSeconds(delayAudioPlay);
        PlayAudioClip();
    }

    private void PlayAudioClip()
    {
        AudioManager.PlaySound(soundActivationEventName, gameObject);
    }
    #endregion

    #region Move Box Methods
    private void ToggleMoveBox()
    {
        if (Input.GetButtonDown("Interact") && !enterMoveBoxMode)
        {
            enterMoveBoxMode = true;
        }
        else if (Input.GetButtonDown("Interact") && enterMoveBoxMode)
        {
            enterMoveBoxMode = false;
            startMoveBox = false;
        }

        if (enterMoveBoxMode)
        {
            float move = Input.GetAxisRaw("Vertical");

            if (move > 0 && canBePushed)
            {
                // Reset move speed to positive.
                if (moveSpeed < 0)
                    moveSpeed *= -1;
                moveBoxDirection = 1;
                AudioManager.PlaySound(soundActivationEventName, gameObject);
                InitialBoxMovement();
            }
            if (move < 0 && canBePulled)
            {
                if (moveSpeed < 0)
                    moveSpeed *= -1;
                moveBoxDirection = -1;
                AudioManager.PlaySound(soundActivationEventName, gameObject);

                InitialBoxMovement();
            }
        }
    }

    private void InitialBoxMovement()
    {
        // Disable controller.
        if (getPlayer() != null)
            getPlayer().GetComponent<PlayerController>().enabled = false;

        if (moveAlongX_Axis)
        {
            // Player is on the right of the box.
            if (Vector3.Cross(-target.transform.forward, getPlayerTransform().forward).y >= 0)
            {
                moveSpeed *= -1;
                boxDistination = target.transform.position + target.transform.right.normalized * -moveBoxDirection * moveDistance;
            }
            else
            {
                boxDistination = target.transform.position + target.transform.right.normalized * moveBoxDirection * moveDistance;
            }
            startMoveBox = true;
        }
        else
        {
            // Player is on the front of the box.
            if (Vector3.Dot(-target.transform.forward, getPlayerTransform().forward) >= 0)
            {
                moveSpeed *= -1;
                boxDistination = target.transform.position + target.transform.forward.normalized * -moveBoxDirection * moveDistance;
            }
            else
            {
                boxDistination = target.transform.position + target.transform.forward.normalized * moveBoxDirection * moveDistance;
            }
            startMoveBox = true;
        }
    }

    private void MoveBox()
    {
        if (moveAlongX_Axis)
        {
            if (Mathf.Abs(target.transform.position.x - boxDistination.x) > .5f)
            {
                target.transform.position += target.transform.right.normalized * moveBoxDirection * moveSpeed * Time.deltaTime;
                getPlayerTransform().position += target.transform.right.normalized * moveBoxDirection * moveSpeed * Time.deltaTime;
            }
            else
                startMoveBox = false;
        }

        else
        {
            if (Mathf.Abs(target.transform.position.z - boxDistination.z) > .5f)
            {
                target.transform.position += target.transform.forward.normalized * moveBoxDirection * moveSpeed * Time.deltaTime;
                getPlayerTransform().position += target.transform.forward.normalized * moveBoxDirection * moveSpeed * Time.deltaTime;
            }
            else
                startMoveBox = false;
        }
        // Enable controller to resume movement.
        if (!startMoveBox)
            if (getPlayer() != null)
                getPlayer().GetComponent<PlayerController>().enabled = true;
    }
    #endregion

    private void UseItem()
    {
        if (inventory != null)
        {
            foreach (string item in requiredItem)
            {
                if (!inventory.CheckItemByTitle(item))
                {
                    print("You do not have enough items.");
                    return;
                }
            }
            foreach (string item in requiredItem)
            {
                inventory.RemoveItemByTitle(item);
            }
            target.SetActive(true);
        }
        else print("Inventory not found.");
    }

    private void ChangeStatus()
    {
        if (selectedStatus == PlayerStatus.canDoubleJump)
        {
            if (statManager.getCanDOubleJump())
                statManager.setCanDoubleJump(false);
            else
                statManager.setCanDoubleJump(true);
        }
        else if (selectedStatus == PlayerStatus.isStealth)
        {
            if (statManager.getStealth())
                statManager.setStealth(false);
            else
                statManager.setStealth(true);
        }

    }

    private GameObject getPlayer()
    {
        return GameManagerScript.instance.getPlayer();
    }

    private Transform getPlayerTransform()
    {
        return GameManagerScript.instance.getPlayer().transform;
    }

    public void ResetTrigger()
    {
        Debug.Log(name + " trigger been reset.");

        if (transform.GetComponent<Collider>().enabled == false)
            transform.GetComponent<Collider>().enabled = true;

        switch (selectedEvent)
        {
            case TriggerEvents.PlayAnimation:
                print("There's nothing need to be reset in this trigger.");
                break;
            case TriggerEvents.DestroyGameObject:
                print("There's nothing need to be reset in this trigger.");
                break;
            case TriggerEvents.StartMovement:
                target.transform.position = targetInitPos;
                reverseDir = false;
                startMovePlatform = false;
                // TODO: Stop playing sounds.
                break;
            case TriggerEvents.ChangeCharacterHealth:
                print("There's nothing need to be reset in this trigger.");
                break;
            case TriggerEvents.TimeChallenge:
                challengeSuccess = false;
                startTimeChallenge = false;
                break;
            case TriggerEvents.PlayAudio:
                print("There's nothing need to be reset in this trigger.");
                break;
            case TriggerEvents.InstantiateAnObject:
                Destroy(prefabRef);
                break;
            case TriggerEvents.MoveBox:
                startMoveBox = false;
                target.transform.position = targetInitPos;
                target.transform.eulerAngles = targetInitRot;
                break;
            case TriggerEvents.ToggleObject:
                target.SetActive(false);
                break;
        }
    }
}

[CustomEditor(typeof(TriggerReceiver))]
public class TriggerReceiverEditor : Editor
{
    private SerializedObject tr;
    private SerializedProperty selectedTriggerEvent;

    private SerializedProperty player;

    #region Animation
    private SerializedProperty animator;
    private SerializedProperty stateName;
    private SerializedProperty m_layer;
    private SerializedProperty playSpeed;
    #endregion Animation

    #region DestroyObject
    private SerializedProperty secondsBeforeDestroy;
    #endregion DestroyObject

    #region PlatformMovement
    private SerializedProperty ezPlatform;
    private SerializedProperty selectedMoveBehavior;
    private SerializedProperty waypoints;
    private SerializedProperty delaySecondsToNextWp;
    private SerializedProperty isAligned;
    private SerializedProperty isRotate;
    private SerializedProperty moveSpeed;
    private SerializedProperty useConsistentSpeed;
    private SerializedProperty timeToNextWp;
    #endregion PlatformMovement

    #region Character Health Changes
    private SerializedProperty playerStatManager;
    private SerializedProperty healthChange;
    #endregion Character Health Changes 

    #region Time Challenge
    private SerializedProperty delayBeforeChallengeStart;
    private SerializedProperty timeLimit;
    #endregion Time Challenge

    #region Play Audio Clip
    public SerializedProperty audioManager;
    public SerializedProperty delayAudioPlay;
    #endregion Play Audio Clip

    #region Instantiate An Object 
    public SerializedProperty prefab;
    public SerializedProperty selectedObjType;
    public SerializedProperty baseOnTargetPos;
    public SerializedProperty SetTargetAsParent;
    public SerializedProperty positionRef;
    public SerializedProperty instantiatePos;
    #endregion Instantiate An Object 

    #region Cutscene
    public SerializedProperty cameraFocus;
    public SerializedProperty cutsceneDuration;
    public SerializedProperty useFadeEffect;
    public SerializedProperty uiCanvas;
    public SerializedProperty fadeMask;
    public SerializedProperty fadeSpeed;
    public SerializedProperty useMovingFocus;
    public SerializedProperty focusWaypoints;
    public SerializedProperty focusMovespeed;
    public SerializedProperty focusUseConsistentSpeed;
    public SerializedProperty f_timeToNextWaypoint;
    public SerializedProperty useMovingCamera;
    public SerializedProperty cameraWaypoints;
    public SerializedProperty cameraMovespeed;
    public SerializedProperty cameraUseConsistentSpeed;
    public SerializedProperty c_timeToNextWaypoint;
    #endregion Cutscene

    #region Move boxes
    private SerializedProperty moveDistance;
    private SerializedProperty canBePushed;
    private SerializedProperty canBePulled;
    private SerializedProperty moveAlongX_Axis;
    #endregion Move boxes

    #region Toggle object
    private SerializedProperty toggleOnOrOff;
    #endregion Toogle Object

    #region Use Items
    public SerializedProperty requiredItem;
    public SerializedProperty inventory;
    #endregion Use Items

    #region Change Player Status
    public SerializedProperty statManager;
    public SerializedProperty selectedStatus;
    public SerializedProperty enableOrDisable;
    #endregion Change Player Status

    private SerializedProperty soundActivationEventName;
    private SerializedProperty soundMovementLoop;

    /* Add your properties beneath */
    /* End of Properties declearation */

    /// <summary>
    /// To use your properties, you need to add FindProperty("youPropertyName") function below.
    /// </summary>
    void OnEnable()
    {
        tr = new SerializedObject(target);
        selectedTriggerEvent = tr.FindProperty("selectedEvent");

        player = tr.FindProperty("player");

        animator = tr.FindProperty("animator");
        stateName = tr.FindProperty("stateName");
        m_layer = tr.FindProperty("m_layer");
        playSpeed = tr.FindProperty("playSpeed");

        secondsBeforeDestroy = tr.FindProperty("secondsBeforeDestroy");

        ezPlatform = tr.FindProperty("ezPlatform");
        selectedMoveBehavior = tr.FindProperty("selectedMoveBehavior");
        waypoints = tr.FindProperty("waypoints");
        delaySecondsToNextWp = tr.FindProperty("delaySecondsToNextWp");
        isAligned = tr.FindProperty("isAligned");
        isRotate = tr.FindProperty("isRotate");
        moveSpeed = tr.FindProperty("moveSpeed");
        useConsistentSpeed = tr.FindProperty("useConsistentSpeed");
        timeToNextWp = tr.FindProperty("timeToNextWp");

        playerStatManager = tr.FindProperty("playerStatManager");
        healthChange = tr.FindProperty("healthChange");

        delayBeforeChallengeStart = tr.FindProperty("delayBeforeChallengeStart");
        timeLimit = tr.FindProperty("timeLimit");

        audioManager = tr.FindProperty("audioManager");
        delayAudioPlay = tr.FindProperty("delayAudioPlay");

        prefab = tr.FindProperty("prefab");
        selectedObjType = tr.FindProperty("selectedObjType");
        baseOnTargetPos = tr.FindProperty("baseOnTargetPos");
        SetTargetAsParent = tr.FindProperty("SetTargetAsParent");
        positionRef = tr.FindProperty("positionRef");
        instantiatePos = tr.FindProperty("instantiatePos");

        cameraFocus = tr.FindProperty("cameraFocus");
        cutsceneDuration = tr.FindProperty("cutsceneDuration");
        useFadeEffect = tr.FindProperty("useFadeEffect");
        uiCanvas = tr.FindProperty("uiCanvas");
        fadeMask = tr.FindProperty("fadeMask");
        fadeSpeed = tr.FindProperty("fadeSpeed");
        useMovingFocus = tr.FindProperty("useMovingFocus");
        focusWaypoints = tr.FindProperty("focusWaypoints");
        focusMovespeed = tr.FindProperty("focusMovespeed");
        focusUseConsistentSpeed = tr.FindProperty("focusUseConsistentSpeed");
        f_timeToNextWaypoint = tr.FindProperty("f_timeToNextWaypoint");
        useMovingCamera = tr.FindProperty("useMovingCamera");
        cameraWaypoints = tr.FindProperty("cameraWaypoints");
        cameraMovespeed = tr.FindProperty("cameraMovespeed");
        cameraUseConsistentSpeed = tr.FindProperty("cameraUseConsistentSpeed");
        c_timeToNextWaypoint = tr.FindProperty("c_timeToNextWaypoint");

        moveDistance = tr.FindProperty("moveDistance");
        canBePushed = tr.FindProperty("canBePushed");
        canBePulled = tr.FindProperty("canBePulled");
        moveAlongX_Axis = tr.FindProperty("moveAlongX_Axis");

        toggleOnOrOff = tr.FindProperty("toggleOnOrOff");

        requiredItem = tr.FindProperty("requiredItem");
        inventory = tr.FindProperty("inventory");

        statManager = tr.FindProperty("statManager");
        selectedStatus = tr.FindProperty("selectedStatus");
        enableOrDisable = tr.FindProperty("enableOrDisable");

        soundActivationEventName = tr.FindProperty("soundActivationEventName");
        soundMovementLoop = tr.FindProperty("soundMovementLoop");

        /* Add your properties beneath */

    }

    /// <summary>
    /// In order to make your properties show on Editor, you need to add EditorGUILayout.PropertyField(youProperty) to enable visibility.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        tr.Update();
        EditorGUILayout.PropertyField(selectedTriggerEvent);

        switch (selectedTriggerEvent.enumValueIndex)
        {
            // Trigger animation clip.
            case 0:
                EditorGUILayout.PropertyField(animator);
                EditorGUILayout.PropertyField(stateName);
                EditorGUILayout.PropertyField(m_layer);
                EditorGUILayout.PropertyField(playSpeed);
                break;
            // Destroy gameobject.
            case 1:
                EditorGUILayout.PropertyField(secondsBeforeDestroy);
                EditorGUILayout.PropertyField(soundActivationEventName);
                break;
            // Move Platform.
            case 2:
                EditorGUILayout.PropertyField(selectedMoveBehavior);
                EditorGUILayout.PropertyField(ezPlatform);

                EditorGUILayout.PropertyField(waypoints, true);

                EditorGUILayout.PropertyField(delaySecondsToNextWp);
                EditorGUILayout.PropertyField(isAligned);
                EditorGUILayout.PropertyField(isRotate);
                EditorGUILayout.PropertyField(useConsistentSpeed);
                if (useConsistentSpeed.boolValue)
                    EditorGUILayout.PropertyField(moveSpeed);
                else
                    EditorGUILayout.PropertyField(timeToNextWp);

                EditorGUILayout.PropertyField(soundActivationEventName);
                EditorGUILayout.PropertyField(soundMovementLoop);
                break;
            // Change Character Health
            case 3:
                EditorGUILayout.PropertyField(playerStatManager);
                EditorGUILayout.PropertyField(healthChange);
                EditorGUILayout.PropertyField(soundActivationEventName);
                break;
            // Time Challenge
            case 4:
                EditorGUILayout.PropertyField(delayBeforeChallengeStart);
                EditorGUILayout.PropertyField(timeLimit);
                EditorGUILayout.PropertyField(soundActivationEventName);
                break;
            // Play An Audio Clip
            case 5:
                EditorGUILayout.PropertyField(delayAudioPlay);
                EditorGUILayout.PropertyField(soundActivationEventName);
                break;
            // Instantiate An Object 
            case 6:
                EditorGUILayout.PropertyField(selectedObjType);
                EditorGUILayout.PropertyField(prefab);
                EditorGUILayout.PropertyField(baseOnTargetPos);
                if (baseOnTargetPos.boolValue)
                    EditorGUILayout.PropertyField(SetTargetAsParent);
                else
                    EditorGUILayout.PropertyField(instantiatePos);
                EditorGUILayout.PropertyField(soundActivationEventName);
                // If selected instantiate gameobject is camera
                if (selectedObjType.enumValueIndex == 0)
                {
                    EditorGUILayout.PropertyField(cameraFocus);
                    EditorGUILayout.PropertyField(cutsceneDuration);
                    EditorGUILayout.PropertyField(useFadeEffect);
                    if (useFadeEffect.boolValue)
                    {
                        EditorGUILayout.PropertyField(uiCanvas);
                        EditorGUILayout.PropertyField(fadeMask);
                        EditorGUILayout.PropertyField(fadeSpeed);
                    }
                    EditorGUILayout.PropertyField(useMovingFocus);
                    if (useMovingFocus.boolValue)
                    {
                        EditorGUILayout.PropertyField(focusWaypoints, true);

                        EditorGUILayout.PropertyField(focusUseConsistentSpeed);

                        if (focusUseConsistentSpeed.boolValue)
                            EditorGUILayout.PropertyField(focusMovespeed);
                        else
                            EditorGUILayout.PropertyField(f_timeToNextWaypoint);
                    }
                    EditorGUILayout.PropertyField(useMovingCamera);
                    if (useMovingCamera.boolValue)
                    {
                        EditorGUILayout.PropertyField(cameraWaypoints, true);

                        EditorGUILayout.PropertyField(cameraUseConsistentSpeed);

                        if (cameraUseConsistentSpeed.boolValue)
                            EditorGUILayout.PropertyField(cameraMovespeed);
                        else
                            EditorGUILayout.PropertyField(c_timeToNextWaypoint);
                    }
                }

                break;

            // Move box.
            case 7:
                EditorGUILayout.PropertyField(animator);
                EditorGUILayout.PropertyField(playSpeed);
                EditorGUILayout.PropertyField(moveSpeed);
                EditorGUILayout.PropertyField(moveDistance);
                EditorGUILayout.PropertyField(canBePushed);
                EditorGUILayout.PropertyField(canBePulled);
                EditorGUILayout.PropertyField(moveAlongX_Axis);
                EditorGUILayout.PropertyField(soundActivationEventName);
                break;
            // Toggle object.
            case 8:
                EditorGUILayout.PropertyField(toggleOnOrOff);
                break;
            // Use items.
            case 9:
                EditorGUILayout.PropertyField(requiredItem, true);
                EditorGUILayout.PropertyField(inventory);
                break;
            // Change player status
            case 10:
                EditorGUILayout.PropertyField(statManager);
                EditorGUILayout.PropertyField(selectedStatus);
                EditorGUILayout.PropertyField(enableOrDisable);
                break;
            default:
                break;

                /* Add your properties beneath */
        }

        tr.ApplyModifiedProperties();

        TriggerReceiver trRef = target as TriggerReceiver;

        if (GUILayout.Button("Reset Trigger"))
            trRef.ResetTrigger();
    }

}
