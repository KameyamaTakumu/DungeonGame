using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSelectButton : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public Image iconImage;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text rangeText;
    [SerializeField] TMP_Text valueText;

    [SerializeField] Outline outline; // ★ 追加

    private CardData card;
    private System.Action onSelected;
    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        outline.enabled = false;
    }

    // セットアップ
    public void Setup(CardData card, System.Action onSelected)
    {
        this.card = card;
        this.onSelected = onSelected;

        if (iconImage != null && card.icon != null)
            iconImage.sprite = card.icon;

        // ===== 表示 =====
        nameText.text = card.cardName;

        if (card.useEffectType == UseEffectType.Heal)
        {
            rangeText.gameObject.SetActive(false);
            valueText.text = $"回復 : {card.healAmount}";
        }
        else
        {
            rangeText.gameObject.SetActive(true);
            rangeText.text = $"範囲 : {card.range}";
            valueText.text = $"威力 : {card.damage}";
        }

        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            onSelected?.Invoke();
            CardTooltipUI.Instance?.Hide(); // ★選択確定時は消す
        });
    }

    public void OnSelect(BaseEventData eventData)
    {
        outline.enabled = true;
        CardTooltipUI.Instance?.Show(card, rect);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        outline.enabled = false;
        CardTooltipUI.Instance?.Hide();
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
