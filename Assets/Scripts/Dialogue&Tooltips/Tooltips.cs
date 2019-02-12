/*
 * UCSC Level Design Toolkit
 * 
 * Aidan Kennell
 * akennell94@gmail.com
 * 8/9/2018
 * 
 * Released under MIT Open Source License
 * 
 * This script controlls a trigger that will cause a tool tip to appear. The actual string for the tooltip will
 * need to be specified in the inspector.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltips : MonoBehaviour {
    public string tip;
    GameObject tooltips;
    Text tipText;

    //Initialize all variables
    private void Start()
    {
        tooltips = GameObject.FindGameObjectWithTag("Tooltips");
        tipText = tooltips.GetComponentInChildren<Text>();
    }

    //Activate tooltip if it is not active
    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player" && !tooltips.activeInHierarchy)
        {
            tooltips.SetActive(true);
            tipText.text = tip;
        }
    }

    //Deactivate tooltip
    private void OnTriggerExit(Collider other)
    {
        tooltips.SetActive(false);
    }
}
