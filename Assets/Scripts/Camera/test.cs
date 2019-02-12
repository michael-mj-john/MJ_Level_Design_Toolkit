using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class test : MonoBehaviour {

    public Transform target;
	// Use this for initialization
	void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
        Debug.DrawLine(transform.position, target.position, Color.green);
        transform.LookAt(target);
    }

    private void OnValidate()
    {
        //transform.LookAt(target);
    }
 }
