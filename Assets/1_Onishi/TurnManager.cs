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
        // ターンフラグを敵側に設定
        isPlayerTurn = false;

        // 演出として少し待機
        yield return new WaitForSeconds(turnDelay); // n秒待つ

        Debug.Log("敵のターン");

        // ---- 攻撃できるならする ----

        var enemies = UnitManager.instance.enemies;

        int finishedCount = 0;

        foreach (var e in enemies)
        {
            e.GetComponent<EnemyAttack>().TryAttackPlayer();
        }

        yield return new WaitForSeconds(0.2f);

        // 攻撃できなければ移動
        //Debug.Log("攻撃できなかったため、移動へ");
        

        // ---- 移動方向の決定 ----
        

        // --- 移動する敵は移動開始（全員同時） ---
        foreach (var e in enemies)
        {
            var mv = e.GetComponent<EnemyMovement>();

            // ① コールバック登録（移動前）
            mv.onMoveFinished = () =>
            {
                finishedCount++;
            };

            // ② フラグ初期化
            mv.moveFinished = false;

            // ③ 移動開始
            Vector2Int dir = mv.DecideMoveDir();
            mv.StartMove(dir);

        }

        // ---- 移動完了を待つ ----
        while (finishedCount < enemies.Count)
        {
            yield return null;
        }

        // ---- 敵ターン終了 ----
        isPlayerTurn = true;
        Debug.Log("敵ターン終了 > 次のプレイヤー行動待ち");
    }
}
