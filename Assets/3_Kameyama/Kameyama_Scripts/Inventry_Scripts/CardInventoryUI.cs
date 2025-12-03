using UnityEngine;

public class CardInventoryUI : MonoBehaviour
{
    public CardInventory inventory;

    public GameObject slotPrefab;         // CardSlotPrefab
    public Transform consumableParent;    // Grid parent
    public Transform passiveParent;       // 今回は表示だけ

    void Start()
    {
        inventory.OnInventoryChanged += Refresh;
        Refresh();
    }

    void Refresh()
    {
        // 既存スロット削除
        foreach (Transform t in consumableParent) Destroy(t.gameObject);
        foreach (Transform t in passiveParent) Destroy(t.gameObject);

        // 使い切りカードの描画
        for (int i = 0; i < inventory.consumableCards.Count; i++)
        {
            var card = inventory.consumableCards[i];
            var slotObj = Instantiate(slotPrefab, consumableParent);
            slotObj.GetComponent<CardSlotUI>().Setup(card, i, true);
        }

        // バフカードの描画（今回はまだ使わない）
        for (int i = 0; i < inventory.passiveCards.Count; i++)
        {
            var card = inventory.passiveCards[i];
            var slotObj = Instantiate(slotPrefab, passiveParent);
            slotObj.GetComponent<CardSlotUI>().Setup(card, i, false);
        }
    }
}
