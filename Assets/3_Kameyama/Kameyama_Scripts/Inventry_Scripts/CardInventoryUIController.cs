using UnityEngine;

/// <summary>
/// CardInventory の UI 操作・スロット生成・キー操作・テスト取得 をすべて1本にまとめた統合版。
/// </summary>
public class CardInventoryUIController : MonoBehaviour
{
    [Header("UI 切替管理")]
    public GameObject consumableUI;
    public GameObject passiveUI;

    [Header("インベントリ参照")]
    public CardInventory inventory;

    [Header("スロット生成")]
    public GameObject slotPrefab;
    public Transform consumableParent;
    public Transform passiveParent;

    [Header("テスト用 Pickup（任意）")]
    public CardData testConsumableCard;
    public CardData testPassiveCard;

    private void Start()
    {
        // Inventory が無ければ自動取得
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<CardInventory>();
        }

        // イベント登録
        if (inventory != null)
        {
            inventory.OnSwapRequested += OnSwapRequested;
            inventory.OnInventoryChanged += Refresh;
        }

        Refresh(); // 初期スロット生成
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnSwapRequested -= OnSwapRequested;
            inventory.OnInventoryChanged -= Refresh;
        }
    }

    private void Update()
    {
        // ----- UI 切替 -----
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ShowConsumableUI();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ShowPassiveUI();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            HideAllUI();
            inventory?.CancelSwap();
        }

        // ----- テストカード取得 -----
        // E → 消費カード
        if (Input.GetKeyDown(KeyCode.E) && testConsumableCard != null)
        {
            inventory.AddCard(testConsumableCard);
        }

        // C → パッシブカード
        if (Input.GetKeyDown(KeyCode.C) && testPassiveCard != null)
        {
            inventory.AddCard(testPassiveCard);
        }
    }

    // ================================
    // UI 表示系
    // ================================
    void OnSwapRequested(CardData pending, CardType type)
    {
        Debug.Log($"UI: 入れ替え要求: {pending.cardName} ({type})");

        if (type == CardType.Consumable)
            ShowConsumableUI();
        else
            ShowPassiveUI();
    }

    public void ShowConsumableUI()
    {
        if (consumableUI) consumableUI.SetActive(true);
        if (passiveUI) passiveUI.SetActive(false);
    }

    public void ShowPassiveUI()
    {
        if (passiveUI) passiveUI.SetActive(true);
        if (consumableUI) consumableUI.SetActive(false);
    }

    public void HideAllUI()
    {
        if (consumableUI) consumableUI.SetActive(false);
        if (passiveUI) passiveUI.SetActive(false);
    }

    // ================================
    // スロット生成系
    // ================================
    void Refresh()
    {
        if (inventory == null) return;

        // 既存削除
        foreach (Transform t in consumableParent) Destroy(t.gameObject);
        foreach (Transform t in passiveParent) Destroy(t.gameObject);

        // 消費スロット生成
        for (int i = 0; i < inventory.consumableCards.Count; i++)
        {
            var card = inventory.consumableCards[i];
            var slotObj = Instantiate(slotPrefab, consumableParent);
            var slot = slotObj.GetComponent<CardSlotUI>();
            slot.Setup(card, i, true);
        }

        // パッシブスロット生成
        for (int i = 0; i < inventory.passiveCards.Count; i++)
        {
            var card = inventory.passiveCards[i];
            var slotObj = Instantiate(slotPrefab, passiveParent);
            var slot = slotObj.GetComponent<CardSlotUI>();
            slot.Setup(card, i, false);
        }
    }
}
