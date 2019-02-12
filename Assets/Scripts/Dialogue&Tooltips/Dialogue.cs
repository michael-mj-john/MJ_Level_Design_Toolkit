/*
 * UCSC Level Design Toolkit
 * 
 * Aidan Kennell
 * akennell94@gmail.com
 * 8/9/2018
 * 
 * Released under MIT Open Source License
 * 
 * This script controls the set up and usage of any dialogue attached to the trigger that this script is attached to.
 * There needs to be some sort of panel in the scene with a text object that is a child of it. The panel needs to be 
 * tagged with the tag "Dialogue".
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour {
    public List<string> lines;
    [Range(50,400)]
    public int maxStringSize;
    public bool forceStartOnEnter = false;
    int dialogueIndex = 0;
    bool inDialogue = false;
    bool exitDialogue = false;
    bool unFreezePlayer = false;
    GameObject dialogueBox;
    Text displayedText;
    GameObject tooltips;
    Text tips;

	void Start ()
    {
        limitLineLength();
        dialogueBox = GameObject.FindGameObjectWithTag("Dialogue");
        displayedText = dialogueBox.GetComponentInChildren<Text>();
        tooltips = GameObject.FindGameObjectWithTag("Tooltips");
        tips = tooltips.GetComponentInChildren<Text>();
        dialogueBox.SetActive(false);
        tooltips.SetActive(false);
	}

	/*
     * I need to use FixedUpdate here because it runs before all of the OnTrigger functions. If this block of code ran after 
     * the OnTrigger functions then this script would always increment dialogueIndex right after the Submit button was pressed
     */
	void FixedUpdate ()
    {
        if (inDialogue && dialogueIndex < lines.Count)
        {
            displayedText.text = lines[dialogueIndex];
        }

        if (inDialogue && Input.GetButtonDown("Continue"))
        {
            dialogueIndex++;
        }

        if (dialogueIndex >= lines.Count)
        {
            exitDialogue = true;
            inDialogue = false;
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            tooltips.SetActive(true);
            tips.text = "A to Talk";

            if (forceStartOnEnter)
            {
                other.GetComponent<ControllerBase>().turnToPos(transform.position);
                other.GetComponent<ControllerBase>().freezeMovement();
                inDialogue = true;
                dialogueBox.SetActive(true);
                tips.text = "Y to Continue";
            }

        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player" && unFreezePlayer)
        {
            unFreezePlayer = false;
            exitDialogue = false;
            other.GetComponent<ControllerBase>().unFreezeMovement();
        }
        //If player is not in dialogue and they press the "Submit" button they are forzen and placed in dialogue
        if(other.tag == "Player" && Input.GetButtonDown("Submit") && !inDialogue && !exitDialogue && !forceStartOnEnter)
        {
            other.GetComponent<ControllerBase>().turnToPos(transform.position);
            other.GetComponent<ControllerBase>().freezeMovement();
            inDialogue = true;
            dialogueBox.SetActive(true);
            tips.text = "Y to Continue";
        }
        //If the player is in dialogue and press the "Cancel" buttont, or they reach the end of the dialogue 
        //they are unforzen and taken out of the dialogue screen
        else if((other.tag == "Player" && Input.GetButtonDown("Cancel") && inDialogue) || exitDialogue)
        {
            unFreezePlayer = true;
            dialogueIndex = 0;
            inDialogue = false;
            dialogueBox.SetActive(false);
            tips.text = "A to Talk";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            tooltips.SetActive(false);
        }
    }

    /*
     * Requires: Nothing
     * Modifies: Lines(List<string>)
     * Returns: Nothing
     * 
     * When this function is done running it should have pulled appart any strings, in the member variable lines, that are longer than
     * maxStringSize, and then made put them back in order so that everyline's character count is under the max.
     */
    void limitLineLength()
    {
        //Create a temporary list to hold all of the split up strings
        List<string> newLines = new List<string>();
        string[] _lines = lines.ToArray();

        for(int i = 0; i < _lines.Length; i++)
        {
            //If the line is under the max, just add it to the list
            if(_lines[i].Length <= maxStringSize)
            {
                newLines.Add(_lines[i]);
            }
            //if the line is over the max, split it up
            else
            {
                string[] splits = splitString(_lines[i]);

                //Add each split to the list
                for(int j = 0; j < splits.Length; j++)
                {
                    //If the split is not the last split place a "..." to make it look like there is more to come
                    if(j != splits.Length - 1 && !splits[j].EndsWith("."))
                    {
                        splits[j] += "...";
                    }

                    newLines.Add(splits[j]);
                }
            }
        }

        lines = newLines;
    }

    /*
     * Requires: A string to be passes to it
     * Modifies: Nothing
     * Returns: a string array that has split up the string that was passed in. The split up portions should be about the size of the maxStringSize.
     *          The string in each split section could be a little larger than the max. The function will prioritize keeping whole words over keeping the 
     *          character count under the max.
     */
    string[] splitString(string toSplit)
    {
        List<string> final = new List<string>();
        int currentCount = 0;
        char[] charSplit = toSplit.ToCharArray();

        for (int i = 0; i < toSplit.Length; i++)
        {
            if((currentCount >= maxStringSize && charSplit[i] == ' ') ||
                i == toSplit.Length - 1)
            {
                final.Add(toSplit.Substring(i - currentCount, currentCount));
                currentCount = -1;
            }

            currentCount++;
        }

        return final.ToArray();
    }
}
