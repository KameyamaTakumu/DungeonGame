using TMPro;
using UnityEngine;

public class CardTooltipUI : MonoBehaviour
{
    public static CardTooltipUI Instance;

    [Header("UI")]
    public GameObject root;
    public TMP_Text nameText;
    public TMP_Text typeText;
    public TMP_Text effectText;

    [Header("Offset")]
    public Vector2 offset = new Vector2(120, 0);

    RectTransform rect;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);

        rect = GetComponent<RectTransform>();
        Hide();
    }

    public void Show(CardData card, RectTransform target)
    {
        if (card == null) return;

        root.SetActive(true);

        // ===== テキスト設定 =====
        nameText.text = card.cardName;

        typeText.text = card.cardType == CardType.Use
            ? "種類：使い切り"
            : "種類：パッシブ";

        effectText.text = BuildEffectText(card);

        // ===== 位置調整（吹き出し）=====
        rect.position = target.position + (Vector3)offset;
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    string BuildEffectText(CardData card)
    {
        if (card.cardType == CardType.Use)
        {
            if (card.useEffectType == UseEffectType.Heal)
            {
                return "HP回復";
            }
            if (card.useEffectType == UseEffectType.StunAttack)
            {
                return "スタン攻撃";
            }

            string rangeText =
                card.rangeType == CardRangeType.Around ? "周囲攻撃" : "直線攻撃";

            return $"{rangeText}";
        }
        else
        {
            if (card.buffType == BuffType.Attack)
                return $"攻撃力 +{card.buffValue}";
            if (card.buffType == BuffType.HP)
                return $"HP +{card.buffValue}";
            if (card.buffType == BuffType.CritChance)
                return $"クリティカル率 +{card.buffValue}%";
            if (card.buffType == BuffType.Range)
                return $"通常攻撃範囲 +{card.buffValue}";
            if (card.buffType == BuffType.PassiveMultiplier)
                return $"バフ効果倍率 x{card.buffMultiplier}";
            if (card.buffType == BuffType.UseAttackBoost)
                return $"消費カード攻撃力 +{card.buffValue}";

            return "";
        }
    }
}
