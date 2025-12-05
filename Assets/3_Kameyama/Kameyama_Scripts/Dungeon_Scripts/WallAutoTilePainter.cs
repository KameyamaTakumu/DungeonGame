using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 壁タイルの自動タイル適用を担当するクラス
/// </summary>
public class WallAutoTilePainter : MonoBehaviour
{
    [Header("タイルマップ")]
    [CustomLabel("当たり判定付き壁タイルマップ"), SerializeField]
    private Tilemap wallTilemap;

    [Header("必要な壁タイル")]
    [Header("内側L字")]
    [CustomLabel("内側L字左上"), SerializeField]
    private TileBase cornerTL;
    [CustomLabel("内側L字右上"), SerializeField]
    private TileBase cornerTR;
    [CustomLabel("内側L字右下"), SerializeField]
    private TileBase cornerBR;
    [CustomLabel("内側L字左下"), SerializeField]
    private TileBase cornerBL;

    [Header("外側L字")]
    [CustomLabel("L字左下"), SerializeField]
    private TileBase innerTR;
    [CustomLabel("L字右下"), SerializeField]
    private TileBase innerTL;
    [CustomLabel("L字左上"), SerializeField]
    private TileBase innerBR;
    [CustomLabel("L字右上"), SerializeField]
    private TileBase innerBL;

    [Header("通常壁")]
    [CustomLabel("下"), SerializeField]
    private TileBase tUp;
    [CustomLabel("左"), SerializeField]
    private TileBase tRight;
    [CustomLabel("上"), SerializeField]
    private TileBase tDown;
    [CustomLabel("右"), SerializeField]
    private TileBase tLeft;

    [Header("完全壁")]
    [CustomLabel("完全壁"), SerializeField]
    private TileBase cross;

    /// <summary>
    /// 自動タイル適用を実行する
    /// </summary>
    /// <param name="map">タイルマップ配列</param>
    public void ApplyAutoTiles(TileType[,] map)
    {
        int width  = map.GetLength(0);
        int height = map.GetLength(1);

        wallTilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] != TileType.Wall) continue;

                Vector3Int pos = new Vector3Int(x, y, 0);

                // 8方向の壁の有無をチェック
                // N, NE, E, SE, S, SW, W, NW
                bool N  = IsWall(map, x,     y + 1);    // 北
                bool NE = IsWall(map, x + 1, y + 1);    // 北東
                bool E  = IsWall(map, x + 1, y);        // 東
                bool SE = IsWall(map, x + 1, y - 1);    // 南東
                bool S  = IsWall(map, x,     y - 1);    // 南
                bool SW = IsWall(map, x - 1, y - 1);    // 南西
                bool W  = IsWall(map, x - 1, y);        // 西
                bool NW = IsWall(map, x - 1, y + 1);    // 北西

                TileBase chosen = null;

                // --- 内角 ---
                if      (E && N && !NE) chosen = innerTR;
                else if (W && N && !NW) chosen = innerTL;
                else if (E && S && !SE) chosen = innerBR;
                else if (W && S && !SW) chosen = innerBL;

                // --- 外角 ---
                if (chosen == null)
                {
                    if      (!E && !N && NE) chosen = cornerTR;
                    else if (!W && !N && NW) chosen = cornerTL;
                    else if (!E && !S && SE) chosen = cornerBR;
                    else if (!W && !S && SW) chosen = cornerBL;
                }

                // --- 十字 ---
                if (chosen == null && N && E && S && W)
                    chosen = cross;

                // --- T字 ---
                if (chosen == null)
                {
                    if     (!N && E && S && W)  chosen = tUp;
                    else if (N && !E && S && W) chosen = tRight;
                    else if (N && E && !S && W) chosen = tDown;
                    else if (N && E && S && !W) chosen = tLeft;
                }

                // --- 2方向（L字など） ---
                if (chosen == null)
                {
                    if      (N && E) chosen = cornerTR;
                    else if (E && S) chosen = cornerBR;
                    else if (S && W) chosen = cornerBL;
                    else if (W && N) chosen = cornerTL;
                }

                wallTilemap.SetTile(pos, chosen);
            }
        }
    }

    /// <summary>
    /// 指定座標が壁タイルかどうかを判定する
    /// </summary>
    /// <param name="map">タイルマップ配列</param>
    /// <param name="x">X座標</param>
    /// <param name="y">Y座標</param>
    private bool IsWall(TileType[,] map, int x, int y)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        if (x < 0 || y < 0 || x >= w || y >= h) return true;

        return map[x, y] == TileType.Wall;
    }
}
