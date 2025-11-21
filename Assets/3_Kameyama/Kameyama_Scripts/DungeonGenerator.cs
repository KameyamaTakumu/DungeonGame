using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

// タイル種別
public enum TileType
{
    Wall,
    Floor
}

public class DungeonGenerator : MonoBehaviour
{
    [Header("マップサイズ")]
    public int width = 64;
    public int height = 64;

    [Header("部屋パラメータ")]
    public int roomCount = 8;         // 最大部屋数
    public int roomMinSize = 4;
    public int roomMaxSize = 10;

    [Header("Tilemap関連")]
    public Tilemap floorTilemap;   // 床
    public Tilemap wallTilemap;    // 壁

    public TileBase floorTile;
    public TileBase wallTile;

    [Header("ミニマップ")]
    [SerializeField] private MiniMapRenderer miniMapRenderer;

    // 内部データ
    public TileType[,] map;

    // 部屋情報クラス
    private class Room
    {
        public int x, y, w, h;

        public int CenterX => x + w / 2;
        public int CenterY => y + h / 2;

        public RectInt ToRect() => new RectInt(x, y, w, h);
    }

    private List<Room> rooms = new List<Room>();

    void Awake()
    {
        Generate();
    }

    public void Generate()
    {
        // 2次元配列初期化（全部壁）
        map = new TileType[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = TileType.Wall;

        rooms.Clear();

        // (1) 部屋をランダムに生成
        CreateRooms();

        // (2) 部屋同士を通路で接続
        ConnectRooms();

        // (3) Tilemap に配置
        RenderMap();

        // (4)ミニマップ描画
        if (miniMapRenderer != null)
            miniMapRenderer.DrawMiniMap(map);

        // デバッグ用（ここでプレイヤーをスポーン）
        FindAnyObjectByType<Test_PlayerSpawner>()?.SpawnPlayer(GetRandomFloorPosition());
        FindAnyObjectByType<Test_PlayerSpawner>()?.SpawnEnemy(GetRandomFloorPosition());
    }

    private void CreateRooms()
    {
        int attempts = 0;

        while (rooms.Count < roomCount && attempts < roomCount * 10)
        {
            attempts++;

            int w = Random.Range(roomMinSize, roomMaxSize + 1);
            int h = Random.Range(roomMinSize, roomMaxSize + 1);
            int x = Random.Range(1, width - w - 1);
            int y = Random.Range(1, height - h - 1);

            Room newRoom = new Room() { x = x, y = y, w = w, h = h };
            RectInt rNew = newRoom.ToRect();

            // 他の部屋と重ならないかチェック
            bool overlap = false;
            foreach (var r in rooms)
            {
                if (rNew.Overlaps(r.ToRect()))
                {
                    overlap = true;
                    break;
                }
            }
            if (overlap) continue;

            rooms.Add(newRoom);

            // 初期化 → 床にする
            for (int ix = x; ix < x + w; ix++)
            {
                for (int iy = y; iy < y + h; iy++)
                {
                    map[ix, iy] = TileType.Floor;
                }
            }
        }
    }

    private void ConnectRooms()
    {
        // 部屋の中心点を X 座標でソート
        rooms.Sort((a, b) => a.CenterX.CompareTo(b.CenterX));

        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Room r1 = rooms[i];
            Room r2 = rooms[i + 1];

            // 中心点
            int x1 = r1.CenterX;
            int y1 = r1.CenterY;
            int x2 = r2.CenterX;
            int y2 = r2.CenterY;

            // L字通路（先に X → 次に Y を掘る）
            DigCorridor(x1, y1, x2, y1);
            DigCorridor(x2, y1, x2, y2);
        }
    }

    private void DigCorridor(int x1, int y1, int x2, int y2)
    {
        int x = x1;
        int y = y1;

        while (x != x2)
        {
            map[x, y] = TileType.Floor;
            x += (x2 > x) ? 1 : -1;
        }
        while (y != y2)
        {
            map[x, y] = TileType.Floor;
            y += (y2 > y) ? 1 : -1;
        }
        map[x, y] = TileType.Floor;
    }

    private void RenderMap()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (map[x, y] == TileType.Floor)
                {
                    floorTilemap.SetTile(pos, floorTile);
                }
                else
                {
                    wallTilemap.SetTile(pos, wallTile);
                }
            }
        }
    }

    /// <summary>
    /// ランダムな床タイルの位置を返す
    /// </summary>
    public Vector2Int GetRandomFloorPosition()
    {
        List<Vector2Int> floors = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == TileType.Floor)
                {
                    floors.Add(new Vector2Int(x, y));
                }
            }
        }

        if (floors.Count == 0)
        {
            Debug.LogError("床がありません！");
            return Vector2Int.zero;
        }

        return floors[Random.Range(0, floors.Count)];
    }
}
