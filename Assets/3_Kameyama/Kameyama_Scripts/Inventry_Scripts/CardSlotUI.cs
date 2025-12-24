using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    public Image icon;
    public Text nameText;

    [HideInInspector] public int slotIndex;
    [HideInInspector] public bool isConsumable;

    [SerializeField] Button btn;

    CardData card;
    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

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

        // ★ 判断はすべて Inventory に任せる
        inv.OnConsumableCardClicked(slotIndex);
    }

    public void SetSelected(bool selected)
    {
        icon.color = selected ? Color.yellow : Color.white;
    }

    // ===== ★ここが追加 =====

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (card == null) return;
        CardTooltipUI.Instance?.Show(card, rect);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CardTooltipUI.Instance?.Hide();
    }
}
