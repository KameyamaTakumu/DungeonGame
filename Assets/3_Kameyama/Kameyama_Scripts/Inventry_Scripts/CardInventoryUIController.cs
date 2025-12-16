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

    [Header("固定スロット（Inspectorで設定）")]
    public CardSlotUI[] consumableSlots;
    public CardSlotUI[] passiveSlots;

    [Header("カードデータベース")]
    public CardDataBase database;

    [Header("カード選択UI")]
    public CardSelectUI selectUI;

    void Start()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<CardInventory>();

        if (inventory != null)
        {
            inventory.OnInventoryChanged += Refresh;
            inventory.OnSwapRequested += OnSwapRequested;
        }

        // スロット初期設定
        for (int i = 0; i < consumableSlots.Length; i++)
        {
            consumableSlots[i].slotIndex = i;
            consumableSlots[i].isConsumable = true;
            consumableSlots[i].Clear();
        }

        for (int i = 0; i < passiveSlots.Length; i++)
        {
            passiveSlots[i].slotIndex = i;
            passiveSlots[i].isConsumable = false;
            passiveSlots[i].Clear();
        }

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
        // Q : Consumable UI
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (consumableUI.activeSelf)
            {
                HideAllUI();
                inventory?.CancelSwap();
            }
            else
            {
                ShowConsumableUI();
            }
        }

        // Z : Passive UI
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (passiveUI.activeSelf)
            {
                HideAllUI();
                inventory?.CancelSwap();
            }
            else
            {
                ShowPassiveUI();
            }
        }

        // E : Consumable取得
        if (Input.GetKeyDown(KeyCode.E))
            ShowRandomSelect(CardType.Consumable);

        // C : Passive取得
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
    // スロット更新
    // ================================
    void Refresh()
    {
        if (inventory == null) return;

        // Consumable
        for (int i = 0; i < consumableSlots.Length; i++)
        {
            if (i < inventory.consumableCards.Count)
                consumableSlots[i].SetCard(inventory.consumableCards[i]);
            else
                consumableSlots[i].Clear();
        }

        // Passive
        for (int i = 0; i < passiveSlots.Length; i++)
        {
            if (i < inventory.passiveCards.Count)
                passiveSlots[i].SetCard(inventory.passiveCards[i]);
            else
                passiveSlots[i].Clear();
        }
    }

    // ================================
    // カード選択UI
    // ================================
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
            Debug.LogWarning("カードが存在しません");
            return;
        }

        CardData[] options = new CardData[3];
        for (int i = 0; i < 3; i++)
            options[i] = list[Random.Range(0, list.Length)];

        selectUI.Open(inventory, options, Refresh);
    }
}
