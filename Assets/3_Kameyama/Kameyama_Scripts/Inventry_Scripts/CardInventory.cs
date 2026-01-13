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
    public static CardInventory Instance { get; private set; }

    public int consumableLimit = 2;
    public int passiveLimit = 2;

    public List<CardData> consumableCards = new List<CardData>();
    public List<CardData> passiveCards = new List<CardData>();

    public Action OnInventoryChanged;

    // swap mode
    public bool IsSwapMode { get; private set; } = false;
    public CardData PendingCard { get; private set; } = null;
    public CardType PendingCardType { get; private set; }

    // 現在選択中の使い切りカード
    public int SelectedConsumableIndex { get; private set; } = -1;

    // UIに「入れ替え開始」を通知するためのイベント
    public Action<CardData, CardType> OnSwapRequested;

    public CardInventoryUIController cardInventoryUIController;

    public CardSlotUI currentSelectedSlot;

    // 追加
    public Action OnSwapEnded;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// カード追加（上限チェック→追加 or 入替モード）
    /// </summary>
    public void AddCard(CardData card)
    {
        List<CardData> list = GetList(card.cardType);
        int limit = GetLimit(card.cardType);

        if (list.Count < limit)
        {
            SoundManager.Instance.PlaySE(SE.CardGet);

            list.Add(card);
            if (card.cardType == CardType.Buff)
                ApplyPassiveEffect(card);

            OnInventoryChanged?.Invoke();
        }
        else
        {
            // ★ プレイヤー操作ロック
            PlayerInputLock.Instance?.Lock();

            StartSwapMode(card, card.cardType);
        }
    }

    List<CardData> GetList(CardType type)
    {
        return type == CardType.Use ? consumableCards : passiveCards;
    }

    int GetLimit(CardType type)
    {
        return type == CardType.Use ? consumableLimit : passiveLimit;
    }

    // 入れ替えモード開始
    void StartSwapMode(CardData card, CardType type)
    {
        IsSwapMode = true;
        PendingCard = card;
        PendingCardType = type;
        Debug.Log($"入れ替えモード開始: {card.cardName} ({type})");

        // ★ プレイヤー操作ロック
        PlayerInputLock.Instance?.Lock();

        // UIに通知
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
        if (PendingCardType == CardType.Buff)
            RemovePassiveEffect(old);

        // 差し替え
        list[index] = PendingCard;

        // パッシブなら新しい効果適用
        if (PendingCardType == CardType.Buff)
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

        // ★ プレイヤー操作アンロック
        //PlayerInputLock.Instance?.Unlock();

        // ★ 入れ替え終了通知
        OnSwapEnded?.Invoke();

        // UIを閉じる
        cardInventoryUIController?.HideAllUI();
        Debug.Log("入れ替えモード終了");
    }

    void ApplyPassiveEffect(CardData passiveCard)
    {
        // TODO: 実際のステータス反映をここに書く
        FindFirstObjectByType<PlayerStatus>()?.ApplyBuff(passiveCard);
    }

    void RemovePassiveEffect(CardData passiveCard)
    {
        // TODO: 古いパッシブの効果解除処理をここに書く
        FindFirstObjectByType<PlayerStatus>()?.RemoveBuff(passiveCard);
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

        SoundManager.Instance.PlaySE(SE.CardUse);

        // 技発動（プレイヤーに通知）
        FindFirstObjectByType<PlayerSkillExecutor>()?.ExecuteCardSkill(card);

        // 削除
        consumableCards.RemoveAt(index);

        OnInventoryChanged?.Invoke();
    }

    public void OnConsumableCardClicked(int index)
    {
        // ★ UIが消費カード表示中でなければ拒否
        if (cardInventoryUIController != null &&
            !cardInventoryUIController.consumableUI.activeSelf)
            return;

        if (index < 0 || index >= consumableCards.Count) return;
        if (IsSwapMode) return;

        var card = consumableCards[index];

        // 回復は即使用
        if (card.useEffectType == UseEffectType.Heal)
        {
            ConsumeConsumableCard(index);
            return;
        }

        // 未選択 → 選択
        if (SelectedConsumableIndex != index)
        {
            SelectedConsumableIndex = index;

            // ★ ここでロック
            PlayerInputLock.Instance?.Lock();

            HighlightManager.instance.Clear();

            var executor = FindFirstObjectByType<PlayerSkillExecutor>();
            if (executor != null)
            {
                var tiles = executor.GetCardRangeTiles(card);
                HighlightManager.instance.ShowTiles(tiles);
            }

            UpdateSlotSelectionUI();
            return;
        }


        // 同じカードを再クリック → 使用
        ConsumeConsumableCard(index);
    }

    void UpdateSlotSelectionUI()
    {
        var ui = cardInventoryUIController;
        if (ui == null) return;

        for (int i = 0; i < ui.consumableSlots.Length; i++)
        {
            var slot = ui.consumableSlots[i];
            if (slot == null) continue;

            slot.SetSelected(i == SelectedConsumableIndex);
        }
    }

    void ConsumeConsumableCard(int index)
    {
        var card = consumableCards[index];

        FindFirstObjectByType<PlayerSkillExecutor>()
            ?.ExecuteCardSkill(card);

        consumableCards.RemoveAt(index);

        SelectedConsumableIndex = -1;
        HighlightManager.instance.Clear();

        // ★ UIフェーズ終了
        //PlayerInputLock.Instance?.Unlock();

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

    public void ClearConsumableSelection()
    {
        // ★ 入れ替え中は絶対に解除しない
        if (IsSwapMode)
            return;

        SelectedConsumableIndex = -1;
        HighlightManager.instance?.Clear();
        UpdateSlotSelectionUI();

        //Debug.Log("カード選択状態を解除");
    }

    public void OnConsumableCardClicked(int index, bool fromKeyboard)
    {
        if (index < 0 || index >= consumableCards.Count) return;
        if (IsSwapMode) return;

        var card = consumableCards[index];

        // 回復は即使用（入力元問わず）
        if (card.useEffectType == UseEffectType.Heal)
        {
            ConsumeConsumableCard(index);
            return;
        }

        // ★ 未選択 → 選択（マウス / キーボード共通）
        if (SelectedConsumableIndex != index)
        {
            SelectedConsumableIndex = index;

            PlayerInputLock.Instance?.Lock();

            HighlightManager.instance.Clear();

            var executor = FindFirstObjectByType<PlayerSkillExecutor>();
            if (executor != null)
            {
                var tiles = executor.GetCardRangeTiles(card);
                HighlightManager.instance.ShowTiles(tiles);
            }

            UpdateSlotSelectionUI();
            return;
        }

        // ★ 同じカードだが「キーボード1回目」は消費しない
        if (fromKeyboard)
            return;

        // ★ マウス2クリック目 or キーボード2回目
        ConsumeConsumableCard(index);
    }

    public void ReapplyAllPassiveEffects()
    {
        var player = FindFirstObjectByType<PlayerStatus>();
        if (player == null) return;

        foreach (var card in passiveCards)
        {
            player.ApplyBuff(card);
        }

        //Debug.Log("全バフを再適用しました");
    }

    public void ResetInventory()
    {
        consumableCards.Clear();
        passiveCards.Clear();

        SelectedConsumableIndex = -1;
        IsSwapMode = false;
        PendingCard = null;

        OnInventoryChanged?.Invoke();

        Debug.Log("カードインベントリをリセット");
    }
}
