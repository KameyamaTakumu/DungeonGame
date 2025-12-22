using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class EnemyAttack : MonoBehaviour
{
    public int attackRange = 1;  // 何マス先まで攻撃するか
    public int attackPower = 10; // 攻撃力
    public int hp = 10;

    private EnemyStatus enemyStatus;
    int atk;
    int range;

    private Vector2Int attackDir;

    void Awake()
    {
        // 同じオブジェクトの EnemyStatus を取得
        enemyStatus = GetComponent<EnemyStatus>();

        if (enemyStatus == null)
        {
            Debug.LogError("PlayerStatus コンポーネントが見つかりません！");
        }
        else
        {
            // ステータスを PlayerStatus から取得
            range = enemyStatus.status.RANGE;
            atk = enemyStatus.status.ATK;
        }

    }

    /// <summary>
    /// 攻撃可能なら true を返す（n マス先にプレイヤーがいる）
    /// </summary>
    public bool TryAttackPlayer()
    {
        // ★ すでに移動していたら攻撃しない
        EnemyMovement mv = GetComponent<EnemyMovement>();
        if (mv != null && mv.hasMoved)
            return false;

        Vector2Int origin = Vector2Int.RoundToInt(transform.position);

        Vector2 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
        Vector2Int playerGrid = Vector2Int.RoundToInt(playerPos);
        //Debug.Log($"[DEBUG] 敵の位置: {origin}, プレイヤーの位置: {playerGrid}, attackRange: {attackRange}");

        // 4方向（上下左右）を順番にチェックする
        Vector2Int[] dirs = {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

        foreach (var dir in dirs)
        {

            //Debug.Log($"[DEBUG] 方向チェック: {dir}");

            for (int i = 1; i <= range; i++)
            {
                Vector2Int check = origin + dir * i;
                //Debug.Log($"[DEBUG]   チェック座標: {check}");

                // マスにプレイヤーがいるか判定
                if (CombatManager.IsPlayerAt(check))
                {
                    attackDir = dir;
                    Debug.Log($"敵が {i} マス先のプレイヤーを攻撃！");
                    AttackForward(dir);

                    // ★ 攻撃したフラグを立てる
                    if (mv != null)
                        mv.hasAttacked = true;

                    return true;
                }
            }
        }

        return false; // 攻撃範囲にプレイヤーはいない
    }

    public void AttackForward(Vector2Int dir)
    {
        Debug.Log($"{name} が攻撃！");

        Vector2Int origin = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );

        //デバッグ用
        //Vector2Int checkPos = origin + dir * attackRange;
        Vector2Int checkPos = origin + dir * range;
        //Debug.Log($"[DEBUG] 攻撃origin={origin}, dir={dir}, checkPos={checkPos}");


        //GameObject target = CombatManager.GetObjectInLine(origin, dir, attackRange);
        // 指定方向に RANGE だけ飛ばす
        GameObject target = CombatManager.GetObjectInLine(
            origin,
            dir,
            range
        );

        if (target != null)
        {
            Debug.Log($"敵は {range} マス先の {target.name} を攻撃した！");

            // PlayerStatus を取得
            PlayerStatus player = target.GetComponent<PlayerStatus>();

            if (player != null)
            {
                player.TakeDamage(atk);
                Debug.Log($"{target.name} に {atk} のダメージを与えた！");
            }
            else
            {
                Debug.LogError("[ERROR] PlayerStatus が見つかりません！");
            }
        }
        else
        {
            Debug.Log("攻撃は外れた（対象なし）");
        }
    }
}
