using UnityEngine;

/// <summary>
/// ボスの占有範囲（3×3）を赤い線で可視化するクラス。
/// アニメーション付きボスでも当たり判定が分かりやすくなる。
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class BossRangeVisualizer : MonoBehaviour
{
    [Header("表示設定")]
    [SerializeField] Color lineColor = Color.red;
    [SerializeField] float lineWidth = 0.05f;
    [SerializeField] float halfSize = 1.5f; // 3×3マス

    LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();

        line.positionCount = 5; // 四角＋始点に戻る
        line.loop = false;
        line.useWorldSpace = true;

        line.startColor = lineColor;
        line.endColor = lineColor;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        UpdateLine();
    }

    void LateUpdate()
    {
        // アニメーションや揺れ対応
        UpdateLine();
    }

    /// <summary>
    /// 外枠の位置を更新
    /// </summary>
    void UpdateLine()
    {
        Vector3 center = transform.position;

        Vector3 leftTop = center + new Vector3(-halfSize, halfSize, 0);
        Vector3 rightTop = center + new Vector3(halfSize, halfSize, 0);
        Vector3 rightBottom = center + new Vector3(halfSize, -halfSize, 0);
        Vector3 leftBottom = center + new Vector3(-halfSize, -halfSize, 0);

        line.SetPosition(0, leftTop);
        line.SetPosition(1, rightTop);
        line.SetPosition(2, rightBottom);
        line.SetPosition(3, leftBottom);
        line.SetPosition(4, leftTop); // 閉じる
    }
}
