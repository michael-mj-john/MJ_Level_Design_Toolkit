using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorControllerScript : MonoBehaviour {

    private Animator animator;

    // Use this for initialization
    void Start () {
        animator = GetComponentInChildren<Animator>();
    }
	
	public void stopMovement()
    {
        animator.SetBool("Run Forward", false);
        animator.SetBool("Walk Forward", false);
    }

    public void startWalking()
    {
        animator.SetBool("Walk Forward", true);
        animator.SetBool("Run Forward", false);
    }

    public void startRunning()
    {
        animator.SetBool("Walk Forward", false);
        animator.SetBool("Run Forward", true);
    }

    public void die()
    {
        animator.Play("Die");
    }
    
    public void playAttack1()
    {
        animator.SetBool("Punch Attack", true);
    }

    public void playGetHit()
    {
        animator.Play("Take Damage");
    }
}
