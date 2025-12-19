using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public List<DropItemSO> items = new List<DropItemSO>();

    public void AddItem(DropItemSO item)
    {
        items.Add(item);
        Debug.Log($"{item.itemName} ‚ð“üŽè‚µ‚Ü‚µ‚½");
    }
}
