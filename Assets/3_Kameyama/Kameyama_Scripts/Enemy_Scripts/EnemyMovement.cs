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

    // 移動完了時のイベント
    public System.Action onMoveFinished;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        UnitManager.instance.RegisterEnemy(this.gameObject);
        gridPos = Vector2Int.RoundToInt(transform.position);
    }

    /// <summary>
    /// 移動完了時に呼ばれるフックメソッド。
    /// BaseMovement の OnMoveFinished をオーバーライドして
    /// 登録された onMoveFinished イベントを呼び出す。
    /// </summary>
    /// <param name="debugMove">デバッグ用フラグ（派生先で使用可）</param>
    protected override void OnMoveFinished(bool debugMove)
    {
        gridPos = Vector2Int.RoundToInt(transform.position);
        moveFinished = true;
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
        // 移動中なら StartMove を受け付けない
        if (isMoving)
        {
            Debug.LogWarning($"{name} は前の移動処理が終わっていないのに StartMove が呼ばれた");
            return;
        }

        moveFinished = false;     // ★ はじめに false にする

        bool moved = TryMove(dir.x, dir.y);

        if (!moved)
        {
            // ★ 移動できなかった場合でも "完了" 扱いにする
            moveFinished = true;
            onMoveFinished?.Invoke();
            return;
        }

        Debug.Log($"{name} が移動開始！");
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

}
