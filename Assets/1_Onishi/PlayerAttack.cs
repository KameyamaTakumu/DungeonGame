using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの攻撃処理および攻撃予測（ハイライト表示）を管理するクラス。
/// グリッドベースの2Dゲームを想定し、プレイヤー位置を基準に
/// 指定方向の n マス先に攻撃を行う。
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Tooltip("攻撃を行う距離（例：1 → 1マス先に攻撃）")]
    public int attackRange = 1;

    private PlayerStatus playerStatus;

    // ステータスを PlayerStatus から取得
    int range;
    int atk;

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
        else
        {
            // ステータスを PlayerStatus から取得
            range = playerStatus.status.RANGE;
            atk = playerStatus.status.ATK;
        }
    }

    /// <summary>
    /// 指定方向へ攻撃を行う。
    /// </summary>
    /// <param name="dir">攻撃方向（上下左右）を示す Vector2Int</param>
    public void AttackForward(Vector2Int dir)
    {
        // プレイヤーの現在グリッド位置
        Vector2Int origin = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );

        // 指定方向に attackRange マス先のターゲット取得
        GameObject target = CombatManager.GetObjectInLine(origin, dir, range);

        if (target != null)
        {
            Debug.Log($"敵 {target.name} に攻撃！ ダメージ {atk}");

            EnemyStatus enemy = target.GetComponent<EnemyStatus>();
            if (enemy != null)
            {
                enemy.TakeDamage(atk);
            }
        }
        else
        {
            Debug.Log("攻撃は空振りしました。");
        }

        // 攻撃後はハイライトを消去
        HighlightManager.instance.Clear();
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
