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

    ScrollView scrollView;

    [HideInInspector]
    public bool isAttacking = false;
    protected override void Awake()
    {
        base.Awake();

        instance = this;
    }

    void Start()
    {
        UnitManager.instance.RegisterPlayer(this.gameObject);

        scrollView = FindFirstObjectByType<ScrollView>();
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
            if (tm != null && !tm.isPlayerTurn)
                return;
        }

        int x = 0, y = 0;

        // 移動入力の判定（WASD または 矢印キー）
        // 通常：1歩ずつ（GetKeyDown）
        // Ctrl中：押しっぱなしで連続移動（GetKey）
        if (debugMove)
        {
            // 連続移動したいので GetKey にする
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y = 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y = -1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x = -1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x = 1;
        }
        else
        {
            // 通常時は1マスずつ
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) y = 1;
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) y = -1;
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) x = -1;
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) x = 1;
        }

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
            if (tm != null)
            {
                tm.StartCoroutine(tm.EnemyTurn());
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
        if (!debugMove && tm != null)
        {
            tm.StartCoroutine(tm.EnemyTurn());
        }
    }
}
