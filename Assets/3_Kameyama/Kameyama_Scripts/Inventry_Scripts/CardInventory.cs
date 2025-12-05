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
    //// 使い切りカード
    //public List<CardData> consumableCards = new List<CardData>();

    //// バフカード
    //public List<CardData> passiveCards = new List<CardData>();

    //// UIへ更新を通知
    //public Action OnInventoryChanged;

    ///// <summary>
    ///// カード追加
    ///// </summary>
    //public void AddCard(CardData card)
    //{
    //    if (card.cardType == CardType.Consumable && consumableCards.Count < 2)
    //    {
    //        consumableCards.Add(card);
    //        Debug.Log($"使い切りカードを追加: {card.cardName}");
    //    }
    //    else if(card.cardType == CardType.Passive && passiveCards.Count < 2)
    //    {
    //        passiveCards.Add(card);
    //        Debug.Log($"常時発動カードを追加: {card.cardName}");
    //        ApplyPassiveEffect(card); // バフ効果の発動
    //    }

    //    OnInventoryChanged?.Invoke(); // ← UI更新！！
    //}

    ///// <summary>
    ///// 常時発動カードの効果を適用
    ///// </summary>
    //void ApplyPassiveEffect(CardData passiveCard)
    //{
    //    // ★ ここで PlayerStatus などにバフを適用する
    //    // （例：攻撃力+10、防御アップなど）
    //}

    ///// <summary>
    ///// 使い切りカードの使用
    ///// </summary>
    //public void UseConsumableCard(int index)
    //{
    //    if (index < 0 || index >= consumableCards.Count) return;

    //    var card = consumableCards[index];

    //    // 技発動（プレイヤーに通知）
    //    FindFirstObjectByType<PlayerSkillExecutor>()?.ExecuteCardSkill(card);

    //    // 削除
    //    consumableCards.RemoveAt(index);

    //    OnInventoryChanged?.Invoke(); // UI更新
    //}
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
        if (card == null) return;

        if (card.cardType == CardType.Consumable)
        {
            if (consumableCards.Count < consumableLimit)
            {
                consumableCards.Add(card);
                Debug.Log($"使い切りカードを追加: {card.cardName}");
                OnInventoryChanged?.Invoke();
            }
            else
            {
                // 上限到達 → 入れ替えモード
                StartSwapMode(card, CardType.Consumable);
            }
        }
        else // Passive
        {
            if (passiveCards.Count < passiveLimit)
            {
                passiveCards.Add(card);
                Debug.Log($"常時発動カードを追加: {card.cardName}");
                ApplyPassiveEffect(card);
                OnInventoryChanged?.Invoke();
            }
            else
            {
                StartSwapMode(card, CardType.Passive);
            }
        }
    }

    void StartSwapMode(CardData card, CardType type)
    {
        IsSwapMode = true;
        PendingCard = card;
        PendingCardType = type;
        Debug.Log($"入れ替えモード開始: {card.cardName} ({type})");
        OnSwapRequested?.Invoke(card, type);
    }

    /// <summary>
    /// 入れ替え実行：使い切りスロット index を指定して、PendingCard と入替える
    /// （※使い切りカードを選んで入替時は効果は発動しない）
    /// </summary>
    public void ReplaceConsumableAt(int index)
    {
        if (!IsSwapMode) return;
        if (PendingCard == null || PendingCardType != CardType.Consumable) return;
        if (index < 0 || index >= consumableCards.Count) return;

        // 交換
        var old = consumableCards[index];
        consumableCards[index] = PendingCard;

        // （使い切りカードを選択しても効果は発動しない仕様）
        Debug.Log($"使い切りカードを入替え: {old.cardName} -> {PendingCard.cardName}");

        EndSwapMode();
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 入れ替え実行：パッシブスロット index を指定して、PendingCard と入替える
    /// （古いパッシブは効果解除し、新しいパッシブを適用する）
    /// </summary>
    public void ReplacePassiveAt(int index)
    {
        if (!IsSwapMode) return;
        if (PendingCard == null || PendingCardType != CardType.Passive) return;
        if (index < 0 || index >= passiveCards.Count) return;

        var old = passiveCards[index];

        // 先に古い効果を解除（実装はゲームに合わせて）
        RemovePassiveEffect(old);

        passiveCards[index] = PendingCard;

        // 新しい効果を適用
        ApplyPassiveEffect(PendingCard);

        Debug.Log($"パッシブカードを入替え: {old.cardName} -> {PendingCard.cardName}");

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
