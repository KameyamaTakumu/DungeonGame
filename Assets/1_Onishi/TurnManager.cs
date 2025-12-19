using UnityEngine;
using System.Collections;

/// <summary>
/// プレイヤーと敵のターン管理を行うバトルマネージャー。
/// 攻撃処理・ターン切り替え・敵の移動処理など、戦闘における
/// メインループの役割を持つ。
/// </summary>
public class TurnManager : MonoBehaviour
{
    // 現在がプレイヤーのターンかどうか
    [HideInInspector]
    public bool isPlayerTurn;

    // ターン遅延の秒数
    public float turnDelay = 1f;

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
        //Debug.Log("プレイヤーターン終了 > 敵ターンへ");
        isPlayerTurn = false;

        Debug.Log("CurrentFloor: " + DungeonGenerator.CurrentFloor);

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

        yield return new WaitForSeconds(turnDelay);

        Debug.Log("敵のターン");

        var enemies = UnitManager.instance.enemies;
        int finishedCount = 0;

        // ==============================
        // ① 攻撃フェーズ
        // ==============================
        foreach (var e in enemies)
        {
            e.GetComponent<EnemyAttack>().TryAttackPlayer();
        }

        yield return new WaitForSeconds(0.2f);

        // ==============================
        // ② 距離マップを「1回だけ」作る
        // ==============================
        Vector2Int playerPos =
            Vector2Int.RoundToInt(PlayerMovement.instance.transform.position);

        DungeonGenerator dungeon = FindFirstObjectByType<DungeonGenerator>();

        var distMap = DistanceMap.Build(playerPos, dungeon);

        // ==============================
        // ③ 移動フェーズ
        // ==============================
        foreach (var e in enemies)
        {
            var mv = e.GetComponent<EnemyMovement>();

            // コールバック登録
            mv.onMoveFinished = () =>
            {
                finishedCount++;
            };

            mv.moveFinished = false;

            // 距離マップから追尾方向を取得
            Vector2Int dir = mv.DecideChaseByDistance(distMap);

            if (dir != Vector2Int.zero)
            {
                mv.StartMove(dir);
            }
            else
            {
                // 動けない場合も完了扱い
                mv.moveFinished = true;
                mv.onMoveFinished?.Invoke();
            }
        }

        // ==============================
        // ④ 全敵の移動完了待ち
        // ==============================
        while (finishedCount < enemies.Count)
        {
            yield return null;
        }

        // ==============================
        // ⑤ ターン終了
        // ==============================
        isPlayerTurn = true;
        Debug.Log("敵ターン終了 > 次のプレイヤー行動待ち");
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
