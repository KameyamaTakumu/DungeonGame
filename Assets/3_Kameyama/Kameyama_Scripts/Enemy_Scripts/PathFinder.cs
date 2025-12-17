using System.Collections.Generic;
using UnityEngine;

public static class PathFinder
{
    static readonly Vector2Int[] dirs =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    class Node
    {
        public Vector2Int pos;
        public int g; // 開始からのコスト
        public int h; // ゴールまでの推定
        public int f => g + h;
        public Node parent;

        public Node(Vector2Int pos, int g, int h, Node parent)
        {
            this.pos = pos;
            this.g = g;
            this.h = h;
            this.parent = parent;
        }
    }

    static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    /// <summary>
    /// A*で経路を計算
    /// </summary>
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        var open = new List<Node>();
        var closed = new HashSet<Vector2Int>();

        open.Add(new Node(start, 0, Heuristic(start, goal), null));

        while (open.Count > 0)
        {
            // f値が最小のノードを選択
            open.Sort((a, b) => a.f.CompareTo(b.f));
            Node current = open[0];
            open.RemoveAt(0);

            if (current.pos == goal)
                return BuildPath(current);

            closed.Add(current.pos);

            foreach (var d in dirs)
            {
                Vector2Int next = current.pos + d;

                if (closed.Contains(next))
                    continue;

                if (!GridMap.instance.IsWalkable(next))
                    continue;

                int g = current.g + 1;
                int h = Heuristic(next, goal);

                Node existing = open.Find(n => n.pos == next);
                if (existing == null)
                {
                    open.Add(new Node(next, g, h, current));
                }
                else if (g < existing.g)
                {
                    existing.g = g;
                    existing.parent = current;
                }
            }
        }

        return null;
    }

    static List<Vector2Int> BuildPath(Node node)
    {
        var path = new List<Vector2Int>();
        while (node != null)
        {
            path.Add(node.pos);
            node = node.parent;
        }
        path.Reverse();
        return path;
    }
}
