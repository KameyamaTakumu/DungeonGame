using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 雑魚敵専用の攻撃処理クラス。
/// </summary>
public class EnemyAttack : MonoBehaviour
{
    // 敵ステータス
    private EnemyStatus enemyStatus;

    // 攻撃力
    private int atk;

    // 攻撃レンジ（直線距離）
    private int range;

    // 最後に選択した攻撃方向
    private Vector2Int attackDir;

    private void Awake()
    {
        // EnemyStatus を取得
        enemyStatus = GetComponent<EnemyStatus>();

        if (enemyStatus == null)
        {
            Debug.LogError("EnemyStatus コンポーネントが見つかりません！");
            return;
        }

        // ステータス反映
        atk   = enemyStatus.status.ATK;
        range = enemyStatus.status.RANGE;
    }

    /// <summary>
    /// 攻撃可能であれば攻撃シーケンスを開始する。
    /// 
    /// ・すでに移動済みの敵は攻撃しない
    /// ・上下左右4方向をチェック
    /// ・RANGE 内にプレイヤーがいれば攻撃
    /// </summary>
    public bool TryAttackPlayer()
    {
        EnemyMovement mv = GetComponent<EnemyMovement>();

        // すでに移動していた場合は攻撃しない
        if (mv != null && mv.hasMoved)
            return false;

        Vector2Int origin = Vector2Int.RoundToInt(transform.position);
        Vector2Int playerGrid = Vector2Int.RoundToInt(
            GameObject.FindGameObjectWithTag("Player").transform.position
        );

        // 上下左右4方向
        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var dir in dirs)
        {
            for (int i = 1; i <= range; i++)
            {
                Vector2Int checkPos = origin + dir * i;

                // プレイヤーが攻撃範囲内にいるか
                if (playerGrid == checkPos)
                {
                    attackDir = dir;

                    // ★ 即攻撃せず、予兆付き攻撃を開始
                    StartCoroutine(AttackSequence(dir));

                    if (mv != null)
                        mv.hasAttacked = true;

                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 攻撃予兆 → 攻撃実行の一連の流れ。
    /// 視覚的に分かりやすい戦闘を実現するため、
    /// 予兆表示とダメージ処理を分離している。
    /// </summary>
    private IEnumerator AttackSequence(Vector2Int dir)
    {
        Vector2Int origin = Vector2Int.RoundToInt(transform.position);

        // ① 攻撃範囲を計算
        List<Vector2Int> area = GetAttackArea(origin, dir);

        // ② 攻撃予兆表示
        HighlightManager.instance.ShowTiles(area);

        // ③ 予兆演出待ち
        yield return new WaitForSeconds(0.5f);

        // ④ 攻撃実行
        ExecuteAttack(dir);

        // ⑤ ハイライト削除
        HighlightManager.instance.Clear();
    }

    /// <summary>
    /// 指定方向に対する直線攻撃の範囲を取得する。
    /// </summary>
    private List<Vector2Int> GetAttackArea(Vector2Int origin, Vector2Int dir)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();

        for (int i = 1; i <= range; i++)
        {
            tiles.Add(origin + dir * i);
        }

        return tiles;
    }

    /// <summary>
    /// 実際のダメージ処理のみを担当する。
    /// 攻撃演出や判定とは分離されている。
    /// </summary>
    private void ExecuteAttack(Vector2Int dir)
    {
        Vector2Int origin = Vector2Int.RoundToInt(transform.position);

        for (int i = 1; i <= range; i++)
        {
            Vector2Int checkPos = origin + dir * i;

            // プレイヤーがそのマスにいるか
            if (CombatManager.IsPlayerAt(checkPos))
            {
                GameObject playerObj =
                    GameObject.FindGameObjectWithTag("Player");

                PlayerStatus player = playerObj.GetComponent<PlayerStatus>();
                if (player != null)
                {
                    player.TakeDamage(atk);
                    Debug.Log($"{name} が {i} マス先のプレイヤーに {atk} ダメージ");
                }

                // 直線攻撃なので最初に当たったら終了
                return;
            }
        }
    }

}

