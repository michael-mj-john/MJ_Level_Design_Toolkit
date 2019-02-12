using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public int id { get; private set; }
    public string title { get; private set; }
    public bool stackable { get; private set; }
    public string description { get; private set; }
    public int initialAmount { get; private set; }
    public int healthChanges { get; private set; }
    public string type { get; private set; }
    public int currentAmount;
    public Sprite itemIcon { get; private set; }

    public Item(
        int id, 
        string title, 
        bool stackable, 
        string description, 
        int initialAmount, 
        int healthChanges, 
        string type, 
        Sprite itemIcon
        )
    {
        this.id = id;
        this.title = title;
        this.stackable = stackable;
        this.description = description;
        this.initialAmount = initialAmount;
        this.healthChanges = healthChanges;
        this.type = type;
        this.itemIcon = itemIcon;
    }

    // If an item is not been initialized, this item will have -1 index in case been read.
    public Item()
    {
        id = -1;
    }
}
