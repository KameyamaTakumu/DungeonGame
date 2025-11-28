using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class EnemyAttack : MonoBehaviour
{
    public int attackRange = 1;  // 何マス先まで攻撃するか
    public int attackPower = 10; // 攻撃力
    public int hp = 10;

    public static EnemyAttack instance;

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 攻撃可能なら true を返す（n マス先にプレイヤーがいる）
    /// </summary>
    public bool TryAttackPlayer()
    {
        // 敵のグリッド位置
        Vector2Int origin = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );

        // 4方向（上下左右）を順番にチェックする
        Vector2Int[] dirs = {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

        foreach (var dir in dirs)
        {
            for (int i = 1; i <= attackRange; i++)
            {
                Vector2Int check = origin + dir * i;

                // マスにプレイヤーがいるか判定
                if (EnemyMovement.instance.PlayerInCell(check))
                {
                    Debug.Log($"敵が {i} マス先のプレイヤーを攻撃！");
                    AttackForward();
                    return true;
                }
            }
        }

        return false; // 攻撃範囲にプレイヤーはいない
    }

    public void AttackForward()
    {
        Vector2Int origin = new Vector2Int(
        Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );

        Vector2Int dir = new Vector2Int(1, 0); // 右方向に攻撃

        GameObject target = CombatManager.GetObjectInLine(origin, dir, attackRange);

        if (target != null)
        {
            Debug.Log($"敵は {attackRange} マス先の {target.name} を攻撃した！");
            // ダメージ処理
            BaseDamage.instance.Damage(attackPower, hp);

            Debug.Log($"{target.name} に {attackPower} のダメージを与えた！");
            if(hp == 0)
            {
                Debug.Log($"{target.name} は倒れた！");
            }
        }
        else
        {
            Debug.Log("攻撃は外れた（対象なし）");
        }
    }

}
