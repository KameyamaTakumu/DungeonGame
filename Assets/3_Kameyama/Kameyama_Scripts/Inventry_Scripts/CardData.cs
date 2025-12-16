using UnityEngine;

public enum CardType
{
    Consumable, // 使い切り
    Passive     // 常時発動(バフ)
}

public enum BuffType
{
    None,
    Attack,
    HP
}

public enum CardRangeType
{
    Around, // 周囲攻撃（今まで通り）
    Line    // 直線攻撃
}

[CreateAssetMenu(menuName = "Card/CardData")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;
    public Sprite icon;
    public CardType cardType;
    [TextArea]
    public string description;

    [Header("Consumable Effect")]
    public int damage;
    public int range = 1;
    public CardRangeType rangeType; // ★追加

    [Header("Passive Buff")]
    public BuffType buffType;
    public int buffValue;
}
