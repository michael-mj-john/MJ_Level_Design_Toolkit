using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTutorialLevel : MonoBehaviour {

    GUIContent content;
    GUIStyle style = new GUIStyle();
    bool showWindows = false;
    string tutorialText;
    public string[] eventsToToggle;
    public TextMesh text;
    // Use this for initialization
    void Start () {
        //content = new GUIContent(tutorialText,  "This is a tooltip");
        // Position the Text in the center of the Box
        //style.alignment = TextAnchor.MiddleCenter;
        tutorialText = text.text;
        text.text = "";
    }
	
	// Update is called once per frame
	void Update () {
        

    }

    //A C# example
    [ExecuteInEditMode]
    void OnGUI()
    {
        //if(showWindows)
        //{
        //    Vector2 worldPoint = Camera.main.WorldToScreenPoint(transform.position);
        //    //GUI.Box(new Rect(worldPoint.x - 100, (Screen.height - worldPoint.y) - 50, 200, 100), content, style);
            
        //}
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            text.text = tutorialText;
            foreach (string e in eventsToToggle)
            {
                AudioManager.PlaySound(e, gameObject);
            }
        }
    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            text.text = "";
            foreach (string e in eventsToToggle)
            {
                AudioManager.StopAllSounds(e);
            }
        }

    }
}
