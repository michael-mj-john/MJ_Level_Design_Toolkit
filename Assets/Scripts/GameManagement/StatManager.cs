using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatManager : MonoBehaviour {

    public float maxHealth = 100;
    public float attackDamage = 100;
    public float currentHealth;
    private bool isStealth = false;
    private bool canDoubleJump = true;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void ChangeHealth(float increment)
    {
        currentHealth += increment;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        if (currentHealth < 0)
            currentHealth = 0;
        
    }

    public void resetHealth()
    {
        currentHealth = maxHealth;
    }

    public void setStealth(bool isStealth)
    {
        this.isStealth = isStealth;
    }

    public bool getStealth()
    {
        return isStealth;
    }

    public void setCanDoubleJump(bool canDoubleJump)
    {
        this.canDoubleJump = canDoubleJump;
    }

    public bool getCanDOubleJump()
    {
        return canDoubleJump;
    }

    public float getAttackDamage()
    {
        return attackDamage;
    }
    

}
