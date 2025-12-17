using UnityEngine;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour
{
    public Image icon;
    public Text nameText;

    [HideInInspector] public int slotIndex;
    [HideInInspector] public bool isConsumable;

    [SerializeField] Button btn; // ★直接指定

    CardData card;

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

        if (btn == null)
        {
            Debug.LogError("Button が CardSlotUI に見つかりません", this);
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
            inv.OnConsumableCardClicked(slotIndex);
        }
        else
        {
            Debug.Log($"パッシブカード「{card.cardName}」は使用できません");
        }
    }
}
