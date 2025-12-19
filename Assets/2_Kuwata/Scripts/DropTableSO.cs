using UnityEngine;

[CreateAssetMenu(fileName = "DropTable", menuName = "Drop/Drop Table")]
public class DropTableSO : ScriptableObject
{
    public DropItemSO[] items;

    [Header("ドロップ無しの重み")]
    public int noDropWeight = 0;  // 0なら無し
}