using UnityEngine;

/// <summary>
/// カメラが指定したターゲットを滑らかに追従する機能を提供するクラス
/// LateUpdate を使用することで、ターゲットの移動処理が完了した後に
/// カメラ位置を更新し、追従のズレを防ぐ
/// </summary>
public class CameraFollow : MonoBehaviour
{
    /// <summary>
    /// 追尾対象のターゲット Transform
    /// </summary>
    [CustomLabel("追尾するターゲット")]
    [SerializeField] private Transform target;

    /// <summary>
    /// ターゲットへ補間移動する際の追従速度（大きいほど素早く追尾）
    /// </summary>
    [CustomLabel("追尾の滑らかさ")]
    [SerializeField] private float smooth = 5f;

    [Header("マップ制限")]
    [SerializeField] private DungeonGenerator dungeon;

    /// <summary>
    /// 毎フレームの後処理としてカメラ位置を調整
    /// target が存在しない場合は処理を行わない
    /// </summary>
    private void LateUpdate()
    {
        if (target == null || dungeon == null) return;

        Vector3 pos = transform.position;

        // 追従（スムーズ）
        pos.x = Mathf.Lerp(pos.x, target.position.x, smooth * Time.deltaTime);
        pos.y = Mathf.Lerp(pos.y, target.position.y, smooth * Time.deltaTime);

        // ===== カメラサイズ取得 =====
        Camera cam = Camera.main;
        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = camHalfHeight * cam.aspect;

        // ===== マップ端を計算（タイル中心基準）=====
        float minX = camHalfWidth - 0.5f;
        float maxX = dungeon.width - camHalfWidth - 0.5f;

        float minY = camHalfHeight - 0.5f;
        float maxY = dungeon.height - camHalfHeight - 0.5f;

        // ===== Clamp =====
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }

    /// <summary>
    /// 外部からカメラ追従対象を設定するためのメソッド
    /// プレイヤー生成のタイミングに応じて動的にターゲットが
    /// 変わるケース（例：キャラ切り替え）に対応するための設計
    /// </summary>
    /// <param name="t">追従対象となる Transform</param>
    public void SetTarget(Transform t)
    {
        target = t;
    }
}
