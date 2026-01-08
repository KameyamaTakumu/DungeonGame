using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ボスが占有しているグリッド座標を管理するクラス。
/// プレイヤー攻撃判定や範囲攻撃判定に使用される。
/// </summary>
public class BossHitbox : MonoBehaviour
{
    /// <summary>
    /// ボスが現在占有している全マスを返す（3×3）
    /// </summary>
    public List<Vector2Int> GetOccupiedTiles()
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        Vector2Int center = Vector2Int.RoundToInt(transform.position);

        //Debug.Log($"[BOSS] center = {center}");

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                tiles.Add(new Vector2Int(center.x + x, center.y + y));
            }
        }

        return tiles;
    }
}
