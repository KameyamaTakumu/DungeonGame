using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 床タイルの自動タイル適用を担当するクラス
/// </summary>
public class FloorAutoTilePainter : MonoBehaviour
{
    [Header("タイルマップ")]
    [CustomLabel("床タイルマップ"), SerializeField]
    private Tilemap floorTilemap;

    [Header("床タイル")]
    [CustomLabel("中央（壁なし）"), SerializeField]
    private TileBase center;

    [Header("辺タイル（1方向が壁）")]
    [CustomLabel("上辺"), SerializeField]
    private TileBase edgeUp;
    [CustomLabel("下辺"), SerializeField]
    private TileBase edgeDown;
    [CustomLabel("左辺"), SerializeField]
    private TileBase edgeLeft;
    [CustomLabel("右辺"), SerializeField]
    private TileBase edgeRight;

    [Header("コーナータイル（2方向が壁）")]
    [CustomLabel("左上コーナー"), SerializeField]
    private TileBase cornerTL;
    [CustomLabel("右上コーナー"), SerializeField]
    private TileBase cornerTR;
    [CustomLabel("左下コーナー"), SerializeField]
    private TileBase cornerBL;
    [CustomLabel("右下コーナー"), SerializeField]
    private TileBase cornerBR;

    /// <summary>
    /// 自動タイル適用を実行する
    /// </summary>
    /// <param name="map">タイルマップ配列</param>
    public void Apply(TileType[,] map)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        floorTilemap.ClearAllTiles();

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (map[x, y] != TileType.Floor) continue;

                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = SelectTile(map, x, y);

                // null の場合はタイルを置かない
                if (tile != null)
                    floorTilemap.SetTile(pos, tile);
            }
        }
    }

    /// <summary>
    /// 座標 (x, y) の床タイルに対して適切なタイルを選択して返す
    /// 優先順位: コーナー → 辺 → 中央
    /// どのパターンにも該当しない場合は null を返す
    /// </summary>
    private TileBase SelectTile(TileType[,] map, int x, int y)
    {
        // 4方向の隣接タイルが壁かどうかをチェック (N=北=Y+)
        bool N = IsWall(map, x, y + 1);
        bool S = IsWall(map, x, y - 1);
        bool E = IsWall(map, x + 1, y);
        bool W = IsWall(map, x - 1, y);

        // -------------------------------------------------------------------------
        // 1. コーナー（2方向が壁）
        //    左上コーナー = 北と西が壁
        //    右上コーナー = 北と東が壁
        //    左下コーナー = 南と西が壁
        //    右下コーナー = 南と東が壁
        // -------------------------------------------------------------------------
        if (N && W) return cornerTL;
        if (N && E) return cornerTR;
        if (S && W) return cornerBL;
        if (S && E) return cornerBR;

        // -------------------------------------------------------------------------
        // 2. 辺（1方向のみ壁）
        // -------------------------------------------------------------------------
        if (N) return edgeUp;
        if (S) return edgeDown;
        if (W) return edgeLeft;
        if (E) return edgeRight;

        // -------------------------------------------------------------------------
        // 3. 中央（周囲に壁なし）
        // -------------------------------------------------------------------------
        return center;
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
