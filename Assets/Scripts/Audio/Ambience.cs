/*
 * UCSC Level Design Toolkit
 * 
 * James Zolyak
 * jameszolyak@gmail.com
 * 
 * This script handles playing simple ambiances.
 * 
 * Released under MIT Open Source License
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ambience : MonoBehaviour {

    public string AmbienceName;

	// Use this for initialization
	void Start () {
		if(AmbienceName != "")
        {
            AudioManager.PlaySound(AmbienceName, gameObject);
        }
    }
}
