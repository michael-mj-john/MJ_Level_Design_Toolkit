using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot: MonoBehaviour 
{
    public Item itemData;
    public Image itemImg;
    public Text itemAmout;
    public Sprite defaultItemImg;

    private void Awake()
    {
        itemData = new Item();
    }

    private void Update()
    {
        itemAmout.text = itemData.currentAmount.ToString();
    }

    public void SetItem(Item itemData)
    {
        if (itemData != null)
        {
            this.itemData = itemData;

            if (itemData.id != -1)
            {
                itemImg.name = itemData.title;
                itemImg.sprite = itemData.itemIcon;
                itemAmout.text = itemData.currentAmount.ToString();
            }
        }
    }

    public void ResetSlot()
    {
        itemData = new Item();
        itemImg.name = "NoItem";
        itemImg.sprite = defaultItemImg;
        itemAmout.text = "";
    }
}
