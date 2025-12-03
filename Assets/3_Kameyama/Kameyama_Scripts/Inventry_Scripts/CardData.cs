using UnityEngine;

public enum CardType
{
    Consumable, // g‚¢Ø‚è
    Passive     // í”­“®(ƒoƒt)
}

[CreateAssetMenu(menuName = "Card/CardData")]
public class CardData : ScriptableObject
{
    public string cardName;
    public Sprite icon;
    public CardType cardType;
    public string description;
}
