using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour {
    public float hitPoints = 100;
    AnimatorControllerScript animatorController;
	// Use this for initialization
	void Start () {
        animatorController = gameObject.GetComponent<AnimatorControllerScript>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void takeDamage(float damage)
    {
        hitPoints -= damage;
        animatorController.playGetHit();
    }

    public bool isDead()
    {
        return hitPoints <= 0;
    }
}
