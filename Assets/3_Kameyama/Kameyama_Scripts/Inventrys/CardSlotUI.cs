using UnityEngine;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour
{
    //public Image icon;
    //public Text nameText;

    //CardData card;
    //int index;
    //bool isConsumable;

    //public void Setup(CardData cardData, int cardIndex, bool consumable)
    //{
    //    card = cardData;
    //    index = cardIndex;
    //    isConsumable = consumable;

    //    icon.sprite = card.icon;
    //    nameText.text = card.cardName;

    //    GetComponent<Button>().onClick.AddListener(OnClickSlot);
    //}

    //void OnClickSlot()
    //{
    //    if (isConsumable)
    //    {
    //        // インベントリを検索し使用
    //        FindFirstObjectByType<CardInventory>().UseConsumableCard(index);
    //    }
    //    else
    //    {
    //        Debug.Log($"常時発動カード「{card.cardName}」は使用できません（持っているだけで発動）");
    //    }
    //}
    public Image icon;
    public Text nameText;

    CardData card;
    int index;
    bool isConsumable;

    Button btn;

    public void Setup(CardData cardData, int cardIndex, bool consumable)
    {
        card = cardData;
        index = cardIndex;
        isConsumable = consumable;

        if (icon != null) icon.sprite = card.icon;
        if (nameText != null) nameText.text = card.cardName;

        btn = GetComponent<Button>();
        if (btn == null) return;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClickSlot);

        // 視覚的に swap 対応したい場合はここで色を変えるなど（UI刷新時に Update 呼ぶ）
        UpdateVisual();
    }

    void UpdateVisual()
    {
        // ここでは簡易的に何もしない — 必要なら色変化を入れてください
    }

    void OnClickSlot()
    {
        var inv = FindFirstObjectByType<CardInventory>();
        if (inv == null)
        {
            Debug.LogWarning("CardInventory が見つかりません");
            return;
        }

        // 入れ替えモードなら入れ替えを実行する（消費 or パッシブに応じて）
        if (inv.IsSwapMode)
        {
            // ただし PendingCardType と一致する UI でのみ置換可能にする
            if (inv.PendingCardType == CardType.Consumable && isConsumable)
            {
                inv.ReplaceConsumableAt(index);
            }
            else if (inv.PendingCardType == CardType.Passive && !isConsumable)
            {
                inv.ReplacePassiveAt(index);
            }
            else
            {
                // 間違ったUIを選んだ場合は無視またはメッセージ
                Debug.Log("入れ替え対象と違う種類のカードスロットが選ばれました。正しいUIを選んでください。");
            }
            return;
        }

        // 通常モード：消費カードなら使用、パッシブなら説明表示（今回は使用不可）
        if (isConsumable)
        {
            inv.UseConsumableCard(index);
        }
        else
        {
            Debug.Log($"常時発動カード「{card.cardName}」は使用できません（所持しているだけで効果がある）");
        }
    }
}
