using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTriggerScript : MonoBehaviour {

    private float attackDamage;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag.Equals("Player"))
        {
            col.gameObject.GetComponent<StatManager>().ChangeHealth(-attackDamage);
        }
    }

    public void setAttackDamage(float damage)
    {
        attackDamage = damage;
    }

   
}
