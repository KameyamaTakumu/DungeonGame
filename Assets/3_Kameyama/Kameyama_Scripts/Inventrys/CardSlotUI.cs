using UnityEngine;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour
{
    public Image icon;
    public Text nameText;

    CardData card;
    int index;
    bool isConsumable;

    public void Setup(CardData cardData, int cardIndex, bool consumable)
    {
        card = cardData;
        index = cardIndex;
        isConsumable = consumable;

        icon.sprite = card.icon;
        nameText.text = card.cardName;

        GetComponent<Button>().onClick.AddListener(OnClickSlot);
    }

    void OnClickSlot()
    {
        if (isConsumable)
        {
            // インベントリを検索し使用
            FindFirstObjectByType<CardInventory>().UseConsumableCard(index);
        }
        else
        {
            Debug.Log($"常時発動カード「{card.cardName}」は使用できません（持っているだけで発動）");
        }
    }
}
