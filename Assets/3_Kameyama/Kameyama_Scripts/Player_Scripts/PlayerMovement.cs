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

    ScrollView scrollView;

    [HideInInspector]
    public bool isAttacking = false;
    [HideInInspector]
    bool isSelectingAttackDir = false;
    private Vector2Int attackDir = Vector2Int.zero;
    public PlayerAttack pa;
    public HighlightManager hm;
    protected override void Awake()
    {
        base.Awake();

        instance = this;
    }

    void Start()
    {
        UnitManager.instance.RegisterPlayer(this.gameObject);

        scrollView = FindFirstObjectByType<ScrollView>();

        pa = GetComponent<PlayerAttack>();
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

        // 攻撃方向選択モード
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isSelectingAttackDir)
            {
                isSelectingAttackDir = true;
                attackDir = Vector2Int.zero;
                pa.ClearHighlight();

                Debug.Log("攻撃方向を選んでください");
                return;
            }
        }

        // 方向キーで攻撃方向決定
        if (isSelectingAttackDir)
        {
            Vector2Int dir = Vector2Int.zero;

            if (Input.GetKeyDown(KeyCode.I)) dir = Vector2Int.up;
            if (Input.GetKeyDown(KeyCode.K)) dir = Vector2Int.down;
            if (Input.GetKeyDown(KeyCode.J)) dir = Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.L)) dir = Vector2Int.right;

            if (dir != Vector2Int.zero)
            {
                attackDir = dir;
                pa.ShowHighlight(attackDir);

                Debug.Log("攻撃方向 → " + attackDir);
                
                    pa.AttackForward(attackDir);
                    isSelectingAttackDir = false;
                    tm.StartCoroutine(tm.EnemyTurn());
                
                return;
            }
        }
    }

    private void UpdateAttackHighlight()
    {
        if (attackDir == Vector2Int.zero) return;
        if (HighlightManager.instance == null)
        {
            Debug.LogError("HighlightManager.instance が null です！");
            return;
        }

        // プレイヤー位置を grid へ
        Vector2Int origin = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );

        // nマス分を計算
        List<Vector2Int> tiles = new List<Vector2Int>();
        for (int i = 1; i <= pa.attackRange; i++)
        {
            tiles.Add(origin + attackDir * i);
        }

        // ハイライト更新
        HighlightManager.instance.ShowTiles(tiles);
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
