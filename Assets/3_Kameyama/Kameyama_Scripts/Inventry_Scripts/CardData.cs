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
    public int damage;        // 敵に与えるダメージ
    public int range = 1;     // 攻撃距離（マス）

    [Header("Passive Buff")]
    public BuffType buffType;
    public int buffValue;     // ATK +◯ / HP +◯
}
