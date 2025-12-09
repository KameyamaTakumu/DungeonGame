using UnityEngine;

public class StepsDownUI : MonoBehaviour
{
    public static StepsDownUI Instance;

    public GameObject panel;     // 確認UI本体
    private System.Action onYes; // 「はい」時の処理

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Open(System.Action yesAction)
    {
        onYes = yesAction;
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    // ▼ボタンから呼ぶ
    public void OnYes()
    {
        panel.SetActive(false);
        onYes?.Invoke();
    }

    public void OnNo()
    {
        panel.SetActive(false);
    }
}
