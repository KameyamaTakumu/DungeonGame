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

    [SerializeField] Outline outline; // Åö í«â¡

    private CardData card;
    private System.Action onSelected;
    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        outline.enabled = false;
    }

    // ÉZÉbÉgÉAÉbÉv
    public void Setup(CardData card, System.Action onSelected)
    {
        this.card = card;
        this.onSelected = onSelected;

        if (iconImage != null && card.icon != null)
            iconImage.sprite = card.icon;

        nameText.text = card.cardName;

        // =========================
        // Use / Buff ï™äÚ
        // =========================
        if (card.cardType == CardType.Use)
        {
            if (card.useEffectType == UseEffectType.Heal)
            {
                rangeText.gameObject.SetActive(false);
                valueText.text = $"âÒïú : {card.healAmount}";
            }
            else
            {
                rangeText.gameObject.SetActive(true);
                rangeText.text = $"îÕàÕ : {card.range}";
                valueText.text = $"à–óÕ : {card.damage}";
            }
        }
        else // Buff
        {
            SetBuffDisplay(card);
        }

        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            onSelected?.Invoke();
            CardTooltipUI.Instance?.Hide(); // ÅöëIëämíËéûÇÕè¡Ç∑
        });
    }

    void SetBuffDisplay(CardData card)
    {
        rangeText.gameObject.SetActive(true);

        switch (card.buffType)
        {
            case BuffType.Attack:
                rangeText.text = "çUåÇóÕUP";
                valueText.text = $"+{card.buffValue}";
                break;

            case BuffType.HP:
                rangeText.text = "ç≈ëÂHPUP";
                valueText.text = $"+{card.buffValue}";
                break;

            case BuffType.Range:
                rangeText.text = "çUåÇîÕàÕUP";
                valueText.text = $"+{card.buffValue}";
                break;

            case BuffType.CritChance:
                rangeText.text = "CRTämó¶UP";
                valueText.text = $"+{card.buffValue}%";
                break;

            case BuffType.PassiveMultiplier:
                rangeText.text = "î{ó¶UP";
                valueText.text = $"Å~{card.buffMultiplier}";
                break;

            case BuffType.UseAttackBoost:
                rangeText.text = "î{ó¶UP";
                valueText.text = $"Å~{card.buffValue}";
                break;

            default:
                rangeText.text = "";
                valueText.text = "";
                break;
        }
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
    // Tooltip ï\é¶
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
