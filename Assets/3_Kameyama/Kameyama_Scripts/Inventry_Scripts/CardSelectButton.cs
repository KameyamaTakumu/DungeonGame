using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSelectButton : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    public Image iconImage;
    public Text nameText;

    private CardData card;
    private System.Action onSelected;
    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    // セットアップ
    public void Setup(CardData card, System.Action onSelected)
    {
        this.card = card;
        this.onSelected = onSelected;

        if (iconImage != null && card.icon != null)
            iconImage.sprite = card.icon;

        if (nameText != null)
            nameText.text = card.cardName;

        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            onSelected?.Invoke();
            CardTooltipUI.Instance?.Hide(); // ★選択確定時は消す
        });
    }

    // =========================
    // Tooltip 表示
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

    void OnDisable()
    {
        CardTooltipUI.Instance?.Hide();
    }
}
