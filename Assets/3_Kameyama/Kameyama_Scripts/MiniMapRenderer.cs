using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// ダンジョンの2D配列からミニマップを描画するクラス。
/// 床（Floor）だけをTilemapに表示する。
/// </summary>
public class MiniMapRenderer : MonoBehaviour
{
    [Header("参照するタイルマップ")]
    [SerializeField] private Tilemap miniMapTilemap;

    [Header("表示するタイル（床のみ）")]
    [SerializeField] private TileBase miniMapFloorTile;

    [Header("ミニマップのスケール（タイルの大きさ）")]
    [SerializeField] private float tileScale = 0.1f;

    /// <summary>
    /// DungeonGenerator から呼び出してミニマップを描画するための関数。
    /// </summary>
    public void DrawMiniMap(TileType[,] map)
    {
        if (miniMapTilemap == null || miniMapFloorTile == null)
        {
            Debug.LogError("MiniMapRenderer：Tilemapかタイルが設定されていません。");
            return;
        }

        // ミニマップ初期化
        miniMapTilemap.ClearAllTiles();

        int width = map.GetLength(0);
        int height = map.GetLength(1);

        // 1マスのサイズを小さくする（ズームアウト）
        miniMapTilemap.transform.localScale = new Vector3(tileScale, tileScale, 1);

        // 床のみ描画
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == TileType.Floor)
                {
                    miniMapTilemap.SetTile(new Vector3Int(x, y, 0), miniMapFloorTile);
                }
            }
        }
    }
}
