using UnityEngine;

public class DropSystem : MonoBehaviour
{
    public DropTableSO dropTable;

    public DropItemSO GetWeightedDrop()
    {
        if (dropTable == null)
        {
            Debug.LogWarning("DropTableが設定されていません");
            return null;
        }

        int totalWeight = dropTable.noDropWeight;

        foreach (var item in dropTable.items)
        {
            if (item.weight > 0)
                totalWeight += item.weight;
        }

        if (totalWeight <= 0)
        {
            Debug.LogWarning("有効な重みがありません");
            return null;
        }

        int randomValue = Random.Range(0, totalWeight);

        // まずドロップ無しを判定
        if (randomValue < dropTable.noDropWeight)
        {
            return null; // ドロップ無し
        }

        randomValue -= dropTable.noDropWeight;

        // ドロップアイテム判定
        foreach (var item in dropTable.items)
        {
            if (randomValue < item.weight)
                return item;

            randomValue -= item.weight;
        }

        return null;
    }
}
