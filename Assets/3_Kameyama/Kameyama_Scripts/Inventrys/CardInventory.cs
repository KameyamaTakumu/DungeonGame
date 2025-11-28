using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// カードインベントリ（使い切り用 + バフ用）
/// </summary>
public class CardInventory : MonoBehaviour
{
    // 使い切りカード
    public List<CardData> consumableCards = new List<CardData>();

    // バフカード
    public List<CardData> passiveCards = new List<CardData>();

    // UIへ更新を通知
    public Action OnInventoryChanged;

    /// <summary>
    /// カード追加
    /// </summary>
    public void AddCard(CardData card)
    {
        if (card.cardType == CardType.Consumable)
        {
            consumableCards.Add(card);
            Debug.Log($"使い切りカードを追加: {card.cardName}");
        }
        else
        {
            passiveCards.Add(card);
            Debug.Log($"常時発動カードを追加: {card.cardName}");
            ApplyPassiveEffect(card); // バフ効果の発動
        }

        OnInventoryChanged?.Invoke(); // ← UI更新！！
    }

    /// <summary>
    /// 常時発動カードの効果を適用
    /// </summary>
    void ApplyPassiveEffect(CardData passiveCard)
    {
        // ★ ここで PlayerStatus などにバフを適用する
        // （例：攻撃力+10、防御アップなど）
    }

    /// <summary>
    /// 使い切りカードの使用
    /// </summary>
    public void UseConsumableCard(int index)
    {
        if (index < 0 || index >= consumableCards.Count) return;

        var card = consumableCards[index];

        // 技発動（プレイヤーに通知）
        FindFirstObjectByType<PlayerSkillExecutor>()?.ExecuteCardSkill(card);

        // 削除
        consumableCards.RemoveAt(index);

        OnInventoryChanged?.Invoke(); // UI更新
    }
}
