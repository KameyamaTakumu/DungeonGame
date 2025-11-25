using UnityEngine;

/// <summary>
/// プレイヤーキャラクターの2D移動および入力管理クラス。
/// BaseMovement を継承しており、グリッド単位での移動をサポート。
/// WASD / 矢印キーでの移動とスペースキーによる攻撃入力を管理する。
/// </summary>
public class PlayerMovement : BaseMovement
{
    // シングルトンインスタンス
    public static PlayerMovement instance;

    [HideInInspector]
    public bool isAttacking = false;
    protected override void Awake()
    {
        base.Awake();

        instance = this;
    }

    private void Update()
    {
        // 移動中は入力を受け付けない
        if (isMoving) return;

        // 左右どちらかの Ctrl が押されている場合はターン無視モード（デバッグ用）
        bool debugMove = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        // バトル中でプレイヤーターンでない場合は移動不可（Ctrl で無視可能）
        if (!debugMove)
        {
            if (bm != null && !bm.isPlayerTurn)
                return;
        }

        int x = 0, y = 0;

        // 移動入力の判定（WASD または 矢印キー）
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))    y =  1;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))  y = -1;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))  x = -1;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) x =  1;

        // 入力があれば移動処理を実行
        if (x != 0 || y != 0)
        {
            TryMove(x, y, debugMove);
        }

        // 攻撃処理（一旦、スペースキー)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isAttacking = true;

            // 攻撃後に敵ターンへ移行
            if (bm != null)
            {
                bm.StartCoroutine(bm.EnemyTurn());
            }
        }
    }

    /// <summary>
    /// 移動完了時のフックメソッド。
    /// debugMove でない場合、移動後に敵ターンへ移行。
    /// </summary>
    /// <param name="debugMove">デバッグモードかどうか</param>
    protected override void OnMoveFinished(bool debugMove)
    {
        // 移動完了 → 敵ターン開始
        if (!debugMove && bm != null)
        {
            bm.StartCoroutine(bm.EnemyTurn());
        }
    }
}
