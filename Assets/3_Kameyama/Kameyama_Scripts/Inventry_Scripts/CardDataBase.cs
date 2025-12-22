using UnityEngine;

[CreateAssetMenu(menuName = "Card/CardDatabase")]
public class CardDataBase : ScriptableObject
{
    // カードデータベース
    public CardData[] consumableCards;
    public CardData[] passiveCards;

    /// <summary>
    /// 指定タイプのカードリストを返す
    /// </summary>
    public CardData[] GetCards(CardType type)
    {
        return type == CardType.Use ? consumableCards : passiveCards;
    }
}
