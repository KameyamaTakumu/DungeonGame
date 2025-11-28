using UnityEngine;
using System;

/// <summary>
/// 敵キャラクターの2D移動を管理するクラス。
/// BaseMovement を継承しており、グリッド単位での移動をサポート。
/// AI やスクリプトによる制御を想定。
/// </summary>
public class EnemyMovement : BaseMovement
{
    // シングルトンインスタンス
    public static EnemyMovement instance;

    [HideInInspector]
    public bool isAttacking = false;　// 攻撃中フラグ

    // 移動完了時のイベント
    public Action onMoveFinished;

    protected override void Awake()
    {
        base.Awake();

        instance = this;
    }

    /// <summary>
    /// 移動完了時に呼ばれるフックメソッド。
    /// BaseMovement の OnMoveFinished をオーバーライドして
    /// 登録された onMoveFinished イベントを呼び出す。
    /// </summary>
    /// <param name="debugMove">デバッグ用フラグ（派生先で使用可）</param>
    protected override void OnMoveFinished(bool debugMove)
    {
        onMoveFinished?.Invoke();
    }

    /// <summary>
    /// 指定座標にプレイヤーがいるか判定
    /// </summary>
    public bool PlayerInCell(Vector2Int cell)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector2 pos = player.transform.position;

        return Mathf.RoundToInt(pos.x) == cell.x &&
               Mathf.RoundToInt(pos.y) == cell.y;
    }
}
