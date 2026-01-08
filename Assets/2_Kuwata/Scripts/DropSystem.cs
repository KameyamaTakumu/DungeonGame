using UnityEngine;

public class DropSystem : MonoBehaviour
{
    [Header("通常ドロップ")]
    public DropTableSO dropTable;

    [Header("カード報酬")]
    public CardDataBase cardDatabase;

    [Header("カード報酬設定")]

    [Tooltip("ここまでは確定でバフカード選択")]
    public int guaranteedPassiveCount = 3;

    [Tooltip("4回目以降に消費カード選択が出る確率")]
    [Range(0f, 1f)]
    public float consumableDropChance = 0.5f;

    int cardDropCount = 0;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 通常アイテムの抽選（既存）
    /// </summary>
    public DropItemSO GetWeightedDrop()
    {
        if (dropTable == null)
            return null;

        int totalWeight = dropTable.noDropWeight;

        foreach (var item in dropTable.items)
            totalWeight += item.weight;

        int randomValue = Random.Range(0, totalWeight);

        if (randomValue < dropTable.noDropWeight)
            return null;

        randomValue -= dropTable.noDropWeight;

        foreach (var item in dropTable.items)
        {
            if (randomValue < item.weight)
                return item;

            randomValue -= item.weight;
        }

        return null;
    }

    /// <summary>
    /// 今回カード選択を出すか / どのタイプか
    /// </summary>
    /// <returns>
    /// Passive / Consumable / null(何も出さない)
    /// </returns>
    public CardType? GetCardRewardType()
    {
        cardDropCount++;

        // 1〜3回目：確定でバフ
        if (cardDropCount <= guaranteedPassiveCount)
        {
            return CardType.Buff;
        }

        // 4回目以降：消費カードのみ確率
        if (Random.value < consumableDropChance)
        {
            return CardType.Use;
        }

        // 出ない
        return null;
    }
}
