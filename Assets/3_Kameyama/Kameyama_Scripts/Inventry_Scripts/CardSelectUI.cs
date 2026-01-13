using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class CardSelectUI : MonoBehaviour
{
    public GameObject optionPrefab;
    public Transform optionParent;

    public GameObject cancelPrefab;   // ★追加

    private CardInventory inventory;

    private System.Action onClose;

    // カード選択UIを開く
    public void Open(CardInventory inv, CardData[] options, System.Action onClose)
    {
        inventory = inv;
        this.onClose = onClose;

        // ★ プレイヤー操作ロック
        PlayerInputLock.Instance?.Lock();

        foreach (Transform t in optionParent) Destroy(t.gameObject);

        for (int i = 0; i < options.Length; i++)
        {
            int index = i; // ← これが超重要！（i を固定する）

            var obj = Instantiate(optionPrefab, optionParent);
            var btn = obj.GetComponent<CardSelectButton>();
            btn.Setup(options[index], () =>
            {
                inventory.AddCard(options[index]);
                Close();
            });
        }

        // ★ キャンセルボタンを最後に追加
        var cancelObj = Instantiate(cancelPrefab, optionParent);
        var cancelButton = cancelObj.GetComponent<Button>();
        cancelButton.onClick.AddListener(() =>
        {
            Cancel();
        });

        gameObject.SetActive(true);

        // ★ Navigation & 選択をまとめて遅延処理
        StartCoroutine(SetupNavigationAndSelect());
    }

    IEnumerator SetupNavigationAndSelect()
    {
        // UI有効化完了を待つ
        yield return null;

        // ★ Navigation をここで設定
        SetupHorizontalLoopNavigation();

        // ★ 選択初期化
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionParent.GetChild(0).gameObject);
    }

    public void Close()
    {
        // ★ 選択解除
        EventSystem.current.SetSelectedGameObject(null);

        gameObject.SetActive(false);

        // ★ 入れ替えモードに入らなかった場合だけ Unlock
        if (!inventory.IsSwapMode)
        {
            PlayerInputLock.Instance?.Unlock();
        }

        // ★ カード選択終了 → ターン再開
        if (TurnManager.Instance != null)
            TurnManager.Instance.isWaitingCardSelect = false;

        onClose?.Invoke();
    }

    public void Cancel()
    {
        gameObject.SetActive(false);

        // ★ 入れ替えモードに入らなかった場合だけ Unlock
        if (!inventory.IsSwapMode)
        {
            PlayerInputLock.Instance?.Unlock();
        }

        if (TurnManager.Instance != null)
            TurnManager.Instance.isWaitingCardSelect = false;

        onClose = null;
    }

    void SetupHorizontalLoopNavigation()
    {
        int count = optionParent.childCount;
        if (count <= 1) return;

        for (int i = 0; i < count; i++)
        {
            var current = optionParent.GetChild(i).GetComponent<Selectable>();

            var nav = new Navigation
            {
                mode = Navigation.Mode.Explicit
            };

            // 左：先頭なら末尾へ
            int leftIndex = (i == 0) ? count - 1 : i - 1;
            // 右：末尾なら先頭へ
            int rightIndex = (i == count - 1) ? 0 : i + 1;

            nav.selectOnLeft = optionParent.GetChild(leftIndex).GetComponent<Selectable>();
            nav.selectOnRight = optionParent.GetChild(rightIndex).GetComponent<Selectable>();

            nav.selectOnUp = null;
            nav.selectOnDown = null;

            current.navigation = nav;
        }
    }
}
