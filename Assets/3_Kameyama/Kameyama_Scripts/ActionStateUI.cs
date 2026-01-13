using TMPro;
using UnityEngine;

public class ActionStateUI : MonoBehaviour
{
    public static ActionStateUI Instance;

    [SerializeField] TextMeshProUGUI stateText;

    void Awake()
    {
        Instance = this;
        stateText.text = "";
    }

    public void ShowMessage(string msg)
    {
        stateText.text = msg;
        stateText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        stateText.text = "";
        stateText.gameObject.SetActive(false);
    }
}
