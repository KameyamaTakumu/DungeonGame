using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 敵キャラクターの2D移動を管理するクラス。
/// BaseMovement を継承しており、グリッド単位での移動をサポート。
/// AI やスクリプトによる制御を想定。
/// </summary>
public class EnemyMovement : BaseMovement
{
    public bool moveFinished = false; // 移動完了フラグ

    public Vector2Int gridPos;

    [HideInInspector]
    public bool isAttacking = false;　// 攻撃中フラグ

    // ★ 追加：このターン攻撃したか
    [HideInInspector]
    public bool hasAttacked = false;

    // ★ 追加：このターン移動したか
    [HideInInspector]
    public bool hasMoved = false;

    // 移動完了時のイベント
    public System.Action onMoveFinished;

    Animator anim;

    protected override void Awake()
    {
        base.Awake();

        anim = GetComponent<Animator>();
    }

    void Start()
    {
        UnitManager.instance.RegisterEnemy(this.gameObject);
        gridPos = Vector2Int.RoundToInt(transform.position);
    }

    // ★ ターン開始時に呼ぶ
    public void ResetTurnState()
    {
        hasAttacked = false;
        hasMoved = false;
        moveFinished = false;
    }

    /// <summary>
    /// 移動完了時に呼ばれるフックメソッド。
    /// BaseMovement の OnMoveFinished をオーバーライドして
    /// 登録された onMoveFinished イベントを呼び出す。
    /// </summary>
    /// <param name="debugMove">デバッグ用フラグ（派生先で使用可）</param>
    protected override void OnMoveFinished(bool debugMove)
    {
        anim.SetBool("isMoving", false);   // ★ 歩行停止

        Vector2Int oldPos = gridPos;

        Vector2Int snapped = Vector2Int.RoundToInt(transform.position);
        transform.position = new Vector3(snapped.x, snapped.y, transform.position.z);

        gridPos = snapped;

        // ★ UnitManager に位置更新を通知
        UnitManager.instance.MoveEnemy(oldPos, gridPos, this);

        moveFinished = true;
        hasMoved = true;


        onMoveFinished?.Invoke();
    }

    // --- 敵の移動方向を決定 ---
    public Vector2Int DecideMoveDir()
    {
        int x = 0;
        int y = 0;

        do
        {
            x = Random.Range(-1, 2);
            y = Random.Range(-1, 2);
        }
        while (!((x == 0 && y != 0) || (x != 0 && y == 0)));

        return new Vector2Int(x, y);
    }

    public Vector2Int DecideChaseDir(Vector2Int playerPos)
    {
        // A* で経路取得
        List<Vector2Int> path = PathFinder.FindPath(gridPos, playerPos);

        // 経路が無い or 自分と同じマス
        if (path == null || path.Count < 2)
            return Vector2Int.zero;

        // 次に進むマス
        Vector2Int next = path[1];

        // 現在地との差分 = 移動方向
        return next - gridPos;
    }


    // --- 移動開始 ---
    public void StartMove(Vector2Int dir)
    {
        if (isMoving)
            return;

        Vector2Int targetPos = gridPos + dir;

        // ★ 予約チェック（隊列用）
        if (!UnitManager.instance.CanReserve(targetPos))
        {
            moveFinished = true;
            onMoveFinished?.Invoke();
            return;
        }

        UnitManager.instance.Reserve(targetPos);

        SetFacingDirection(dir);              // ★ 向きを先に合わせる
        anim.SetBool("isMoving", true);       // ★ 歩行開始

        bool moved = TryMove(dir.x, dir.y);

        if (!moved)
        {
            moveFinished = true;
            onMoveFinished?.Invoke();
            return;
        }
    }

    public Vector2Int DecideChaseByDistance(
    Dictionary<Vector2Int, int> distMap)
    {
        int best = int.MaxValue;
        Vector2Int bestDir = Vector2Int.zero;

        Vector2Int cur = gridPos;

        Vector2Int[] dirs =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

        foreach (var d in dirs)
        {
            Vector2Int next = cur + d;

            if (!distMap.ContainsKey(next))
                continue;

            if (distMap[next] < best)
            {
                best = distMap[next];
                bestDir = d;
            }
        }

        return bestDir;
    }

    /// <summary>
    /// 移動せずに敵の向きだけを変える
    /// </summary>
    private void SetFacingDirection(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return;

        anim.SetFloat("moveX", dir.x);
        anim.SetFloat("moveY", dir.y);
        anim.SetBool("isMoving", false);
    }

    public void FaceToTarget(Vector2Int targetPos)
    {
        Vector2Int dir = targetPos - gridPos;

        // 斜めを除去
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            dir = new Vector2Int((int)Mathf.Sign(dir.x), 0);
        else
            dir = new Vector2Int(0, (int)Mathf.Sign(dir.y));

        SetFacingDirection(dir);
    }
}
