using UnityEngine;

public class CardInventoryUIManager : MonoBehaviour
{
    public GameObject consumableUI;
    public GameObject passiveUI;

    public CardInventory inventory; // Inspector で割当推奨

    void Start()
    {
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<CardInventory>();
        }

        if (inventory != null)
        {
            inventory.OnSwapRequested += OnSwapRequested;
        }
    }

    void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnSwapRequested -= OnSwapRequested;
        }
    }

    void Update()
    {
        // 使い切りカードUI表示 → Qキー
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ShowConsumableUI();
        }

        // バフカードUI表示 → Zキー
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ShowPassiveUI();
        }

        // カードUI非表示 → Xキー (キャンセル)
        if (Input.GetKeyDown(KeyCode.X))
        {
            HideAllUI();
            // 入れ替え中だったらキャンセル
            inventory?.CancelSwap();
        }
    }

    void OnSwapRequested(CardData pending, CardType type)
    {
        // 上限に到達したときに呼ばれる
        Debug.Log($"UIManager: 入れ替え要求を受信: {pending.cardName} ({type})");

        if (type == CardType.Consumable)
        {
            ShowConsumableUI();
            // UI 上で何か表示して「このスロットと入れ替える？」等のメッセージを出すと親切
        }
        else
        {
            ShowPassiveUI();
        }

        // ここで必要ならフォーカスや説明テキストの切り替えを行ってください
    }

    public void ShowConsumableUI()
    {
        if (consumableUI != null) consumableUI.SetActive(true);
        if (passiveUI != null) passiveUI.SetActive(false);
    }

    public void ShowPassiveUI()
    {
        if (passiveUI != null) passiveUI.SetActive(true);
        if (consumableUI != null) consumableUI.SetActive(false);
    }

    public void HideAllUI()
    {
        if (consumableUI != null) consumableUI.SetActive(false);
        if (passiveUI != null) passiveUI.SetActive(false);
    }
}
