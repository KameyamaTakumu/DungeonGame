using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public Image icon;

    [SerializeField] Outline outline; // ★ 追加

    [HideInInspector] public int slotIndex;
    [HideInInspector] public bool isConsumable;

    [SerializeField] Button btn;

    CardData card;
    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        outline.enabled = false;
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

        btn.interactable = true;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }

    public void Clear()
    {
        card = null;
        icon.enabled = false;
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

        inv.OnConsumableCardClicked(slotIndex, false); // ← マウス
    }

    public void SetSelected(bool selected)
    {
        icon.color = selected ? Color.yellow : Color.white;
    }

    // =========================
    // ★ キーボード選択対応
    // =========================

    public void OnSelect(BaseEventData eventData)
    {
        outline.enabled = true;

        if (card != null)
            CardTooltipUI.Instance?.Show(card, rect);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        outline.enabled = false;
        CardTooltipUI.Instance?.Hide();
    }

    // =========================
    // マウス操作（既存）
    // =========================

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
