using UnityEngine;

/// <summary>
/// シンプルな2D移動。WASD/矢印キーで動く。
/// </summary>
public class PlayerMovement : BaseMovement
{
    public static PlayerMovement instance;

    public bool isAttacking = false;

    protected override void Awake()
    {
        base.Awake();

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (isMoving) return;

        // ★Ctrl でターン無視モード（デバッグ）
        bool debugMove =
            Input.GetKey(KeyCode.LeftControl) ||
            Input.GetKey(KeyCode.RightControl);

        // ★バトル中でプレイヤーターンじゃない → ただしCtrlなら無視して動く
        if (!debugMove)
        {
            if (bm != null && !bm.isPlayerTurn)
                return;
        }

        int x = 0, y = 0;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) y = 1;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) y = -1;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) x = -1;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) x = 1;

        if (x != 0 || y != 0)
        {
            TryMove(x, y, debugMove); // ★debugMove を渡す
        }

        // 攻撃処理（スペース）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isAttacking = true;
            if (bm != null)
            {
                bm.StartCoroutine(bm.EnemyTurn());
            }
        }
    }

    protected override void OnMoveFinished(bool debugMove)
    {
        // 移動完了 → 敵ターンへ
        if (!debugMove && bm != null)
        {
            bm.StartCoroutine(bm.EnemyTurn());
        }
    }
}
