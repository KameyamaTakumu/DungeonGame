using UnityEngine;

public class CardSelectUI : MonoBehaviour
{
    public GameObject optionPrefab;
    public Transform optionParent;

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

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);

        // ★ UIフェーズ終了
        PlayerInputLock.Instance?.Unlock();

        // ★ カード選択終了 → ターン再開
        if (TurnManager.Instance != null)
            TurnManager.Instance.isWaitingCardSelect = false;

        onClose?.Invoke();
    }

    public void Cancel()
    {
        gameObject.SetActive(false);

        PlayerInputLock.Instance?.Unlock();

        if (TurnManager.Instance != null)
            TurnManager.Instance.isWaitingCardSelect = false;

        onClose = null;
    }
}
