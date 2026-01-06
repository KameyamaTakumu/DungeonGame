using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public Image icon;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text rangeText;
    [SerializeField] TMP_Text valueText;

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

        nameText.text = card.cardName;

        // =========================
        // Use / Buff 分岐
        // =========================
        if (card.cardType == CardType.Use)
        {
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
        }
        else // Buff
        {
            SetBuffDisplay(card);
        }

        btn.interactable = true;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }

    public void Clear()
    {
        card = null;
        icon.enabled = false;
        nameText.text = "";
        valueText.text = "";
        rangeText.text = "";
        btn.interactable = false;
        btn.onClick.RemoveAllListeners();
    }

    void SetBuffDisplay(CardData card)
    {
        rangeText.gameObject.SetActive(true);

        switch (card.buffType)
        {
            case BuffType.Attack:
                rangeText.text = "攻撃力UP";
                valueText.text = $"+{card.buffValue}";
                break;

            case BuffType.HP:
                rangeText.text = "最大HPUP";
                valueText.text = $"+{card.buffValue}";
                break;

            case BuffType.Range:
                rangeText.text = "攻撃範囲UP";
                valueText.text = $"+{card.buffValue}";
                break;

            case BuffType.CritChance:
                rangeText.text = "CRT確率UP";
                valueText.text = $"+{card.buffValue}%";
                break;

            case BuffType.PassiveMultiplier:
                rangeText.text = "倍率UP";
                valueText.text = $"×{card.buffMultiplier}";
                break;

            case BuffType.UseAttackBoost:
                rangeText.text = "倍率UP";
                valueText.text = $"×{card.buffValue}";
                break;

            default:
                rangeText.text = "";
                valueText.text = "";
                break;
        }
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
