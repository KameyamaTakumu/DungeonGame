using TMPro;
using UnityEngine;

public class CardSwapNoticeUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject useSwapText;
    [SerializeField] private GameObject buffSwapText;

    private CardInventory inventory;

    void Start()
    {
        inventory = FindFirstObjectByType<CardInventory>();

        inventory.OnSwapRequested += OnSwapStart;
        inventory.OnSwapEnded += OnSwapEnd;

        root.SetActive(false);
    }

    void OnSwapStart(CardData card, CardType type)
    {
        root.SetActive(true);

        useSwapText.SetActive(type == CardType.Use);
        buffSwapText.SetActive(type == CardType.Buff);
    }

    void OnSwapEnd()
    {
        root.SetActive(false);
        useSwapText.SetActive(false);
        buffSwapText.SetActive(false);
    }

    void OnDestroy()
    {
        inventory.OnSwapRequested -= OnSwapStart;
        inventory.OnSwapEnded -= OnSwapEnd;
    }
}
