using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// カードインベントリ（使い切り用 + バフ用）
/// - 上限に到達したら入れ替えモード(PendingCard)になる
/// - OnSwapRequested(CardData pending, CardType type) を発火
/// </summary>
public class CardInventory : MonoBehaviour
{
    public int consumableLimit = 2;
    public int passiveLimit = 2;

    public List<CardData> consumableCards = new List<CardData>();
    public List<CardData> passiveCards = new List<CardData>();

    public Action OnInventoryChanged;

    // swap mode
    public bool IsSwapMode { get; private set; } = false;
    public CardData PendingCard { get; private set; } = null;
    public CardType PendingCardType { get; private set; }

    // UIに「入れ替え開始」を通知するためのイベント
    public Action<CardData, CardType> OnSwapRequested;

    /// <summary>
    /// カード追加（上限チェック→追加 or 入替モード）
    /// </summary>
    public void AddCard(CardData card)
    {
        List<CardData> list = GetList(card.cardType);
        int limit = GetLimit(card.cardType);

        if (list.Count < limit)
        {
            list.Add(card);
            if (card.cardType == CardType.Passive)
                ApplyPassiveEffect(card);

            OnInventoryChanged?.Invoke();
        }
        else
        {
            StartSwapMode(card, card.cardType);
        }
    }

    List<CardData> GetList(CardType type)
    {
        return type == CardType.Consumable ? consumableCards : passiveCards;
    }

    int GetLimit(CardType type)
    {
        return type == CardType.Consumable ? consumableLimit : passiveLimit;
    }

    void StartSwapMode(CardData card, CardType type)
    {
        IsSwapMode = true;
        PendingCard = card;
        PendingCardType = type;
        Debug.Log($"入れ替えモード開始: {card.cardName} ({type})");
        OnSwapRequested?.Invoke(card, type);
    }

    public void ReplaceCardAt(int index)
    {
        if (!IsSwapMode || PendingCard == null)
            return;

        var list = GetList(PendingCardType);

        if (index < 0 || index >= list.Count)
            return;

        var old = list[index];

        // パッシブなら解除
        if (PendingCardType == CardType.Passive)
            RemovePassiveEffect(old);

        // 差し替え
        list[index] = PendingCard;

        // パッシブなら新しい効果適用
        if (PendingCardType == CardType.Passive)
            ApplyPassiveEffect(PendingCard);

        EndSwapMode();
        OnInventoryChanged?.Invoke();
    }

    void EndSwapMode()
    {
        IsSwapMode = false;
        PendingCard = null;
        // PendingCardTypeは残しても良いが初期化しておく
        PendingCardType = default;
        Debug.Log("入れ替えモード終了");
    }

    void ApplyPassiveEffect(CardData passiveCard)
    {
        // TODO: 実際のステータス反映をここに書く
        // 例：PlayerStatus.Instance.AddBuff(passiveCard);
    }

    void RemovePassiveEffect(CardData passiveCard)
    {
        // TODO: 古いパッシブの効果解除処理をここに書く
        // 例：PlayerStatus.Instance.RemoveBuff(passiveCard);
    }

    /// <summary>
    /// 使い切りカードの使用（通常の使用）
    /// </summary>
    public void UseConsumableCard(int index)
    {
        if (index < 0 || index >= consumableCards.Count) return;
        if (IsSwapMode)
        {
            // 入れ替えモード中に直接「使用」するのは混乱するので禁止にしておく
            Debug.Log("入れ替えモード中は直接使用できません。スロットを選んで入れ替えてください。");
            return;
        }

        var card = consumableCards[index];

        // 技発動（プレイヤーに通知）
        FindFirstObjectByType<PlayerSkillExecutor>()?.ExecuteCardSkill(card);

        // 削除
        consumableCards.RemoveAt(index);

        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 入れ替えをキャンセルしたい（UIから呼べる）
    /// </summary>
    public void CancelSwap()
    {
        if (!IsSwapMode) return;
        Debug.Log("入れ替えキャンセル");
        EndSwapMode();
        // UI側は必要なら閉じる
    }
}
