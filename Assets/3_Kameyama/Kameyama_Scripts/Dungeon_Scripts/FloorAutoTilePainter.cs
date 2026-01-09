using UnityEngine;
using UnityEngine.Tilemaps;

public class FloorAutoTilePainter : MonoBehaviour
{
    [SerializeField] Tilemap floorTilemap;

    [Header("è∞É^ÉCÉã")]
    [SerializeField] TileBase center;
    [SerializeField] TileBase edgeUp;
    [SerializeField] TileBase edgeDown;
    [SerializeField] TileBase edgeLeft;
    [SerializeField] TileBase edgeRight;

    [SerializeField] TileBase cornerTL;
    [SerializeField] TileBase cornerTR;
    [SerializeField] TileBase cornerBL;
    [SerializeField] TileBase cornerBR;

    public void Apply(TileType[,] map)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (map[x, y] != TileType.Floor) continue;

                bool N = IsWall(map, x, y + 1);
                bool S = IsWall(map, x, y - 1);
                bool E = IsWall(map, x + 1, y);
                bool W = IsWall(map, x - 1, y);

                TileBase tile = center;

                // äpóDêÊ
                if (N && W) tile = cornerTL;
                else if (N && E) tile = cornerTR;
                else if (S && W) tile = cornerBL;
                else if (S && E) tile = cornerBR;

                // ï”
                else if (N) tile = edgeUp;
                else if (S) tile = edgeDown;
                else if (W) tile = edgeLeft;
                else if (E) tile = edgeRight;

                floorTilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    bool IsWall(TileType[,] map, int x, int y)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        if (x < 0 || y < 0 || x >= w || y >= h) return true;
        return map[x, y] == TileType.Wall;
    }
}
