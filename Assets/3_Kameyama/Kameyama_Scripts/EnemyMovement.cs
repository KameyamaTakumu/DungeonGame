using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// シンプルな2D移動。WASD/矢印キーで動く。
/// </summary>
public class EnemyMovement : MonoBehaviour
{
    public static EnemyMovement instance; // 唯一のインスタンス（常駐）

    [Header("1マスの大きさ")]
    public float cellSize = 1f;

    [Header("1マス移動の時間")]
    public float moveTime = 0.1f;

    [HideInInspector]
    public bool isMoving = false;
    [HideInInspector]
    public bool isAttacking = false;

    public Action onMoveFinished; // 移動完了イベント

    private DungeonGenerator dungeon;   // 自動取得
    private BattleManager bm;

    private void Awake()
    {
        // --- シングルトンパターンの確立 ---
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // 重複したインスタンスを削除
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); // シーンを跨いでも破棄しない

        // シーン内の DungeonGenerator を自動取得
        dungeon = FindFirstObjectByType<DungeonGenerator>();
        if (dungeon == null)
        {
            Debug.LogError("DungeonGenerator がシーンにありません！");
        }
        bm = FindFirstObjectByType<BattleManager>();
        if (bm == null)
        {
            Debug.LogError("BattleManager がシーンにありません！");
        }
    }

    public int TryMove(int mx, int my)
    {
        // 現在のグリッド座標
        Vector3 pos = transform.position;
        int cx = Mathf.RoundToInt(pos.x);
        int cy = Mathf.RoundToInt(pos.y);

        int nx = cx + mx;
        int ny = cy + my;

        // マップ範囲外は移動不可
        if (nx < 0 || ny < 0 || nx >= dungeon.width || ny >= dungeon.height)
            return 0;

        // 壁なら移動しない
        if (dungeon.map[nx, ny] == TileType.Wall)
            return 1;

        // 床だから移動OK
        StartCoroutine(MoveToCell(new Vector2(nx, ny)));

        return 0;
    }

    // コルーチンで1マス分だけ移動
    private IEnumerator MoveToCell(Vector2 target)
    {
        isMoving = true;

        Vector3 start = transform.position;
        Vector3 end = new Vector3(target.x, target.y, start.z);

        float t = 0;
        while (t < moveTime)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, t / moveTime);
            yield return null;
        }

        transform.position = end;
        isMoving = false;

        // 移動完了イベントを通知
        onMoveFinished?.Invoke();
    }
}
