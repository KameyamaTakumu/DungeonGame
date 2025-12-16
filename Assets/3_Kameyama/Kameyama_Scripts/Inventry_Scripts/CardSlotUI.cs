using UnityEngine;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour
{
    //public Image icon;
    //public Text nameText;

    //CardData card;
    //int index;
    //bool isConsumable;

    //Button btn;

    //public void Setup(CardData cardData, int cardIndex, bool consumable)
    //{
    //    card = cardData;
    //    index = cardIndex;
    //    isConsumable = consumable;

    //    if (icon != null) icon.sprite = card.icon;
    //    if (nameText != null) nameText.text = card.cardName;

    //    btn = GetComponent<Button>();
    //    if (btn == null) return;

    //    btn.onClick.RemoveAllListeners();
    //    btn.onClick.AddListener(OnClickSlot);

    //    // 視覚的に swap 対応したい場合はここで色を変えるなど（UI刷新時に Update 呼ぶ）
    //    UpdateVisual();
    //}

    //void UpdateVisual()
    //{
    //    // ここでは簡易的に何もしない — 必要なら色変化を入れてください
    //}

    //void OnClickSlot()
    //{
    //    var inv = FindFirstObjectByType<CardInventory>();
    //    if (inv == null)
    //    {
    //        Debug.LogWarning("CardInventory が見つかりません");
    //        return;
    //    }

    //    // 入れ替えモードなら入れ替えを実行する（消費 or パッシブに応じて）
    //    if (inv.IsSwapMode)
    //    {
    //        inv.ReplaceCardAt(index);
    //        return;
    //    }

    //    // 通常モード：消費カードなら使用、パッシブなら説明表示（今回は使用不可）
    //    if (isConsumable)
    //    {
    //        inv.UseConsumableCard(index);
    //    }
    //    else
    //    {
    //        Debug.Log($"常時発動カード「{card.cardName}」は使用できません（所持しているだけで効果がある）");
    //    }
    //}

    public Image icon;
    public Text nameText;

    [HideInInspector] public int slotIndex;
    [HideInInspector] public bool isConsumable;

    CardData card;
    Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
    }

    /// <summary>
    /// カードを表示する
    /// </summary>
    public void SetCard(CardData cardData)
    {
        card = cardData;

        if (card == null)
        {
            Clear();
            return;
        }

        icon.enabled = true;
        icon.sprite = card.icon;
        nameText.text = card.cardName;

        btn.interactable = true;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// 空スロット表示
    /// </summary>
    public void Clear()
    {
        card = null;

        icon.enabled = false;
        nameText.text = "";

        btn.interactable = false;
        btn.onClick.RemoveAllListeners();
    }

    void OnClick()
    {
        var inv = FindFirstObjectByType<CardInventory>();
        if (inv == null) return;

        if (inv.IsSwapMode)
        {
            inv.ReplaceCardAt(slotIndex);
            return;
        }

        if (isConsumable)
        {
            inv.UseConsumableCard(slotIndex);
        }
        else
        {
            Debug.Log($"パッシブカード「{card.cardName}」は使用できません");
        }
    }
}
