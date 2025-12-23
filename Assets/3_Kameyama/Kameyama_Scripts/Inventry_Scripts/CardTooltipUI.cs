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
        Instance = this;
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
                return $"HP回復\n回復量：{card.healAmount}";
            }
            if (card.useEffectType == UseEffectType.StunAttack)
            {
                return
                    $"硬直攻撃\n" +
                    $"ダメージ：{card.damage}\n" +
                    $"硬直：{card.stunTurn}ターン\n" +
                    $"範囲：{card.range}";
            }

            string rangeText =
                card.rangeType == CardRangeType.Around ? "周囲攻撃" : "直線攻撃";

            return
                $"{rangeText}\n" +
                $"ダメージ：{card.damage}\n" +
                $"範囲：{card.range}";
        }
        else
        {
            if (card.buffType == BuffType.Attack)
                return $"攻撃力 +{card.buffValue}";
            if (card.buffType == BuffType.HP)
                return $"HP +{card.buffValue}";

            return "効果なし";
        }
    }
}
