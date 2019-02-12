/*
 * UCSC Level Design Toolkit
 * 
 * Aidan Kennell
 * akennell94@gmail.com
 * 8/9/2018
 * 
 * Released under MIT Open Source License
 * 
 * This script is a framework for any Input devices. We use the frame work so that we can easily 
 * switch between two diffent types of controllers as long as they inherit from this class. For 
 * example, this structure allows us to easily take over the player character by switching out
 * its character input for an AI input. Switching those inputs might be useful for cutscenes, for
 * example.
 */

public class VirtualInput{
#region Member Variables
    public float heading;
    public float throttle;
    public float deadZone;
    public bool jump;
    public bool attack;
    public bool doubleJump;
    public bool isGrounded;
    public bool freezeMovement;
#endregion

    public VirtualInput()
    {
        heading = 0.0f;
        isGrounded = true;
        freezeMovement = false;
        clear();
    }

    public void clear()
    {
        attack = false;
        jump = false;
        doubleJump = false;
        throttle = 0.0f;
    }

    public virtual void read()
    {
        clear();
    }
}
