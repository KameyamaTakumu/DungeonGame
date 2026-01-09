using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

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
    public CardSlotUI[] consumableSlots;
    public CardSlotUI[] passiveSlots;

    int currentIndex = 0;

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

        if (Input.GetKeyDown(KeyCode.E) && Input.GetKeyDown(KeyCode.LeftShift))
            ShowRandomSelect(CardType.Use);

        if (Input.GetKeyDown(KeyCode.C) && Input.GetKeyDown(KeyCode.LeftShift))
            ShowRandomSelect(CardType.Buff);

        if (!consumableUI.activeSelf && !passiveUI.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            MoveSelection(1);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            MoveSelection(-1);

        if (Input.GetKeyDown(KeyCode.Return))
            PressSelected();
    }

    void LateUpdate()
    {
        if (consumableUI.activeSelf &&
            EventSystem.current.currentSelectedGameObject == null)
        {
            SelectFirstConsumable();
        }

        if (passiveUI.activeSelf &&
            EventSystem.current.currentSelectedGameObject == null)
        {
            SelectFirstPassive();
        }
    }

    void MoveSelection(int dir)
    {
        var slots = consumableUI.activeSelf ? consumableSlots : passiveSlots;
        if (slots == null || slots.Length == 0) return;

        int next = currentIndex + dir;

        // ★ 端ループ
        if (next < 0)
            next = slots.Length - 1;
        else if (next >= slots.Length)
            next = 0;

        // ★ 空スロットをスキップ
        int safety = 0;
        while (slots[next] == null)
        {
            next += dir > 0 ? 1 : -1;

            if (next < 0)
                next = slots.Length - 1;
            else if (next >= slots.Length)
                next = 0;

            // 無限ループ防止
            if (++safety > slots.Length)
                return;
        }

        currentIndex = next;
        EventSystem.current.SetSelectedGameObject(slots[currentIndex].gameObject);
    }

    void PressSelected()
    {
        var go = EventSystem.current.currentSelectedGameObject;
        if (go == null) return;

        var slot = go.GetComponent<CardSlotUI>();
        if (slot != null && slot.isConsumable)
        {
            inventory.OnConsumableCardClicked(slot.slotIndex, true); // ← キーボード
            return;
        }

        // ★ 消費カードのみキーボード操作を許可
        if (slot.isConsumable)
        {
            inventory.OnConsumableCardClicked(slot.slotIndex, true);
            return;
        }

        // ★ バフカードは Enter で「何もしない」
        Debug.Log("バフカードはEnterで使用できません");

        //// パッシブなどは通常クリック扱い
        //var btn = go.GetComponent<UnityEngine.UI.Button>();
        //btn?.onClick.Invoke();
    }

    // ================================
    // UI 表示制御
    // ================================
    void OnSwapRequested(CardData pending, CardType type)
    {
        if (type == CardType.Use)
            ShowConsumableUI();
        else
            ShowPassiveUI();
    }

    public void ShowConsumableUI()
    {
        CardTooltipUI.Instance?.Hide();

        consumableUI?.SetActive(true);
        passiveUI?.SetActive(false);

        // ★ ロック
        PlayerInputLock.Instance?.Lock();

        StartCoroutine(SelectConsumableNextFrame());
    }

    IEnumerator SelectConsumableNextFrame()
    {
        yield return null;      // SetActive反映
        yield return null;      // EventSystem安定待ち

        SelectFirstConsumable();
    }

    void SelectFirstConsumable()
    {
        currentIndex = 0;

        for (int i = 0; i < consumableSlots.Length; i++)
        {
            if (consumableSlots[i] != null)
            {
                currentIndex = i;
                EventSystem.current.SetSelectedGameObject(consumableSlots[i].gameObject);
                return;
            }
        }

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ShowPassiveUI()
    {
        CardTooltipUI.Instance?.Hide();

        passiveUI?.SetActive(true);
        consumableUI?.SetActive(false);

        // ★ ロック
        PlayerInputLock.Instance?.Lock();

        StartCoroutine(SelectPassiveNextFrame());
    }

    IEnumerator SelectPassiveNextFrame()
    {
        yield return null;      // SetActive反映
        yield return null;      // EventSystem安定待ち

        SelectFirstPassive();
    }

    void SelectFirstPassive()
    {
        currentIndex = 0;

        for (int i = 0; i < passiveSlots.Length; i++)
        {
            if (passiveSlots[i] != null)
            {
                currentIndex = i;
                EventSystem.current.SetSelectedGameObject(passiveSlots[i].gameObject);
                return;
            }
        }

        // カードが1枚も無い場合
        EventSystem.current.SetSelectedGameObject(null);
    }


    public void HideAllUI()
    {
        // ★ 重要：先に選択を明示的に解除
        EventSystem.current.SetSelectedGameObject(null);

        // ★ 追加：カード選択解除（範囲も消える）
        inventory?.ClearConsumableSelection();

        consumableUI?.SetActive(false);
        passiveUI?.SetActive(false);

        CardTooltipUI.Instance?.Hide();

        // ★ ロック解除
        PlayerInputLock.Instance?.Unlock();
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

    void RefreshSlot(    System.Collections.Generic.List<CardData> cardList,int index,Transform parent,ref CardSlotUI[] slots,bool isConsumable)
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

        // ★ ここでUIフェーズに入る
        PlayerInputLock.Instance?.Lock();

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
