using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointsManager : MonoBehaviour
{
    public Inventory inventory;
    [SerializeField]
    private List<Checkpoint> checkpointList;
    private int currentCheckpoint = -1;

    public void StartUp()
    {
        InitializeCheckpointData();
    }

    private void InitializeCheckpointData()
    {
        if (DataManager.CheckCheckpointDataExist())
        {
            int[] checkpointID = new int[checkpointList.Count];
            bool[] checkpointActivationStatus = new bool[checkpointList.Count];
            checkpointID = DataManager.LoadCheckpointID();
            checkpointActivationStatus = DataManager.LoadCheckpointActivationStatus();
            currentCheckpoint = DataManager.GetCurrentCheckpoint();

            for (int i = 0; i < this.checkpointList.Count; i++)
            {
                checkpointList[i].checkpointID = checkpointID[i];
                checkpointList[i].isActivate = checkpointActivationStatus[i];
            }

            for (int i = 0; i < this.checkpointList.Count; i++)
            {
                if (checkpointList[i].checkpointID == currentCheckpoint)
                {
                    GameManagerScript.instance.getPlayer().transform.position = checkpointList[i].transform.position;
                    break;
                }
            }
        }
    }

    public void SaveData()
    {
        DataManager.SaveCheckpointData(this);
        if (inventory)
            DataManager.SaveInventoryData(inventory);
        else
            print("Inventory system not found.");
    }

    public void SetCurrentCheckpoint(int currentCheckpoint)
    {
        this.currentCheckpoint = currentCheckpoint;
    }

    public int GetCurrentCheckpoint()
    {
        return currentCheckpoint;
    }

    public List<Checkpoint> GetCheckpointList()
    {
        return checkpointList;
    }

    public void respawnPlayerAtLatestCheckpoint()
    {
        GameManagerScript.instance.getPlayer().transform.position = checkpointList[GetCurrentCheckpoint()].transform.position;
        inventory.LoadLastCheckpointBackpack();
    }

}
