using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameManagerScript : MonoBehaviour {

    public GameObject spawnPoint;
    public GameObject mainPlayer;
    public ThirdPersonCameraController thirPersonCamera;
    public List<PuzzleManagerScript> puzzleManagers = new List<PuzzleManagerScript>();

    public Inventory inventoryManager;
    public CheckPointsManager checkpointManager;

    public static GameManagerScript instance = null;
    
    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }

        checkpointManager.StartUp();
        inventoryManager.StartUp();
    }


    // Use this for initialization
    void Start () {
        spawnPlayer();
        //setupCamera();
	}
	
	// Update is called once per frame
	void Update () {
        handlePlayerHealth();
	}

    private void handlePlayerHealth()
    {
        float health = mainPlayer.GetComponent<StatManager>().GetCurrentHealth();
        if (health <= 0)
        {
            resetPuzzles();
            respawnPlayer();
        }
    }

    private void resetPuzzles()
    {
        foreach (PuzzleManagerScript puzzleManagerScript in puzzleManagers)
        {
            if (!puzzleManagerScript.isSolved())
            {
                puzzleManagerScript.resetPuzzle();
            }
        }
    }

    private void setupCamera()
    {
        thirPersonCamera.setCameraTarget(mainPlayer);
    }

    private void spawnPlayer()
    {
        if(mainPlayer == null)
        {
            Debug.Log("Main Player NOT set on GameManager.");
        }
        if(thirPersonCamera == null)
        {
            Debug.Log("Third Person Camera NOT set on GameManager.");
        }
        Vector3 offset = spawnPoint.transform.position - mainPlayer.transform.position;
        mainPlayer.transform.position += offset;
        mainPlayer.transform.rotation = spawnPoint.transform.rotation;
        thirPersonCamera.gameObject.transform.position += offset;
    }

    

    public void respawnPlayer()
    {
        mainPlayer.GetComponent<StatManager>().resetHealth();
        if (checkpointManager.GetCurrentCheckpoint() == -1)
        {
            mainPlayer.transform.position = spawnPoint.transform.position;
        }
        else {
            checkpointManager.respawnPlayerAtLatestCheckpoint();
        }
    }

    public GameObject getPlayer()
    {
        return mainPlayer;
    }

    public Camera getMainCamera()
    {
        return thirPersonCamera.getCamera();
    }

}
[CustomEditor(typeof(GameManagerScript))]
public class GMEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameManagerScript gmRef = target as GameManagerScript;

        if (GUILayout.Button("Quick Save"))
        {
            Debug.Log("On construction");
        }

        if (GUILayout.Button("Restart from last checkpoint"))
        {
            gmRef.checkpointManager.respawnPlayerAtLatestCheckpoint();
        }

        if (GUILayout.Button("Restart"))
        {
            DataManager.DeleteData();
            Application.LoadLevel(Application.loadedLevel);
        }
    }
    
}