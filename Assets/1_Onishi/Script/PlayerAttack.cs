using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの攻撃処理および攻撃予測（ハイライト表示）を管理するクラス。
/// グリッドベースの2Dゲームを想定し、プレイヤー位置を基準に
/// 指定方向の n マス先に攻撃を行う。
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    private PlayerStatus playerStatus;

    // 現在表示中のハイライトインスタンス
    private GameObject currentHighlight;

    void Awake()
    {
        // 同じオブジェクトに付いている PlayerStatus を取得
        playerStatus = GetComponent<PlayerStatus>();
        if(playerStatus == null)
        {
            Debug.LogError("PlayerStatus コンポーネントが見つかりません！");
        }
    }

    /// <summary>
    /// 指定方向へ攻撃を行う。
    /// </summary>
    /// <param name="dir">攻撃方向（上下左右）を示す Vector2Int</param>
    public void AttackForward(Vector2Int dir)
    {
        Vector2Int origin = Vector2Int.RoundToInt(transform.position);
        int range = playerStatus.Range;

        bool hitAny = false;

        // シーン内のボスヒットボックスを取得（1体想定）
        BossHitbox bossHitbox = FindObjectOfType<BossHitbox>();
        EnemyStatus bossStatus = null;

        if (bossHitbox != null)
        {
            bossStatus = bossHitbox.GetComponent<EnemyStatus>();
        }

        for (int i = 1; i <= range; i++)
        {
            Vector2Int checkPos = origin + dir * i;

            // =========================
            // 通常敵の判定
            // =========================
            GameObject target = CombatManager.GetObjectAt(checkPos);
            if (target != null)
            {
                EnemyStatus enemy = target.GetComponent<EnemyStatus>();
                if (enemy != null)
                {
                    DealDamage(enemy);
                    hitAny = true;
                }
            }

            // =========================
            // ボスの判定（3×3）
            // =========================
            if (bossHitbox != null && bossStatus != null)
            {
                if (bossHitbox.GetOccupiedTiles().Contains(checkPos))
                {
                    DealDamage(bossStatus);
                    hitAny = true;
                }
            }
        }

        if (!hitAny)
        {
            Debug.Log("攻撃は空振りしました。");
        }

        HighlightManager.instance.Clear();
    }

    void DealDamage(EnemyStatus enemy)
    {
        bool isCritical;
        int damage = CalculateDamage(out isCritical);

        Debug.Log(
            isCritical
                ? $"【CRITICAL】{enemy.name} に {damage} ダメージ！"
                : $"{enemy.name} に {damage} ダメージ"
        );

        enemy.TakeDamage(damage);
    }


    int CalculateDamage(out bool isCritical)
    {
        int baseDamage = playerStatus.Attack;

        // 消費攻撃UP（倍率系）
        baseDamage = Mathf.RoundToInt(baseDamage * playerStatus.UseAttackBoost);

        // クリティカル判定
        float roll = UnityEngine.Random.value;
        isCritical = roll < playerStatus.CritChance;

        if (isCritical)
        {
            baseDamage = Mathf.RoundToInt(baseDamage * 1.5f);
            Debug.Log("クリティカル!!");
        }

        return baseDamage;
    }

    /// <summary>
    /// プレイヤーが攻撃方向を選択している時、
    /// 実際に攻撃が届くマスを視覚的に表示する。
    /// </summary>
    /// <param name="dir">攻撃方向</param>
    public void ShowHighlight(Vector2Int dir)
    {
        // 攻撃方向が未指定の場合は何もしない
        if (dir == Vector2Int.zero) return;

        // プレイヤーの現在グリッド位置
        Vector2Int origin = Vector2Int.RoundToInt(transform.position);
        List<Vector2Int> tiles = new List<Vector2Int>();

        int range = playerStatus.Range; // ★ ここも

        // nマス分を計算
        for (int i = 1; i <= range; i++)
        {
            tiles.Add(origin + dir * i);
        }

        HighlightManager.instance.ShowTiles(tiles);
    }

    /// <summary>
    /// 現在表示されている攻撃ハイライトを削除する。
    /// 攻撃実行時や方向変更時に呼び出される。
    /// </summary>
    public void ClearHighlight()
    {
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
            currentHighlight = null;
        }
    }
}
