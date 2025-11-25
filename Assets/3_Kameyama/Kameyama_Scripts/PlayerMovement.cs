using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// シンプルな2D移動。WASD/矢印キーで動く。
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance; // 唯一のインスタンス（常駐）

    [Header("1マスの大きさ")]
    public float cellSize = 1f;

    [Header("1マス移動の時間")]
    public float moveTime = 0.1f;

    [HideInInspector]
    public bool isMoving = false;
    [HideInInspector]
    public bool isAttacking = false;

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
        if(bm == null)
        {
            Debug.LogError("BattleManager がシーンにありません！");
        }
    }

    private void Update()
    {
        if (isMoving) return; // 移動中は受付しない
        BattleManager bm = FindFirstObjectByType<BattleManager>();
        if(bm != null && !bm.isPlayerTurn) return; // 戦闘中でプレイヤーターンでないなら受付しない

        int x = 0, y = 0;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) y = 1;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) y = -1;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) x = -1;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) x = 1;

        // 入力なし
        if (x == 0 && y == 0) return;

        // 移動先チェック
        TryMove(x, y);

        // --デバッグ用　攻撃--
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Attack!");
            isAttacking = true;
            
            if (bm != null)
            {
                bm.StartCoroutine(bm.EnemyTurn());
            }
        }
    }

    public void TryMove(int mx, int my)
    {
        // 現在のグリッド座標
        Vector3 pos = transform.position;
        int cx = Mathf.RoundToInt(pos.x);
        int cy = Mathf.RoundToInt(pos.y);

        int nx = cx + mx;
        int ny = cy + my;

        // マップ範囲外は移動不可
        if (nx < 0 || ny < 0 || nx >= dungeon.width || ny >= dungeon.height)
            return;

        // 壁なら移動しない
        if (dungeon.map[nx, ny] == TileType.Wall)
            return;

        // 床だから移動OK
        StartCoroutine(MoveToCell(new Vector2(nx, ny)));
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

        if (bm != null)
        {
            bm.StartCoroutine(bm.EnemyTurn());
        }
    }
}
