using UnityEngine;

public enum CardType
{
    Use, // 使い切り
    Buff     // 常時発動(バフ)
}

public enum UseEffectType
{
    Attack,
    Heal,
    StunAttack   // スタン
}

public enum BuffType
{
    None,
    Attack,
    HP,
    Range,              // 攻撃範囲プラス
    CritChance,         // クリ率
    PassiveMultiplier,  // 常時バフ倍率
    UseAttackBoost      // 消費枠攻撃力UP
}

public enum CardRangeType
{
    Around, // 周囲攻撃
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

    [Header("Use Effect")]
    public UseEffectType useEffectType;
    public int damage;
    public int healAmount;
    public int range = 1;
    public CardRangeType rangeType;

    [Header("Buff Buff")]
    public BuffType buffType;
    public int buffValue;

    [Header("Use Extra Effect")]
    public int stunTurn; // スタンターン数

    [Header("Buff Extra")]
    public float buffMultiplier = 1f; // 倍率
}
