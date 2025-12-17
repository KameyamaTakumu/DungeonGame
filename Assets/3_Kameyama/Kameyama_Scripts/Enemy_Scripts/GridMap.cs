using UnityEngine;
using UnityEngine.Tilemaps;

public class GridMap : MonoBehaviour
{
    public static GridMap instance;

    [SerializeField] Tilemap wallTilemap; // 壁用タイルマップ

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 指定セルが通行可能か
    /// </summary>
    public bool IsWalkable(Vector2Int cell)
    {
        // 壁タイルがあれば通行不可
        return !wallTilemap.HasTile((Vector3Int)cell);
    }
}
