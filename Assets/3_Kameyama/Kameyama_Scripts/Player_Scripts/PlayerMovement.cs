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
    bool isSelectingAttackDir    = false; // 攻撃方向選択中かどうか
    //bool isAttackmode            = false; // 攻撃モード中かどうか
    private Vector2Int attackDir = Vector2Int.zero; // 選択中の攻撃方向

    Animator anim;

    public PlayerAttack pa;
    PlayerStatus playerStatus;

    protected override void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        base.Awake();
    }

    void Start()
    {
        UnitManager.instance.RegisterPlayer(this.gameObject);

        pa = GetComponent<PlayerAttack>();

        anim = GetComponent<Animator>();
        playerStatus = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        // ボス撃破後移動ロック
        if (TurnManager.Instance.isInputLocked)
            return;

        // 移動中は入力を受け付けない
        if (isMoving) return;

        // 左右どちらかの Ctrl が押されている場合はターン無視モード（デバッグ用）
        bool debugMove = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        // ★ UI操作中は移動不可（DebugModeは例外）
        if (!debugMove && PlayerInputLock.Instance != null && PlayerInputLock.Instance.IsLocked)
            return;

        // バトル中でプレイヤーターンでない場合は移動不可（Ctrl で無視可能）
        if (!debugMove && tm != null && !tm.isPlayerTurn)
            return;

        HandleMovementInput(debugMove);
        HandleAttackInput();
    }

    void OnDestroy()
    {
        if (UnitManager.instance != null)
        {
            UnitManager.instance.UnregisterPlayer(gameObject);
        }
    }


    /// <summary>
    /// 移動入力処理
    /// </summary>
    private void HandleMovementInput(bool debugMove)
    {
        if (isSelectingAttackDir) return;

        int x = 0, y = 0;

        // 移動入力の判定（WASD または 矢印キー）
        // 通常：1歩ずつ（GetKeyDown）
        // Ctrl中：押しっぱなしで連続移動（GetKey）
        if (debugMove)
        {
            if (Input.GetKey(KeyCode.W)) y = 1;
            if (Input.GetKey(KeyCode.S)) y = -1;
            if (Input.GetKey(KeyCode.A)) x = -1;
            if (Input.GetKey(KeyCode.D)) x = 1;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.W)) y = 1;
            if (Input.GetKeyDown(KeyCode.S)) y = -1;
            if (Input.GetKeyDown(KeyCode.A)) x = -1;
            if (Input.GetKeyDown(KeyCode.D)) x = 1;
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
        // スぺースキーを押すと移動ができないようにする
        if (Input.GetKeyDown(KeyCode.Space) && !isSelectingAttackDir)
        {
            isSelectingAttackDir = true;
            attackDir = Vector2Int.zero;

            HighlightManager.instance.Clear();
            Debug.Log("攻撃方向を選択してください（IJKL）");
            Debug.Log("移動するにはもう一度Spaceを押してください");
            return;
        }

        // 移動ロック解除
        if (Input.GetKeyDown(KeyCode.Space) && isSelectingAttackDir)
        {
            isSelectingAttackDir = false;
            attackDir = Vector2Int.zero;

            HighlightManager.instance.Clear();
            Debug.Log("移動モードに戻りました");
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

            SetFacingDirection(attackDir);   // ★ 向きを変更

            pa.ShowHighlight(attackDir);
        }
        // 同じ方向 → 攻撃確定
        else if (attackDir == inputDir)
        {
            SetFacingDirection(attackDir);   // ★ 最終確認

            pa.AttackForward(attackDir);
            
            isSelectingAttackDir = false;
            attackDir            = Vector2Int.zero;
            //isAttackmode         = false;

            tm.PlayerTurn();
        }
        // 別方向 → 方向変更
        else if (attackDir != inputDir)
        {
            attackDir = inputDir;

            SetFacingDirection(attackDir);   // ★ 向きを変更

            pa.ShowHighlight(attackDir);
        }
        // もう一度
        else
        {

        }
    }

    public override bool TryMove(int mx, int my, bool debugMove = false)
    {
        // 移動中は不可
        if (isMoving) return false;

        Vector2Int moveDir = new Vector2Int(mx, my);

        // ★ 向きの確定（移動開始時）
        if (moveDir != Vector2Int.zero && playerStatus != null)
        {
            playerStatus.facingDir = moveDir;
        }

        // 向きだけ先に記録
        anim.SetFloat("moveX", mx);
        anim.SetFloat("moveY", my);
        anim.SetBool("isMoving", true);

        Vector3 pos = transform.position;
        Vector2Int cur = Vector2Int.RoundToInt(pos);
        Vector2Int next = cur + new Vector2Int(mx, my);

        // ★ 敵がいるなら移動不可（入力を無視）
        if (UnitManager.instance.IsEnemyAt(next))
        {
            // Debug.Log("敵がいて移動できない");
            return false;
        }
        // デバッグモード中は通過可能にする
        else if (!debugMove)
        {

        }

        // 通常の移動処理
        return base.TryMove(mx, my, debugMove);
    }

    /// <summary>
    /// 移動せずにプレイヤーの向きだけを変える
    /// </summary>
    private void SetFacingDirection(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return;

        anim.SetFloat("moveX", dir.x);
        anim.SetFloat("moveY", dir.y);
        anim.SetBool("isMoving", false); // 念のため

        // ★ ロジック用（超重要）
        PlayerStatus status = GetComponent<PlayerStatus>();
        if (status != null)
        {
            status.facingDir = dir;
        }
    }

    /// <summary>
    /// 移動完了時のフックメソッド。
    /// debugMove でない場合、移動後に敵ターンへ移行。
    /// </summary>
    /// <param name="debugMove">デバッグモードかどうか</param>
    protected override void OnMoveFinished(bool debugMove)
    {
        anim.SetBool("isMoving", false);

        // 移動完了 → 敵ターン開始
        if (!debugMove && tm != null)
        {
            tm.PlayerTurn();
        }
    }
}
