using System.Collections.Generic;
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
    bool isSelectingAttackDir = false;
    private Vector2Int attackDir = Vector2Int.zero;

    public PlayerAttack pa;

    protected override void Awake()
    {
        base.Awake();

        instance = this;
    }

    void Start()
    {
        UnitManager.instance.RegisterPlayer(this.gameObject);

        pa = GetComponent<PlayerAttack>();
    }

    private void Update()
    {
        // 移動中は入力を受け付けない
        if (isMoving) return;

        // 左右どちらかの Ctrl が押されている場合はターン無視モード（デバッグ用）
        bool debugMove = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        // バトル中でプレイヤーターンでない場合は移動不可（Ctrl で無視可能）
        if (!debugMove && tm != null && !tm.isPlayerTurn)
            return;

        HandleMovementInput(debugMove);
        HandleAttackInput();
    }

    /// <summary>
    /// 移動入力処理
    /// </summary>
    private void HandleMovementInput(bool debugMove)
    {
        int x = 0, y = 0;

        // 移動入力の判定（WASD または 矢印キー）
        // 通常：1歩ずつ（GetKeyDown）
        // Ctrl中：押しっぱなしで連続移動（GetKey）
        if (debugMove)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y = 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y = -1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x = -1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x = 1;
        }
        else
        {
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
    }

    /// <summary>
    /// 攻撃入力処理
    /// </summary>
    private void HandleAttackInput()
    {
        // 攻撃モード開始
        if (Input.GetKeyDown(KeyCode.Space) && !isSelectingAttackDir)
        {
            isSelectingAttackDir = true;
            attackDir = Vector2Int.zero;

            HighlightManager.instance.Clear();
            Debug.Log("攻撃方向を選択してください（IJKL）");
            return;
        }

        if (!isSelectingAttackDir) return;

        Vector2Int inputDir = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.I)) inputDir = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.K)) inputDir = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.J)) inputDir = Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.L)) inputDir = Vector2Int.right;

        if (inputDir == Vector2Int.zero) return;

        // 初回入力 → 仮決定＆ハイライト
        if (attackDir == Vector2Int.zero)
        {
            attackDir = inputDir;
            pa.ShowHighlight(attackDir);
        }
        // 同じ方向 → 攻撃確定
        else if (attackDir == inputDir)
        {
            pa.AttackForward(attackDir);

            isSelectingAttackDir = false;
            attackDir = Vector2Int.zero;

            tm.StartCoroutine(tm.EnemyTurn());
        }
        // 別方向 → 方向変更
        else
        {
            attackDir = inputDir;
            pa.ShowHighlight(attackDir);
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
