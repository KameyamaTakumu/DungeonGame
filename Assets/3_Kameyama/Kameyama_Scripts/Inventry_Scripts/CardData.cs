using UnityEngine;

public enum CardType
{
    Use, // g‚¢Ø‚è
    Buff     // í”­“®(ƒoƒt)
}

public enum UseEffectType
{
    Attack,
    Heal,
    StunAttack   // š’Ç‰Áid’¼‹Êj
}

public enum BuffType
{
    None,
    Attack,
    HP,
    Range,              // šUŒ‚”ÍˆÍ{
    CritChance,         // šƒNƒŠ—¦
    PassiveMultiplier,  // šíƒoƒt”{—¦
    UseAttackBoost      // šÁ”ï˜gUŒ‚—ÍUP
}

public enum CardRangeType
{
    Around, // üˆÍUŒ‚i¡‚Ü‚Å’Ê‚èj
    Line    // ’¼üUŒ‚
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
    public UseEffectType useEffectType; // š’Ç‰Á
    public int damage;
    public int healAmount;              // š’Ç‰Á
    public int range = 1;
    public CardRangeType rangeType; // š’Ç‰Á

    [Header("Buff Buff")]
    public BuffType buffType;
    public int buffValue;

    [Header("Use Extra Effect")]
    public int stunTurn; // šƒXƒ^ƒ“ƒ^[ƒ“”id’¼‹Ê—pj

    [Header("Buff Extra")]
    public float buffMultiplier = 1f; // š”{—¦Œn
}
