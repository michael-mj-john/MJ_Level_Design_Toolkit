﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour {

    #region inspector properties    

    [Tooltip("The target pivot that this camera follows and looks at. Suggest to be an empty object under the character object.")]
    public Transform target;
    [Tooltip("The renderer that the target character uses.")]
    public Renderer targetRender;
    [Tooltip("The camera under this manager.")]
    public Camera myCamera;

    public float rotationSmoothTime = .12f;
    public float xSensitivity = 3f;
    public float ySensitivity = 3f;
    [Tooltip("Minimum of Y-axis rotation")]
    public float yMinLimit = 10f;
    [Tooltip("Maximum of Y-axis rotation")]
    public float yMaxLimit = 40f;
    [System.Serializable]
    public struct axis {
        [Tooltip("If enabled, the positive input will send negative values to the axis, and vice versa.")]
        public bool x;
        public bool y;
    }
    public axis invertAxis;
    [Tooltip("Offset between camera target and screen center.")]
    public Vector2 offsetToScreenCenter;

    [Header("Obstacle Detection")]
    [Tooltip("The Unity layer mask against which the collider will raycast")]
    public LayerMask collideAgainst = 1;
    [Tooltip("Obstacles with this tag will be ignored.  It is a good idea to set this field to the target's tag")]
    public string ignoreTag = string.Empty;
    [Tooltip("Obstacles closer to the target than this will be ignored")]
    public float minDistanceFromTarget = 0f;
    [Tooltip("The maximum raycast distance when checking if the line of sight to this camera's target is clear.  If the setting is 0 or less, the current actual distance to target will be used.")]
    public float rayDistanceLimit = 0f;

    [Header("Character Transparency")]
    [Tooltip("When the distance is smaller than this, the character starts to become transparent. Should > maxDistTransparent")]
    public float minDistOpaque = 0.8f;
    [Tooltip("When the distance is smaller than this, the character will become completely transparent. Should < minDistOpaque")]
    public float maxDistTransparent = 0.5f;

    #endregion

    #region hide properties    

    // Distance between camera and target
    float distance;
    float rotationX;
    float rotationY;
    Vector3 currentSmoothVelocity = Vector3.zero;
    Vector3 currentPositionVelocity = Vector3.zero;
    Vector3 currentRotation;
    const float epsilon = 0.0001f;
    // This must be small but greater than 0 - reduces false results due to precision
    const float precisionSlush = 0.001f;
    float lastDistance;
    float occlusionOffset = 1.0f;
    float occlusionRotationMod = 0.35f;//0.35f;
    bool rotatePos = false;
    bool rotateNeg = false;
    float rotationTimmer = 0.0f;
    Vector2 lastOffset;
    float positionSmoothTime = .03f;
    [HideInInspector]
    public static bool ignoreInput = false;
    [HideInInspector]
    public static bool disableRotation = false;

    #endregion

    private void Start () {
        transform.LookAt(target);
        rotationX = transform.eulerAngles.y;
        rotationY = transform.eulerAngles.x;
        distance = Vector3.Distance(transform.position,target.position);
        currentRotation = new Vector3(rotationY, rotationX, 0);
    }

    private void OnValidate()
    {
        if (myCamera) {
            if (myCamera.transform.parent != transform) {
                print("The camera should be under this manager!");
                myCamera = null;
            }
            else if(Camera.allCameras.Length >1)
            {
                foreach (Camera cam in Camera.allCameras)
                    if (!cam.Equals(myCamera))
                        cam.gameObject.SetActive(false);
            }
            
        }
        
        rotationSmoothTime = Mathf.Max(0, rotationSmoothTime);
        xSensitivity = Mathf.Max(0, xSensitivity);
        ySensitivity = Mathf.Max(0, ySensitivity);
        minDistOpaque = Mathf.Max(minDistOpaque, maxDistTransparent);
        maxDistTransparent = Mathf.Min(minDistOpaque, maxDistTransparent);
        myCamera.transform.localPosition = new Vector3(offsetToScreenCenter.x, offsetToScreenCenter.y, 0);
    }

    private void LateUpdate()
    {
        if (!target || !targetRender || !myCamera) {
            print("Camera manager can't find target/targetRender/myCamera !");
            return;
        }
            

        if (!ignoreInput)
        {
            if (!invertAxis.x)
            {
                rotationX += Input.GetAxis("Mouse X") * xSensitivity;
            }
            else
            {
                rotationX += -Input.GetAxis("Mouse X") * xSensitivity;
            }

            if (!invertAxis.y)
            {
                rotationY -= Input.GetAxis("Mouse Y") * ySensitivity;
            }
            else
            {
                rotationY -= -Input.GetAxis("Mouse Y") * ySensitivity;
            }
            rotationY = Mathf.Clamp(rotationY, yMinLimit, yMaxLimit);
            currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(rotationY, rotationX, 0), ref currentSmoothVelocity, rotationSmoothTime);
            transform.eulerAngles = currentRotation;
        }

        Vector3 calculatedPosition = target.position - transform.forward * distance;
        //Debug.DrawLine(calculatedPosition, target.position, Color.green);
        Vector3 cameraPosition = CheckOcclusion(calculatedPosition + myCamera.transform.localPosition);
        // if no occlusion happens
        if (cameraPosition == Vector3.zero) {
            transform.position = target.position - transform.forward * distance;
        }
        // if there is an occlusion
        else
        {
            Vector3 shortLine = cameraPosition - target.position;
            Vector3 longLine = calculatedPosition + myCamera.transform.localPosition - target.position;
            float displacement = shortLine.magnitude / longLine.magnitude * distance;
            Vector3 targetPosition = target.position - transform.forward * displacement;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentPositionVelocity, positionSmoothTime);
        }
        
        OcclusionRotation();

        // preserve the status before an occlusion, and adjust offset proportionally, in case the target stays outside the camera view
        if (cameraPosition != Vector3.zero)
        {
            float currentDistance = Vector3.Distance(transform.position, target.position);
            myCamera.transform.localPosition = new Vector3(currentDistance/lastDistance * lastOffset.x, currentDistance / lastDistance * lastOffset.y, 0);
        }
        else {
            lastDistance = distance;
            lastOffset = offsetToScreenCenter;
        }

        // adjust target's transparency when the camera is very close to it
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist < maxDistTransparent) targetRender.material.color = Color.clear;
        else if (dist > minDistOpaque) targetRender.material.color = Color.white;
        else targetRender.material.color = Color.Lerp(Color.white, Color.clear, (minDistOpaque - dist)/(minDistOpaque - maxDistTransparent));
    }

    private Vector3 CheckOcclusion(Vector3 calculatedCamPos)
    {
        /*
         *Offset the direction so that we are looking at the top of the player, this ensures that 
         *we are not running the occlusion code when we can still see the character's head
         */
        Vector3 targetPos = target.position+ new Vector3(0, occlusionOffset, 0);
        Vector3 resPos = Vector3.zero;
        Vector3 dir = calculatedCamPos - targetPos;

        float targetDistance = dir.magnitude;
        float _minDistanceFromTarget = Mathf.Max(minDistanceFromTarget, epsilon);
        if (targetDistance > _minDistanceFromTarget)
        {
            dir.Normalize();
            float rayLength = targetDistance - _minDistanceFromTarget;
            if (rayDistanceLimit > epsilon)
                rayLength = Mathf.Min(rayDistanceLimit, rayLength);

            // Make a ray that looks towards the camera, to get the most distant obstruction
            Ray ray = new Ray(calculatedCamPos - rayLength * dir, dir);
            rayLength += precisionSlush;
            if (rayLength > epsilon)
            {
                RaycastHit hitInfo;
                if (RaycastIgnoreTag(ray, out hitInfo, rayLength))
                {

                    //Use cross product to get the vector pointing ridectly to the right of the camera
                    //Vector3 right = Vector3.Cross(camera.forward.normalized, camera.up.normalized);
                    Vector3 negativeOffset = calculatedCamPos - myCamera.transform.right;
                    Vector3 positiveOffset = calculatedCamPos + myCamera.transform.right;

                    Vector3 negativeDir = negativeOffset - target.position;
                    Vector3 positiveDir = positiveOffset - target.position;
                    negativeDir.Normalize();
                    positiveDir.Normalize();

                    Ray negative = new Ray(negativeOffset - rayLength * negativeDir, negativeDir);
                    Ray positive = new Ray(positiveOffset - rayLength * positiveDir, positiveDir);

                    /*
                     * If the player is currently giving no input, check to see if the camera can rotate a little bit. If
                     * the camera can rotate, then rotate it. If the camera cannot rotate or the player is giving input, 
                     * then do the regular occlusion. (Shouldn't do rotation with ground layer)
                     */
                    RaycastHit hitInfo2;
                    if (hitInfo.collider.gameObject.layer != 9 && !RaycastIgnoreTag(negative, out hitInfo2, rayLength) && Input.GetAxis("Mouse X") == 0)
                    {
                        rotationTimmer = 0.0f;
                        rotateNeg = true;
                    }
                    else if (hitInfo.collider.gameObject.layer != 9 && !RaycastIgnoreTag(positive, out hitInfo2, rayLength) && Input.GetAxis("Mouse X") == 0)
                    {
                        rotationTimmer = 0.0f;
                        rotatePos = true;
                    }
                    else
                    {
                        // Pull camera forward in front of obstacle

                        float displacement = Mathf.Max(0, hitInfo.distance - precisionSlush);
                        resPos = ray.GetPoint(displacement);
                        Debug.DrawLine(calculatedCamPos - rayLength * dir, resPos, Color.red);
                    
                    }
                }
            }
        }   
        return resPos;
    }

    /*
     * Requires: Nothing
     * Modifies: totationTimmer(float), rotationX(float), rotateNeg(bool), and rotatePos(bool)
     * Returns: Nothing
     * 
     * This is ment to cause the camera to rotate for a set amount of time.
     */ 
    private void OcclusionRotation()
    {
        float maxTime = 0.3f; //0.3f;
        rotationTimmer += Time.deltaTime;

        if (rotateNeg && rotationTimmer <= maxTime)
        {
            rotationX -= xSensitivity * occlusionRotationMod;
        }
        else if (rotatePos && rotationTimmer <= maxTime)
        {
            rotationX += xSensitivity * occlusionRotationMod;
        }

        if(rotationTimmer > maxTime)
        {
            rotateNeg = false;
            rotatePos = false;
        }
    }

    private bool RaycastIgnoreTag(Ray ray, out RaycastHit hitInfo, float rayLength)
    {
        while (Physics.Raycast(
            ray, out hitInfo, rayLength, collideAgainst.value,
            QueryTriggerInteraction.Ignore))
        {
            if (ignoreTag.Length == 0 || !hitInfo.collider.CompareTag(ignoreTag))
                return true;

            // Pull ray origin forward in front of tagged obstacle
            Ray inverseRay = new Ray(ray.GetPoint(rayLength), -ray.direction);
            if (!hitInfo.collider.Raycast(inverseRay, out hitInfo, rayLength))
                break; // should never happen
            rayLength = hitInfo.distance - precisionSlush;
            if (rayLength < epsilon)
                break;
            ray.origin = inverseRay.GetPoint(rayLength);
        }
        return false;
    }

    public void setCameraTarget(GameObject cameraTarget)
    {
        target = cameraTarget.transform;
    }

    public Camera getCamera()
    {
        return myCamera;
    }
}
