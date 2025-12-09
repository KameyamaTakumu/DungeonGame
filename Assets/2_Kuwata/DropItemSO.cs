using UnityEngine;

[CreateAssetMenu(fileName = "DropItem", menuName = "Drop/Drop Item")]
public class DropItemSO : ScriptableObject
{
    public string itemName;
    public int weight;
    public GameObject prefab;
}