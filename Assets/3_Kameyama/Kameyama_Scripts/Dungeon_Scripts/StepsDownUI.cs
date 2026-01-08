using UnityEngine;
using UnityEngine.UI;

public class StepsDownUI : MonoBehaviour
{
    //public static StepsDownUI Instance;

    //public GameObject panel;     // 確認UI本体
    //private System.Action onYes; // 「はい」時の処理

    //void Awake()
    //{
    //    Instance = this;
    //    panel.SetActive(false);
    //}

    //public void Open(System.Action yesAction)
    //{
    //    onYes = yesAction;
    //    panel.SetActive(true);
    //}

    //public void Close()
    //{
    //    panel.SetActive(false);
    //}

    //// ▼ボタンから呼ぶ
    //public void OnYes()
    //{
    //    DungeonGenerator.CurrentFloor += 1;
    //    panel.SetActive(false);
    //    onYes?.Invoke();
    //}

    //public void OnNo()
    //{
    //    panel.SetActive(false);
    //}
    public static StepsDownUI Instance;

    public GameObject panel;

    [Header("Buttons")]
    public Button yesButton;
    public Button noButton;

    private System.Action onYes;

    // true = Yes選択中 / false = No選択中
    private bool isYesSelected = true;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    void Update()
    {
        if (!panel.activeSelf) return;

        HandleKeyboard();
    }

    public void Open(System.Action yesAction)
    {
        onYes = yesAction;
        panel.SetActive(true);

        // ★ プレイヤー操作をロック
        PlayerInputLock.Instance.Lock();

        // 初期選択を Yes に
        isYesSelected = true;
        UpdateSelectionVisual();
    }

    public void Close()
    {
        panel.SetActive(false);

        // ★ ロック解除
        PlayerInputLock.Instance.Unlock();
    }

    private void HandleKeyboard()
    {
        // ← → / A D で切り替え
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            isYesSelected = true;
            UpdateSelectionVisual();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            isYesSelected = false;
            UpdateSelectionVisual();
        }

        // Enterで決定
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (isYesSelected)
                OnYes();
            else
                OnNo();
        }
    }

    private void UpdateSelectionVisual()
    {
        // EventSystemを使わず、色で簡易的に表現
        Color selected = new Color(1f, 1f, 1f, 1f);
        Color unselected = new Color(1f, 1f, 1f, 0.5f);

        yesButton.image.color = isYesSelected ? selected : unselected;
        noButton.image.color = isYesSelected ? unselected : selected;
    }

    // ▼ ボタン / Enter 両対応
    public void OnYes()
    {
        DungeonGenerator.CurrentFloor += 1;
        Close();
        onYes?.Invoke();
    }

    public void OnNo()
    {
        // ★ ロック解除
        Close();
    }
}
