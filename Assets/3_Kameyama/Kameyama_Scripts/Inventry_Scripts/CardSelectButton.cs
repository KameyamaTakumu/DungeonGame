using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSelectButton : MonoBehaviour
{
    public Image iconImage;      // ← カード画像を表示するUI
    public Text nameText; // 任意：カード名

    private CardData card;
    private System.Action onSelected;

    // セットアップ
    public void Setup(CardData card, System.Action onSelected)
    {
        this.card = card;
        this.onSelected = onSelected;

        // 画像と名前を反映
        if (iconImage != null && card.icon != null)
        {
            iconImage.sprite = card.icon;   // ← カードの画像をセット
        }

        if (nameText != null)
        {
            nameText.text = card.cardName;        // ← 名前もセット
        }

        // ボタンイベント登録
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() =>
        {
            onSelected?.Invoke();
        });
    }
}
