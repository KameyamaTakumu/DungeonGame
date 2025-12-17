using System.Collections.Generic;
using UnityEngine;

public static class DistanceMap
{
    static readonly Vector2Int[] dirs =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    /// <summary>
    /// プレイヤー位置から全床タイルへの距離を計算
    /// </summary>
    public static Dictionary<Vector2Int, int> Build(
        Vector2Int playerPos,
        DungeonGenerator dungeon)
    {
        var dist = new Dictionary<Vector2Int, int>();
        var queue = new Queue<Vector2Int>();

        queue.Enqueue(playerPos);
        dist[playerPos] = 0;

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();

            foreach (var d in dirs)
            {
                Vector2Int next = cur + d;

                // 範囲外
                if (next.x < 0 || next.y < 0 ||
                    next.x >= dungeon.width ||
                    next.y >= dungeon.height)
                    continue;

                // 壁は不可
                if (dungeon.map[next.x, next.y] == TileType.Wall)
                    continue;

                if (dist.ContainsKey(next))
                    continue;

                dist[next] = dist[cur] + 1;
                queue.Enqueue(next);
            }
        }

        return dist;
    }
}
