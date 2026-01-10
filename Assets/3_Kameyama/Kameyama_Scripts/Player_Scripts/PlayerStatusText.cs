using TMPro;
using UnityEngine;

/// <summary>
/// 攻撃力とクリティカル率をまとめて表示する
/// </summary>
public class PlayerStatusText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI statusText;

    void Start()
    {
        UpdateText();
    }

    void Update()
    {
        // バフでリアルタイムに変わるので毎フレーム更新
        UpdateText();
    }

    void UpdateText()
    {
        if (PlayerStatus.instance == null) return;

        int atk = PlayerStatus.instance.Attack;
        float crit = PlayerStatus.instance.CritChance * 100f;

        statusText.text = $"攻撃力 : {atk} | クリティカル率 : {crit}%";
    }
}
