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

    [Header("外角タイル（凸コーナー）")]
    [CustomLabel("外角・左上"), SerializeField]
    private TileBase cornerTL;
    [CustomLabel("外角・右上"), SerializeField]
    private TileBase cornerTR;
    [CustomLabel("外角・右下"), SerializeField]
    private TileBase cornerBR;
    [CustomLabel("外角・左下"), SerializeField]
    private TileBase cornerBL;

    [Header("内角タイル（凹コーナー）")]
    [CustomLabel("内角・左上"), SerializeField]
    private TileBase innerTL;
    [CustomLabel("内角・右上"), SerializeField]
    private TileBase innerTR;
    [CustomLabel("内角・右下"), SerializeField]
    private TileBase innerBR;
    [CustomLabel("内角・左下"), SerializeField]
    private TileBase innerBL;

    [Header("T字タイル（開口方向）")]
    [CustomLabel("上が開口"), SerializeField]
    private TileBase tUp;
    [CustomLabel("右が開口"), SerializeField]
    private TileBase tRight;
    [CustomLabel("下が開口"), SerializeField]
    private TileBase tDown;
    [CustomLabel("左が開口"), SerializeField]
    private TileBase tLeft;

    [Header("十字タイル")]
    [CustomLabel("十字（全方向壁）"), SerializeField]
    private TileBase cross;

    /// <summary>
    /// 自動タイル適用を実行する
    /// </summary>
    /// <param name="map">タイルマップ配列</param>
    public void ApplyAutoTiles(TileType[,] map)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        wallTilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] != TileType.Wall) continue;

                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase chosen = SelectTile(map, x, y);

                // null の場合はタイルを置かない
                if (chosen != null)
                    wallTilemap.SetTile(pos, chosen);
            }
        }
    }

    /// <summary>
    /// 座標 (x, y) の壁タイルに対して適切なタイルを選択して返す
    /// 優先順位: 内角 → 外角 → 十字 → T字 → 2方向L字
    /// どのパターンにも該当しない場合は null を返す
    /// </summary>
    private TileBase SelectTile(TileType[,] map, int x, int y)
    {
        // 8方向の壁の有無をチェック (N=北=Y+)
        bool N  = IsWall(map, x, y + 1);
        bool NE = IsWall(map, x + 1, y + 1);
        bool E  = IsWall(map, x + 1, y);
        bool SE = IsWall(map, x + 1, y - 1);
        bool S  = IsWall(map, x, y - 1);
        bool SW = IsWall(map, x - 1, y - 1);
        bool W  = IsWall(map, x - 1, y);
        bool NW = IsWall(map, x - 1, y + 1);

        // -------------------------------------------------------------------------
        // 1. 内角（凹コーナー）
        //    条件: 隣接2方向が両方壁 かつ その対角のみが空き
        //    内角・右上 = 東と北が壁 かつ 北東が空き
        //    内角・左上 = 西と北が壁 かつ 北西が空き
        //    内角・右下 = 東と南が壁 かつ 南東が空き
        //    内角・左下 = 西と南が壁 かつ 南西が空き
        // -------------------------------------------------------------------------
        if (E && N && !NE) return innerTR;
        if (W && N && !NW) return innerTL;
        if (E && S && !SE) return innerBR;
        if (W && S && !SW) return innerBL;

        // -------------------------------------------------------------------------
        // 2. 外角（凸コーナー）
        //    条件: 隣接2方向が両方空き（対角は問わない）
        //    外角・右上 = 東と北が両方空き
        //    外角・左上 = 西と北が両方空き
        //    外角・右下 = 東と南が両方空き
        //    外角・左下 = 西と南が両方空き
        // -------------------------------------------------------------------------
        if (!E && !N) return cornerTR;
        if (!W && !N) return cornerTL;
        if (!E && !S) return cornerBR;
        if (!W && !S) return cornerBL;

        // -------------------------------------------------------------------------
        // 3. 十字（全4方向が壁）
        // -------------------------------------------------------------------------
        if (N && E && S && W) return cross;

        // -------------------------------------------------------------------------
        // 4. T字（3方向が壁・1方向が開口）
        //    tUp    = 上が開口（N なし、E/S/W あり）
        //    tRight = 右が開口（E なし、N/S/W あり）
        //    tDown  = 下が開口（S なし、N/E/W あり）
        //    tLeft  = 左が開口（W なし、N/E/S あり）
        // -------------------------------------------------------------------------
        if (!N  &&  E &&  S &&  W) return tUp;
        if ( N  && !E &&  S &&  W) return tRight;
        if ( N  &&  E && !S &&  W) return tDown;
        if ( N  &&  E &&  S && !W) return tLeft;

        // -------------------------------------------------------------------------
        // 5. 2方向L字（外角と同形状のフォールバック）
        //    上記の外角判定で既にカバーされるケースが多いが
        //    対角情報によって外角に該当しなかった場合のフォールバック
        // -------------------------------------------------------------------------
        if (N && E) return cornerTR;
        if (E && S) return cornerBR;
        if (S && W) return cornerBL;
        if (W && N) return cornerTL;

        // どのパターンにも該当しない→タイルなし
        return null;
    }

    /// <summary>
    /// 指定座標が壁タイルかどうかを判定する
    /// マップ範囲外は壁として扱う
    /// </summary>
    private bool IsWall(TileType[,] map, int x, int y)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        if (x < 0 || y < 0 || x >= w || y >= h) return true;

        return map[x, y] == TileType.Wall;
    }
}
