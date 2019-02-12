using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    public CheckPointsManager checkPointsManager;

    public int checkpointID;
    public bool isActivate = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivate)
        {
            checkPointsManager.SetCurrentCheckpoint(checkpointID);
            checkPointsManager.SaveData();
            isActivate = true;
        }
    }
}


