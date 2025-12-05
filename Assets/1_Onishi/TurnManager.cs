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

        var enemies = enemySpawnerController.GetSpawnedEnemies();
        int enemyCount = enemies.Count;

        for (int i = 0; i < enemyCount; i++)
        {
        if (EnemyAttack.instance.TryAttackPlayer())
        {
            //Debug.Log("敵は攻撃してターン終了");
            isPlayerTurn = true;
            yield break;   // 移動せずターン終了
        }

        // 攻撃できなければ移動
        //Debug.Log("攻撃できなかったため、移動へ");

        // ---- 移動方向の決定 ----
        //（上下・左右のいずれかのみを許可）
        int x = 0, y = 0;

        // 有効な方向が出るまでループ
        // 上下（x=0, y=±1）または左右（x=±1, y=0）
        do
        {
            // -1,0,1
            x = Random.Range(-1, 2); 
            y = Random.Range(-1, 2);
        }
        while (!((x == 0 && y != 0) || (x != 0 && y == 0)));

        // TryMove が false の場合は移動可能な方向が出るまで再抽選
        while (!EnemyMovement.instance.TryMove(x, y))
        {
            x = Random.Range(-1, 2);
            y = Random.Range(-1, 2);
        }

        // ---- 移動完了を待つ ----
        bool moved = false;

            // 移動完了時に呼ばれるコールバック
            EnemyMovement.instance.onMoveFinished = () =>
        {
            moved = true;
        };

            // 実際の移動実行
        EnemyMovement.instance.TryMove(x,y); // 左に1マス移動

        // 移動完了になるまで待機
        while (!moved)
            yield return null;

        }


        // ---- 敵ターン終了 ----
        isPlayerTurn = true;
        //Debug.Log("敵ターン終了 > 次のプレイヤー行動待ち");
    }
}
