using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// RPGツクール風 47種類オートタイル実装
/// - wallTiles length must be 47 (0..46)
/// - Inspector に並べる順序は下の enum (TileIndex) を参照してください
/// </summary>
public class WallAutoTilePainter : MonoBehaviour
{
    [Header("壁タイル（47種類）")]
    [Tooltip("0..46 の順でタイルを割り当ててください。コード内のTileIndex参照")]
    public TileBase[] wallTiles = new TileBase[47];

    public Tilemap wallTilemap;

    // 使いやすいように意味のある名前でインデックスを定義
    private enum TileIndex
    {
        // 0-3: 単純な端（上下左右単独）
        Isolated = 0,    // 0: 孤立（使わない場合あり）
        EdgeTop = 1,     // 1: 上だけ接続（上に壁）
        EdgeRight = 2,   // 2: 右だけ
        EdgeBottom = 3,  // 3: 下だけ
        EdgeLeft = 4,    // 4: 左だけ

        // 5-10: 直線・L字など
        Vertical = 5,    // 5: 上下
        Horizontal = 6,  // 6: 左右
        CornerTL = 7,    // 7: 左上外角（見た目の命名は素材に合わせて）
        CornerTR = 8,    // 8: 右上外角
        CornerBR = 9,    // 9: 右下外角
        CornerBL = 10,   // 10: 左下外角

        // 11-14: 内角（床が斜めにあるときに使う内側の角）
        InnerTR = 11,    // 11: 内角（右上が床）
        InnerTL = 12,    // 12: 内角（左上が床）
        InnerBR = 13,    // 13: 内角（右下が床）
        InnerBL = 14,    // 14: 内角（左下が床）

        // 15-18: 3方向（T字）
        TJunctionUp = 15,    // 15: T字（上へ繋がる）
        TJunctionRight = 16, // 16: T字（右へ）
        TJunctionDown = 17,  // 17: T字（下へ）
        TJunctionLeft = 18,  // 18: T字（左へ）

        // 19: 四方すべて（十字）
        Cross = 19, // 19: 四方に壁

        // 20-23: 端の小バリエーション（素材によって使い分け）
        // ここから下は必要に応じて素材合わせで調整してください
        // ここでは 47 個に達するまで placeholder として配置
        Misc20 = 20,
        Misc21 = 21,
        Misc22 = 22,
        Misc23 = 23,
        Misc24 = 24,
        Misc25 = 25,
        Misc26 = 26,
        Misc27 = 27,
        Misc28 = 28,
        Misc29 = 29,
        Misc30 = 30,
        Misc31 = 31,
        Misc32 = 32,
        Misc33 = 33,
        Misc34 = 34,
        Misc35 = 35,
        Misc36 = 36,
        Misc37 = 37,
        Misc38 = 38,
        Misc39 = 39,
        Misc40 = 40,
        Misc41 = 41,
        Misc42 = 42,
        Misc43 = 43,
        Misc44 = 44,
        Misc45 = 45,
        Misc46 = 46
    }

    public void ApplyAutoTiles(TileType[,] map)
    {
        if (wallTiles == null || wallTiles.Length < 47)
        {
            Debug.LogError("wallTiles must contain 47 TileBase entries.");
            return;
        }

        int width = map.GetLength(0);
        int height = map.GetLength(1);

        wallTilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] != TileType.Wall) continue;

                Vector3Int pos = new Vector3Int(x, y, 0);

                // 8方向チェック
                bool N = IsWall(map, x, y + 1);
                bool NE = IsWall(map, x + 1, y + 1);
                bool E = IsWall(map, x + 1, y);
                bool SE = IsWall(map, x + 1, y - 1);
                bool S = IsWall(map, x, y - 1);
                bool SW = IsWall(map, x - 1, y - 1);
                bool W = IsWall(map, x - 1, y);
                bool NW = IsWall(map, x - 1, y + 1);

                TileBase chosen = null;

                // 1) 内角（inner corner）判定：斜めが「床（＝壁なし）」なら内角
                // 例：右上内角 -> E && N && !NE
                if (E && N && !NE)
                {
                    chosen = wallTiles[(int)TileIndex.InnerTR];
                }
                else if (W && N && !NW)
                {
                    chosen = wallTiles[(int)TileIndex.InnerTL];
                }
                else if (E && S && !SE)
                {
                    chosen = wallTiles[(int)TileIndex.InnerBR];
                }
                else if (W && S && !SW)
                {
                    chosen = wallTiles[(int)TileIndex.InnerBL];
                }

                // 2) 外角（outer corner）判定：斜めが壁で、隣接のどちらかが床のケース
                // 例：右上外角 -> !E && !N && NE
                if (chosen == null)
                {
                    if (!E && !N && NE)
                        chosen = wallTiles[(int)TileIndex.CornerTR];
                    else if (!W && !N && NW)
                        chosen = wallTiles[(int)TileIndex.CornerTL];
                    else if (!E && !S && SE)
                        chosen = wallTiles[(int)TileIndex.CornerBR];
                    else if (!W && !S && SW)
                        chosen = wallTiles[(int)TileIndex.CornerBL];
                }

                // 3) 十字判定
                if (chosen == null && N && E && S && W)
                {
                    chosen = wallTiles[(int)TileIndex.Cross];
                }

                // 4) T 字判定（3方向接続）
                if (chosen == null)
                {
                    // T with opening to top (i.e., connected left, right, down)
                    if (!N && E && S && W) chosen = wallTiles[(int)TileIndex.TJunctionUp];
                    // T with opening to right
                    else if (N && !E && S && W) chosen = wallTiles[(int)TileIndex.TJunctionRight];
                    // T with opening to bottom
                    else if (N && E && !S && W) chosen = wallTiles[(int)TileIndex.TJunctionDown];
                    // T with opening to left
                    else if (N && E && S && !W) chosen = wallTiles[(int)TileIndex.TJunctionLeft];
                }

                // 5) 直線（vertical/horizontal）
                if (chosen == null)
                {
                    if (N && S && !E && !W)
                        chosen = wallTiles[(int)TileIndex.Vertical];
                    else if (E && W && !N && !S)
                        chosen = wallTiles[(int)TileIndex.Horizontal];
                }

                // 6) 単純な端（片側のみ接続）
                if (chosen == null)
                {
                    if (N && !E && !S && !W) chosen = wallTiles[(int)TileIndex.EdgeTop];
                    else if (E && !N && !S && !W) chosen = wallTiles[(int)TileIndex.EdgeRight];
                    else if (S && !N && !E && !W) chosen = wallTiles[(int)TileIndex.EdgeBottom];
                    else if (W && !N && !E && !S) chosen = wallTiles[(int)TileIndex.EdgeLeft];
                }

                // 7) 残りは bitmask を使った細かい判定（2方向接続など）
                if (chosen == null)
                {
                    // 上+右
                    if (N && E && !S && !W) chosen = wallTiles[(int)TileIndex.CornerTR];
                    // 右+下
                    else if (E && S && !N && !W) chosen = wallTiles[(int)TileIndex.CornerBR];
                    // 下+左
                    else if (S && W && !N && !E) chosen = wallTiles[(int)TileIndex.CornerBL];
                    // 左+上
                    else if (W && N && !E && !S) chosen = wallTiles[(int)TileIndex.CornerTL];
                    // 上下左右いずれも false（孤立）
                    else if (!N && !E && !S && !W) chosen = wallTiles[(int)TileIndex.Isolated];
                    else
                    {
                        // その他の混合ケースは素材に合わせて適当に fallback
                        // 例えば 2 方向（斜めを含む）や 1 方向の微妙な組み合わせ
                        // ここでは horizontal をデフォルトにしておく
                        chosen = wallTiles[(int)TileIndex.Misc20];
                    }
                }

                // 最後に tilemap にセット
                wallTilemap.SetTile(pos, chosen);
            }
        }
    }

    // 範囲外は壁扱いにする（タイル境界を壁で包みたい場合）
    private bool IsWall(TileType[,] map, int x, int y)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);
        if (x < 0 || y < 0 || x >= w || y >= h) return true;
        return map[x, y] == TileType.Wall;
    }
}
