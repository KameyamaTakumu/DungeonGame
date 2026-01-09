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
    [CustomLabel("必中攻撃")] public EnemyAttackData directAttack;
    [CustomLabel("左右３×３攻撃")] public EnemyAttackData sideWideAttack;

    public enum BossActionType
    {
        FrontWide,
        FrontLine,
        PlayerTarget,
        SideWide,
    }

    [SerializeField]
    List<BossActionType> actionPatterns = new List<BossActionType>()
    {
        BossActionType.FrontWide,
        BossActionType.FrontLine,
        BossActionType.PlayerTarget,
        BossActionType.SideWide
    };

    /// <summary>
    /// ボスの行動開始。
    /// ターン制バトルマネージャーから呼ばれる想定。
    /// </summary>
    public void BossAction()
    {
        BossActionType action =
       actionPatterns[Random.Range(0, actionPatterns.Count)];

        switch (action)
        {
            case BossActionType.FrontWide:
                ExecuteFrontWideAttack();
                break;

            case BossActionType.FrontLine:
                ExecuteFrontLineAttack();
                break;

            case BossActionType.PlayerTarget:
                ExecutePlayerTargetAttack();
                break;

            case BossActionType.SideWide:
                ExecuteSideWideAttack();
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

    void ExecutePlayerTargetAttack()
    {
        Debug.Log("ボスの必中攻撃！");

        PlayerStatus player = GetPlayerStatus();

        // ハイライト不要 or プレイヤー位置だけ表示してもOK
        HighlightManager.instance.ShowTiles(
            new List<Vector2Int> { GetPlayerGridPos() }
        );

        player.TakeDamage(directAttack.damage);

        if (directAttack.knockbackY != 0)
        {
            player.transform.position += new Vector3(0, directAttack.knockbackY, 0);
        }
    }

    /// <summary>
    /// ボスの左右に対する3×3範囲攻撃。
    /// 左右それぞれに判定を持つ。
    /// </summary>
    void ExecuteSideWideAttack()
    {
        var area = GetSideWideArea();
        HighlightManager.instance.ShowTiles(area);

        Vector2Int playerPos = GetPlayerGridPos();

        if (area.Contains(playerPos))
        {
            ApplyAttackToPlayer(sideWideAttack);
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
                tiles.Add(new Vector2Int(center.x + x, center.y - 1 - y));
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
            tiles.Add(new Vector2Int(center.x + x, center.y - 2));
        }

        return tiles;
    }

    /// <summary>
    /// ボス左右の3×3範囲を生成する。
    /// ボスは3×3サイズを想定。
    /// </summary>
    List<Vector2Int> GetSideWideArea()
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        Vector2Int center = Vector2Int.RoundToInt(transform.position);

        // 左側 3×3
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -4; x <= -2; x++)
            {
                tiles.Add(new Vector2Int(center.x + x, center.y + y));
            }
        }

        // 右側 3×3
        for (int y = -1; y <= 1; y++)
        {
            for (int x = 2; x <= 4; x++)
            {
                tiles.Add(new Vector2Int(center.x + x, center.y + y));
            }
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