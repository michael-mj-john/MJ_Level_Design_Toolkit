/*
 * UCSC Level Design Toolkit
 * 
 * James Zolyak
 * jameszolyak@gmail.com
 * 
 * This script handles playing footstep sounds. Every footfall the animation plays, triggers an animation event 
 * that calls the function below.
 * 
 * Released under MIT Open Source License
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour {

    public string FootStepEvent;
    
    // Function that plays audio based on animation events
    // Depending on the tag of the material, along with the ground layer, a different footstep sound will play.
    void PlayFootstep()
    {
        RaycastHit objectHit = new RaycastHit();
        float raycastDistance = 1000.0f;
        int layerMask = 1 << 9;

        if (Physics.Raycast(transform.position, Vector3.down, out objectHit, raycastDistance, layerMask))
        {
            Debug.Log(objectHit.transform.tag);
            if(objectHit.transform.tag == "Sand")
            {
                Fabric.EventManager.Instance.SetParameter("SFX/GrassWalk", "GrassVolume", 0, gameObject);
                Fabric.EventManager.Instance.SetParameter("SFX/StoneWalk", "StoneVolume", 0, gameObject);
                Fabric.EventManager.Instance.SetParameter("SFX/SandWalk", "SandVolume", 1, gameObject);
                Fabric.EventManager.Instance.SetParameter("SFX/WoodWalk", "WoodVolume", 0, gameObject);
            }
            else if (objectHit.transform.tag == "Stone")
            {
                Fabric.EventManager.Instance.SetParameter("SFX/GrassWalk", "GrassVolume", 0, gameObject);
                Fabric.EventManager.Instance.SetParameter("SFX/StoneWalk", "StoneVolume", 1, gameObject);
                Fabric.EventManager.Instance.SetParameter("SFX/SandWalk", "SandVolume", 0, gameObject);
                Fabric.EventManager.Instance.SetParameter("SFX/WoodWalk", "WoodVolume", 0, gameObject);
            }
            else if (objectHit.transform.tag == "Grass")
            {
                Fabric.EventManager.Instance.SetParameter("SFX/GrassWalk", "GrassVolume", 1, gameObject);
                Fabric.EventManager.Instance.SetParameter("SFX/StoneWalk", "StoneVolume", 0, gameObject);
                Fabric.EventManager.Instance.SetParameter("SFX/SandWalk", "SandVolume", 0, gameObject);
                Fabric.EventManager.Instance.SetParameter("SFX/WoodWalk", "WoodVolume", 0, gameObject);
            }
            else if (objectHit.transform.tag == "Wood")
            {
                Fabric.EventManager.Instance.SetParameter("SFX/GrassWalk", "GrassVolume", 0, gameObject);
                Fabric.EventManager.Instance.SetParameter("SFX/StoneWalk", "StoneVolume", 0, gameObject);
                Fabric.EventManager.Instance.SetParameter("SFX/SandWalk", "SandVolume", 0, gameObject);
                Fabric.EventManager.Instance.SetParameter("SFX/WoodWalk", "WoodVolume", 1, gameObject);
            }

            AudioManager.PlaySound(FootStepEvent, gameObject);
        }
    }
}
