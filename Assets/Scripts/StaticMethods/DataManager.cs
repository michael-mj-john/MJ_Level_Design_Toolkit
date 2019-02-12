using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataManager
{
    #region Inventory Data
    public static bool CheckInventoryDataExist()
    {
        return PlayerPrefsX.GetBool("haveInventorySaveData");
    }

    public static void SaveInventoryData(Inventory inventory)
    {
        if (inventory != null)
        {
            List<Item> itemSet = new List<Item>();
            itemSet = inventory.GetInventoryData();

            int[] itemAmount = new int[itemSet.Count];
            int[] itemID = new int[itemSet.Count];

            for (int i = 0; i < itemSet.Count; i++)
            {
                itemID[i] = itemSet[i].id;
                itemAmount[i] = itemSet[i].currentAmount;
            }

            PlayerPrefsX.SetIntArray("itemID", itemID);
            PlayerPrefsX.SetIntArray("itemAmont", itemAmount);

            PlayerPrefsX.SetBool("haveInventorySaveData", true);

            Debug.Log("Save Completed.");
        }
    }

    public static int[] LoadInventoryDataID()
    {
        Debug.Log("Load ID Completed.");
        return PlayerPrefsX.GetIntArray("itemID");
    }

    public static int[] LoadInventoryDataAmount()
    {
        Debug.Log("Load Data Amount Completed.");
        return PlayerPrefsX.GetIntArray("itemAmont");
    }

    #endregion Inventory Data

    #region Checkpoint
    public static bool CheckCheckpointDataExist()
    {
        return PlayerPrefsX.GetBool("haveCheckpointSaveData");
    }

    public static void SaveCheckpointData(CheckPointsManager checkPointsManager)
    {
        PlayerPrefsX.SetBool("haveCheckpointSaveData", true);

        List<Checkpoint> checkpointList = checkPointsManager.GetCheckpointList();
        int[] checkpointID = new int[checkpointList.Count];
        bool[] checkpointActivationStatus = new bool[checkpointList.Count];

        for (int i = 0; i < checkpointList.Count; i++)
        {
            checkpointID[i] = checkpointList[i].checkpointID;
            checkpointActivationStatus[i] = checkpointList[i].isActivate;
        }

        PlayerPrefs.SetInt("currentCheckpoint", checkPointsManager.GetCurrentCheckpoint());
        PlayerPrefsX.SetIntArray("checkpointID", checkpointID);
        PlayerPrefsX.SetBoolArray("checkpointActivationStatus", checkpointActivationStatus);

    }

    public static int GetCurrentCheckpoint()
    {
        return PlayerPrefs.GetInt("currentCheckpoint");
    }

    public static int[] LoadCheckpointID()
    {
        return PlayerPrefsX.GetIntArray("checkpointID");
    }

    public static bool[] LoadCheckpointActivationStatus()
    {
        return PlayerPrefsX.GetBoolArray("checkpointActivationStatus");
    }
    #endregion Checkpoint


    public static void DeleteData()
    {
        Debug.Log("All Saved Data Deleted.");
        PlayerPrefs.DeleteAll();
    }
}
