using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    [Header("Enemy Base Status")]
    private DropSystem dropSystem;
    private PlayerInventory playerInventory;
    public BaseStatus status = new BaseStatus(10, 5, 1);

    private CardInventory cardInventory;

    private void Start()
    {
        // ※ シーン内にDropSystemは1つだけ付いている前提
        dropSystem = FindFirstObjectByType<DropSystem>();
        playerInventory = FindFirstObjectByType<PlayerInventory>();
        // ※ プレイヤーがシーンに1人いる前提

        cardInventory = FindFirstObjectByType<CardInventory>();

        if (dropSystem == null)
            Debug.LogError($"{name}: DropSystem が付いていません");

        if (playerInventory == null)
            Debug.LogError("PlayerInventory がシーンに存在しません");

        if (cardInventory == null)
            Debug.LogError("CardInventory がシーンに存在しません");
    }


    public void TakeDamage(int amount)
    {
        status.TakeDamage(amount);
        Debug.Log($"敵HP: {status.HP}");

        if (status.IsDead())
        {
            Debug.Log("敵死亡！");

            // ★ UnitManager のリストから削除
            UnitManager.instance.enemies.Remove(gameObject);

            Die();
        }
    }

    /// <summary>
    /// 敵が死亡したときの処理
    /// </summary>
    private void Die()
    {
        Debug.Log($"{name} は倒れた！");

        EnemyMovement mv = GetComponent<EnemyMovement>();
        if (mv != null)
        {
            UnitManager.instance.UnregisterEnemy(mv);
        }

        //// 通常ドロップ（必要なら）
        //var dropItem = dropSystem.GetWeightedDrop();
        //if (dropItem != null)
        //    playerInventory.AddItem(dropItem);

        CardType? type = dropSystem.GetCardRewardType();

        if (!type.HasValue)
        {
            Debug.Log("カード報酬なし");
            Destroy(gameObject);
            return;
        }

        // ★ インベントリ満杯チェック
        if (IsInventoryFull(type.Value))
        {
            Debug.Log($"カードインベントリ満杯のため報酬スキップ: {type.Value}");
            Destroy(gameObject);
            return;
        }

        Debug.Log($"カード報酬タイプ: {type.Value}");

        var ui = FindFirstObjectByType<CardInventoryUIController>();
        if (ui != null)
            ui.ShowRandomSelect(type.Value);

        Destroy(gameObject);
    }

    bool IsInventoryFull(CardType type)
    {
        if (cardInventory == null) return true;

        if (type == CardType.Passive)
            return cardInventory.passiveCards.Count >= cardInventory.passiveLimit;

        return cardInventory.consumableCards.Count >= cardInventory.consumableLimit;
    }
}
