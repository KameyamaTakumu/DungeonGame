using UnityEngine;
using System.Collections;

/// <summary>
/// プレイヤーと敵のターン管理を行うバトルマネージャー。
/// 攻撃処理・ターン切り替え・敵の移動処理など、戦闘における
/// メインループの役割を持つ。
/// </summary>
public class TrunManager : MonoBehaviour
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
    /// 現在のターンを逆転させる
    /// </summary>
    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn; // ターンを逆にする

        if (isPlayerTurn)
        {
            Debug.Log("プレイヤーのターンへ");
            PlayerTurn();
        }
        else
        {
            Debug.Log("敵のターンへ");
            StartCoroutine(EnemyTurn());
        }
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

        StartCoroutine(EnemyTurn());
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
}
