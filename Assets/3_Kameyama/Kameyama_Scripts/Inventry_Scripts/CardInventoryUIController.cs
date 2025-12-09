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

    [Header("カードデータベース")]
    public CardDataBase database;
    [Header("カード選択UI")]
    public CardSelectUI selectUI;

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

        // E → 使い切りカードを拾う（ランダム3枚から選択）
        if (Input.GetKeyDown(KeyCode.E))
        {
            ShowRandomSelect(CardType.Consumable);
        }

        // C → パッシブカードを拾う（ランダム3枚から選択）
        if (Input.GetKeyDown(KeyCode.C))
        {
            ShowRandomSelect(CardType.Passive);
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

    /// <summary>
    /// ランダムで3枚選出してカード選択UIを開く
    /// </summary>
    /// <param name="type">カードタイプ</param>
    void ShowRandomSelect(CardType type)
    {
        if (database == null)
        {
            Debug.LogError("CardDatabase が設定されていません");
            return;
        }

        var list = database.GetCards(type);
        if (list == null || list.Length == 0)
        {
            Debug.LogWarning("カードが設定されていません");
            return;
        }

        // --- ランダムで3枚選出 ---
        CardData[] options = new CardData[3];
        for (int i = 0; i < 3; i++)
        {
            options[i] = list[Random.Range(0, list.Length)];
        }

        // --- カード選択UIを開く ---
        selectUI.Open(inventory, options, () =>
        {
            // 選択後のUIリフレッシュ
            Refresh();
        });
    }
}
