using UnityEngine;

public class CardPickupTester : MonoBehaviour
{
    public CardInventory cardInventory;

    public CardData testConsumableCard;
    public CardData testPassiveCard;

    private void Start()
    {
        if (cardInventory == null)
        {
            cardInventory = FindFirstObjectByType<CardInventory>();
        }
    }

    void Update()
    {
        // Eキー → 使い切りカード取得
        if (Input.GetKeyDown(KeyCode.E))
        {
            cardInventory.AddCard(testConsumableCard);
        }

        // Rキー → バフカード取得
        if (Input.GetKeyDown(KeyCode.C))
        {
            cardInventory.AddCard(testPassiveCard);
        }
    }
}
