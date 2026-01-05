using UnityEngine;
using System.Collections;

/// <summary>
/// プレイヤーと敵のターン管理を行うバトルマネージャー。
/// 攻撃処理・ターン切り替え・敵の移動処理など、戦闘における
/// メインループの役割を持つ。
/// </summary>
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    // 現在がプレイヤーのターンかどうか
    [HideInInspector]
    public bool isPlayerTurn;

    // ★ 追加：カード選択待ち
    public bool isWaitingCardSelect = false;

    // ターン遅延の秒数
    public float turnDelay = 1f;

    private void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        isPlayerTurn = true;
    }

    /// <summary>
    /// プレイヤーの行動処理。
    /// </summary>
    public void PlayerTurn()
    {
        // プレイヤーのターンでない場合は処理しない
        if (!isPlayerTurn) return;

        Debug.Log("プレイヤーのターン ");

        // ダメージ処理

        // プレイヤーのターン終了
        isPlayerTurn = false;

        if( DungeonGenerator.CurrentFloor == 3)
        {
            StartCoroutine(BossTurn());
        }
        else
        {
            StartCoroutine(EnemyTurn());
        }
        
    }

    /// <summary>
    /// 敵の行動処理を行うコルーチン。
    /// 敵の移動完了まで待機し、移動後にプレイヤーターンへ戻る。
    /// </summary>

    [SerializeField] private EnemySpawnerController enemySpawnerController;


    public IEnumerator EnemyTurn()
    {
        isPlayerTurn = false;

        // ★ カード選択が終わるまで待つ
        while (isWaitingCardSelect)
            yield return null;

        yield return new WaitForSeconds(turnDelay);

        Debug.Log("敵のターン");

        var enemies = UnitManager.instance.enemies;
        int finishedCount = 0;

        // ==============================
        // ターン状態リセット
        // ==============================
        UnitManager.instance.ClearReservations();

        foreach (var e in enemies)
        {
            e.GetComponent<EnemyMovement>().ResetTurnState();
        }

        // ==============================
        // 距離マップ作成（1回だけ）
        // ==============================
        Vector2Int playerPos =
            Vector2Int.RoundToInt(PlayerMovement.instance.transform.position);

        DungeonGenerator dungeon = FindFirstObjectByType<DungeonGenerator>();
        var distMap = DistanceMap.Build(playerPos, dungeon);

        // ==============================
        // 敵ごとに行動決定
        // ==============================
        foreach (var e in enemies)
        {
            EnemyMovement mv = e.GetComponent<EnemyMovement>();
            EnemyAttack atk = e.GetComponent<EnemyAttack>();
            EnemyStatus status = e.GetComponent<EnemyStatus>();

            // 移動完了コールバック
            mv.onMoveFinished = () =>
            {
                finishedCount++;
            };

            // =========================
            // ★ スタン判定（最優先）
            // =========================
            if (status != null && status.ConsumeStun())
            {
                Debug.Log($"{e.name} はスタン中で行動不能");
                finishedCount++;   // ★ 何もしないがターン消費
                continue;
            }

            // ---------- 攻撃 ----------
            bool attacked = atk.TryAttackPlayer();

            if (attacked)
            {
                finishedCount++;
                continue;
            }

            // ---------- 移動 ----------
            Vector2Int dir = mv.DecideChaseByDistance(distMap);

            if (dir != Vector2Int.zero)
            {
                mv.StartMove(dir);
            }
            else
            {
                finishedCount++;
            }

        }

        // ==============================
        // 全敵の行動完了待ち
        // ==============================
        while (finishedCount < enemies.Count)
        {
            yield return null;
        }

        // ==============================
        // ターン終了
        // ==============================
        isPlayerTurn = true;
        Debug.Log("敵ターン終了 > プレイヤーのターンへ");
    }

    /// <summary>
    /// ボス専用のターン処理。
    /// ボスの行動を実行
    /// 演出待ち
    /// プレイヤーターンへ戻す
    /// </summary>
    public IEnumerator BossTurn()
    {
        isPlayerTurn = false;

        Debug.Log("ボスのターン開始");

        // 少し間を空けてから行動
        yield return new WaitForSeconds(turnDelay);

        // ボス取得
        BossController boss =
            FindFirstObjectByType<BossController>();

        if (boss != null)
        {
            // ボスの行動を実行
            boss.BossAction();
        }
        else
        {
            Debug.LogWarning("BossController が見つかりません");
        }

        // 攻撃演出・予兆表示を見せる時間
        yield return new WaitForSeconds(1.0f);

        // ハイライト消去
        HighlightManager.instance.Clear();

        // ターン終了 → プレイヤーへ
        isPlayerTurn = true;

        Debug.Log("ボスターン終了 > プレイヤーのターンへ");
    }
}
