using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using LitJson;

public class Inventory : MonoBehaviour
{
    private JsonData jsonItemDB;
    public string itemDatabaseLocation = "/ItemDatabase.json";
    public string itemSpriteFolderLocaition = "InventorySprites/";
    private List<Item> itemDatabase = new List<Item>();

    [Header("Inventory panel setting")]
    public GameObject slotPrefab;
    public GameObject invPanel;
    public GameObject slotPanel;
    public const int backpackCapacity = 25;
    private List<GameObject> slotSet = new List<GameObject>();

    public void StartUp()
    {
        ConstructItemDatabase();
        InitializeInventoryUI();
        InitializeBackpack();
    }

    public void LoadLastCheckpointBackpack()
    {
        ClearBackpack();
        InitializeBackpack();
    }

    private void ConstructItemDatabase()
    {
        jsonItemDB = JsonMapper.ToObject(File.ReadAllText(Application.streamingAssetsPath + itemDatabaseLocation));
        // Fetch item data from Json database.
        for (int i = 0; i < jsonItemDB.Count; i++)
        {
            itemDatabase.Add(new Item((int)jsonItemDB[i]["id"], 
                jsonItemDB[i]["title"].ToString(),
                (bool)jsonItemDB[i]["stackable"], 
                jsonItemDB[i]["description"].ToString(), 
                (int)jsonItemDB[i]["initialAmount"],
                (int)jsonItemDB[i]["healthChanges"],
                (string)jsonItemDB[i]["type"],
                Resources.Load<Sprite>(itemSpriteFolderLocaition + jsonItemDB[i]["title"].ToString())));

        }
        //Item item = new Item();
    }

    private void InitializeInventoryUI()
    {
        for (int i = 0; i < backpackCapacity; i++)
        {
            slotSet.Add(Instantiate(slotPrefab));
            slotSet[i].name = "Slot_" + i;
            slotSet[i].transform.SetParent(slotPanel.transform);
        }
    }

    private void InitializeBackpack()
    {       
        if (!DataManager.CheckInventoryDataExist())
        {
            for (int i = 0; i < itemDatabase.Count; i++)
            {
                if (itemDatabase[i].initialAmount > 0)
                {
                    for (int j = 0; j < itemDatabase[i].initialAmount; j++)
                        if (!AddItem(itemDatabase[i]))
                            print("Inventory is full.");
                }
            }
        }
        else
        {
            int[] loadItemID = new int[backpackCapacity];
            int[] loadItemAmount = new int[backpackCapacity];

            loadItemID = DataManager.LoadInventoryDataID();
            loadItemAmount = DataManager.LoadInventoryDataAmount();

            for (int i = 0; i < backpackCapacity; i++)
            {
                if (loadItemID[i] != -1)
                {
                    for (int j = 0; j < loadItemAmount[i]; j++)
                        AddItem(itemDatabase[loadItemID[i]]);
                }
            }
        }
    }

    private void ClearBackpack()
    {
        for (int i = 0; i < backpackCapacity; i++)
        {
            slotSet[i].GetComponent<Slot>().ResetSlot();
        }
    }

    public bool AddItem(Item item)
    {
        if (item.stackable)
        {
            for (int i = 0; i < backpackCapacity; i++)
            {
                if (slotSet[i].GetComponent<Slot>().itemData.id == -1)
                {
                    // Make sure there's no same item stack behind the current empty slot.
                    for (int j = i; j < backpackCapacity; j++)
                    {
                        if (slotSet[i].GetComponent<Slot>().itemData.id == item.id)
                        {
                            slotSet[i].GetComponent<Slot>().itemData.currentAmount += 1;
                            return true;
                        }
                    }
                    slotSet[i].GetComponent<Slot>().SetItem(item);
                    slotSet[i].GetComponent<Slot>().itemData.currentAmount = 1;
                    return true;
                }
                else
                {
                    if (slotSet[i].GetComponent<Slot>().itemData.id == item.id)
                    {
                        slotSet[i].GetComponent<Slot>().itemData.currentAmount += 1;
                        return true;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < backpackCapacity; i++)
            {
                if (slotSet[i].GetComponent<Slot>().itemData.id == -1)
                {
                    slotSet[i].GetComponent<Slot>().SetItem(item);
                    slotSet[i].GetComponent<Slot>().itemData.currentAmount = 1;
                    return true;
                }
            }
        }

        return false;
    }

    public bool CheckItemByID(int ID)
    {
        Item item = itemDatabase[ID];

        if (item == null)
        {
            print("ID is invalid.");
            return false;
        }
        else
            return CheckItem(item);
             
    }

    public bool CheckItemByTitle(string title)
    {
        for (int i = 0; i < itemDatabase.Count; ++i)
            if (itemDatabase[i].title == title)
                return CheckItem(itemDatabase[i]);

        return false;
    }

    private bool CheckItem(Item item)
    {
        for (int i = 0; i < backpackCapacity; ++i)
            if (slotSet[i].GetComponent<Slot>().itemData.id == item.id
                && slotSet[i].GetComponent<Slot>().itemData.currentAmount >= 1)
                    return true;
        return false;
    }

    public void RemoveItemByID(int ID)
    {
        Item item = itemDatabase[ID];

        if (item == null)
            print("ID is invalid.");
        else
            RemoveItem(item);
    }

    public void RemoveItemByTitle(string title)
    {
        for (int i = 0; i < itemDatabase.Count; ++i)
            if (itemDatabase[i].title == title)
            {
                RemoveItem(itemDatabase[i]);
                return;
            }

        print("Title is invalid.");
    }

    private void RemoveItem(Item item)
    {
        for (int i = 0; i < backpackCapacity; ++i)
        {
            if (slotSet[i].GetComponent<Slot>().itemData.id == item.id)
            {
                if (slotSet[i].GetComponent<Slot>().itemData.currentAmount > 1)
                {
                    slotSet[i].GetComponent<Slot>().itemData.currentAmount -= 1;
                }
                // Remove this item.
                if (slotSet[i].GetComponent<Slot>().itemData.currentAmount == 1)
                {
                    slotSet[i].GetComponent<Slot>().ResetSlot();
                }
            }
        }
    }

    public void SetInventoryData(List<Item> itemSet)
    {
        if (itemSet != null)
        {
            for (int i = 0; i < backpackCapacity; ++i)
            {
                slotSet[i].GetComponent<Slot>().SetItem(itemSet[i]);
            }
        }      
    }

    public List<Item> GetInventoryData()
    {
        List<Item> tempItemList = new List<Item>();
        for (int i = 0; i < backpackCapacity; ++i)
            tempItemList.Add(slotSet[i].GetComponent<Slot>().itemData);
        return tempItemList;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            if (invPanel.activeSelf)
                invPanel.SetActive(false);
            else
                invPanel.SetActive(true);
        }
    }

  
}