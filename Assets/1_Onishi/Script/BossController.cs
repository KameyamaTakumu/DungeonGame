using System.Collections.Generic;
using UnityEngine;
using static EnemyStatus;

/// <summary>
/// ボス専用の行動制御クラス。
/// 雑魚敵とは異なり「移動せず、範囲攻撃でプレイヤーを制圧する」
/// ことを目的としたAI設計になっている。
/// 
/// 各攻撃は
/// ・攻撃範囲の計算
/// ・予兆（ハイライト表示）
/// ・ダメージ／特殊効果の適用
/// の3段階で構成されており、拡張性を重視している。
/// </summary>
public class BossController : MonoBehaviour
{
    [Header("ボスの攻撃")]
    [CustomLabel("ノックバック攻撃")] public EnemyAttackData frontWideAttack;
    [CustomLabel("範囲攻撃１×５")]   public EnemyAttackData frontLineAttack;

    /// <summary>
    /// ボスの行動開始。
    /// ターン制バトルマネージャーから呼ばれる想定。
    /// </summary>
    public void BossAction()
    {
        // ランダムで行動を選択
        int action = Random.Range(0, 2);

        switch (action)
        {
            case 0:
                ExecuteFrontWideAttack();
                break;

            case 1:
                ExecuteFrontLineAttack();
                break;
        }
    }

    /// <summary>
    /// ボス正面に対する2×3範囲攻撃。
    /// プレイヤーが範囲内にいる場合、
    /// ダメージ＋Y方向に2マスのノックバックを行う。
    /// </summary>
    void ExecuteFrontWideAttack()
    {
        var area = GetFrontWideArea();
        HighlightManager.instance.ShowTiles(area);

        Vector2Int playerPos = GetPlayerGridPos();

        if (area.Contains(playerPos))
        {
            ApplyAttackToPlayer(frontWideAttack);
        }
    }

    /// <summary>
    /// ボス正面に対する5×1の横一直線範囲攻撃。
    /// シンプルな範囲ダメージ攻撃として使用。
    /// </summary>
    void ExecuteFrontLineAttack()
    {
        var area = GetFrontLineArea();
        HighlightManager.instance.ShowTiles(area);

        Vector2Int playerPos = GetPlayerGridPos();

        if (area.Contains(playerPos))
        {
            ApplyAttackToPlayer(frontLineAttack);
        }
    }

    /// <summary>
    /// プレイヤーに攻撃を適用する
    /// </summary>
    /// <param name="attack"></param>
    void ApplyAttackToPlayer(EnemyStatus.EnemyAttackData attack)
    {
        PlayerStatus player = GetPlayerStatus();

        player.TakeDamage(attack.damage);

        if (attack.knockbackY != 0)
        {
            player.transform.position += new Vector3(0, attack.knockbackY, 0);
        }
    }

    /// <summary>
    /// ボス正面（下方向）に2×3の攻撃範囲を生成する。
    /// ボスは3×3サイズのため、足元基準で計算している。
    /// </summary>
    List<Vector2Int> GetFrontWideArea()
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        Vector2Int center = Vector2Int.RoundToInt(transform.position);

        // 正面に2マス
        for (int y = 1; y <= 2; y++)
        {
            // 横3マス
            for (int x = -1; x <= 1; x++)
            {
                tiles.Add(new Vector2Int(center.x + x, center.y - 2 - y));
            }
        }

        return tiles;
    }

    /// <summary>
    /// ボス正面に横5マスの攻撃範囲を生成する。
    /// </summary>
    List<Vector2Int> GetFrontLineArea()
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        Vector2Int center = Vector2Int.RoundToInt(transform.position);

        // 横5マス
        for (int x = -2; x <= 2; x++)
        {
            tiles.Add(new Vector2Int(center.x + x, center.y - 3));
        }

        return tiles;
    }

    /// <summary>
    /// プレイヤーの現在のグリッド座標を取得する。
    /// </summary>
    Vector2Int GetPlayerGridPos()
    {
        return Vector2Int.RoundToInt(
            GameObject.FindGameObjectWithTag("Player").transform.position
        );
    }

    /// <summary>
    /// プレイヤーの PlayerStatus コンポーネントを取得する。
    /// </summary>
    PlayerStatus GetPlayerStatus()
    {
        return GameObject.FindGameObjectWithTag("Player")
                         .GetComponent<PlayerStatus>();
    }
}