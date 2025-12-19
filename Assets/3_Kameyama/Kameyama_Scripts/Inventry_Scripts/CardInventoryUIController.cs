using UnityEngine;

/// <summary>
/// CardInventory の UI 制御（固定スロット方式）
/// ・スロットは事前配置
/// ・GridLayout / Instantiate 不使用
/// ・Index = スロット番号で完全一致
/// </summary>
public class CardInventoryUIController : MonoBehaviour
{
    [Header("UI 切替管理")]
    public GameObject consumableUI;
    public GameObject passiveUI;

    [Header("インベントリ参照")]
    public CardInventory inventory;

    [Header("カードスロットPrefab")]
    public CardSlotUI slotPrefab;

    [Header("固定スロット位置（Transform）")]
    public Transform[] consumableSlotPoints;
    public Transform[] passiveSlotPoints;

    [Header("カードデータベース")]
    public CardDataBase database;

    [Header("カード選択UI")]
    public CardSelectUI selectUI;

    // 生成済みスロット管理
    CardSlotUI[] consumableSlots;
    CardSlotUI[] passiveSlots;

    void Start()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<CardInventory>();

        if (inventory != null)
        {
            inventory.OnInventoryChanged += Refresh;
            inventory.OnSwapRequested += OnSwapRequested;
        }

        consumableSlots = new CardSlotUI[consumableSlotPoints.Length];
        passiveSlots = new CardSlotUI[passiveSlotPoints.Length];

        Refresh();
    }

    void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= Refresh;
            inventory.OnSwapRequested -= OnSwapRequested;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (consumableUI.activeSelf)
            {
                HideAllUI();
                inventory?.CancelSwap();
            }
            else ShowConsumableUI();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (passiveUI.activeSelf)
            {
                HideAllUI();
                inventory?.CancelSwap();
            }
            else ShowPassiveUI();
        }

        if (Input.GetKeyDown(KeyCode.E))
            ShowRandomSelect(CardType.Consumable);

        if (Input.GetKeyDown(KeyCode.C))
            ShowRandomSelect(CardType.Passive);
    }

    // ================================
    // UI 表示制御
    // ================================
    void OnSwapRequested(CardData pending, CardType type)
    {
        if (type == CardType.Consumable)
            ShowConsumableUI();
        else
            ShowPassiveUI();
    }

    public void ShowConsumableUI()
    {
        CardTooltipUI.Instance?.Hide();

        consumableUI?.SetActive(true);
        passiveUI?.SetActive(false);
    }

    public void ShowPassiveUI()
    {
        CardTooltipUI.Instance?.Hide();

        passiveUI?.SetActive(true);
        consumableUI?.SetActive(false);
    }

    public void HideAllUI()
    {
        consumableUI?.SetActive(false);
        passiveUI?.SetActive(false);

        CardTooltipUI.Instance?.Hide();
    }

    // ================================
    // スロット更新（核心）
    // ================================
    void Refresh()
    {
        if (inventory == null) return;

        // ---------- Consumable ----------
        for (int i = 0; i < consumableSlotPoints.Length; i++)
        {
            RefreshSlot(
                inventory.consumableCards,
                i,
                consumableSlotPoints[i],
                ref consumableSlots,
                true
            );
        }

        // ---------- Passive ----------
        for (int i = 0; i < passiveSlotPoints.Length; i++)
        {
            RefreshSlot(
                inventory.passiveCards,
                i,
                passiveSlotPoints[i],
                ref passiveSlots,
                false
            );
        }
    }

    void RefreshSlot(
    System.Collections.Generic.List<CardData> cardList,
    int index,
    Transform parent,
    ref CardSlotUI[] slots,
    bool isConsumable)
    {
        // 既存スロットがあれば削除
        if (slots[index] != null)
        {
            Destroy(slots[index].gameObject);
            slots[index] = null;
        }

        // カードが無ければ何も生成しない
        if (index >= cardList.Count)
            return;

        // Prefab生成
        var slot = Instantiate(slotPrefab, parent);

        // ★ここが重要：Setupは使わない
        slot.slotIndex = index;
        slot.isConsumable = isConsumable;
        slot.SetCard(cardList[index]);

        slots[index] = slot;
    }

    // ================================
    // カード選択UI
    // ================================
    public void ShowRandomSelect(CardType type)
    {
        if (database == null)
        {
            Debug.LogError("CardDatabase が設定されていません");
            return;
        }

        var list = database.GetCards(type);
        if (list == null || list.Length == 0)
        {
            Debug.LogWarning("カードが存在しません");
            return;
        }

        CardData[] options = new CardData[3];
        for (int i = 0; i < 3; i++)
            options[i] = list[Random.Range(0, list.Length)];

        selectUI.Open(inventory, options, Refresh);
    }
}
