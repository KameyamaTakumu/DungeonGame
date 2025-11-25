using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// ダンジョン生成および移動判定に使用するタイル種別.
/// map[x, y] の状態を表し、
/// ・通行可能か
/// ・描画するタイルは何か
/// などの判定に利用される。
/// </summary>
public enum TileType
{
    /// <summary>
    /// 壁タイル。
    /// ・通行不可
    /// ・Tilemap では wallTile として描画される
    /// </summary>
    Wall,
    /// <summary>
    /// 床タイル。
    /// ・通行可能
    /// ・プレイヤー／敵のスポーン位置として利用される
    /// ・Tilemap では floorTile として描画される
    /// </summary>
    Floor
}

/// <summary>
/// ダンジョン生成を担当するクラス。
/// ・ランダムな部屋の生成
/// ・部屋間を結ぶ通路の作成
/// ・Tilemap への描画
/// ・ミニマップ描画やプレイヤー／敵の自動配置
/// など、マップ生成に必要な一連の処理を統括します。
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    // ================================
    // 生成パラメータ
    // ================================

    [Header("マップサイズ")]
    [CustomLabel("横幅（タイル数）")]
    public int width = 64;
    [CustomLabel("縦幅（タイル数）")]
    public int height = 64;

    [Header("部屋パラメータ")]
    [CustomLabel("生成する部屋の最大数")]
    public int roomCount = 8;
    [CustomLabel("部屋の最小サイズ")]
    public int roomMinSize = 4;
    [CustomLabel("部屋の最大サイズ")]
    public int roomMaxSize = 10;

    [Header("Tilemap関連")]
    [CustomLabel("床タイルを描画する Tilemap")]
    public Tilemap floorTilemap;
    [CustomLabel("壁タイルを描画する Tilemap")]
    public Tilemap wallTilemap;

    [CustomLabel("床として使用する Tile")]
    public TileBase floorTile;
    [CustomLabel("壁として使用する Tile")]
    public TileBase wallTile;

    [Header("ミニマップ")]
    [CustomLabel("ミニマップ描画コンポーネント")]
    [SerializeField] 
    private MiniMapRenderer miniMapRenderer;

    // マップデータ（TileType の 2 次元配列）
    // ここを基準に Tilemap やミニマップ描画を行う
    public TileType[,] map;

    /// <summary>
    /// 内部で使用する部屋情報クラス。
    /// ・位置 (x, y)
    /// ・幅 w、高さ h
    /// ・中心点計算
    /// </summary>
    private class Room
    {
        public int x, y, w, h;

        public int CenterX => x + w / 2;
        public int CenterY => y + h / 2;

        public RectInt ToRect() => new RectInt(x, y, w, h);
    }

    // 生成した部屋の一覧
    private List<Room> rooms = new List<Room>();

    void Awake()
    {
        Generate();
    }

    /// <summary>
    /// ダンジョン生成のメイン処理。
    /// ・初期化
    /// ・部屋生成
    /// ・通路生成
    /// ・Tilemap 反映
    /// ・ミニマップ描画
    /// ・プレイヤー／敵のスポーン
    /// </summary>
    private void Generate()
    {
        // 2Dマップ配列を全て「壁」で初期化
        map = new TileType[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = TileType.Wall;

        rooms.Clear();

        // (1) ランダムに部屋を生成
        CreateRooms();

        // (2) 部屋同士を通路で繋ぐ
        ConnectRooms();

        // (3) Tilemap へ反映
        RenderMap();

        // (4) ミニマップ描画
        if (miniMapRenderer != null)
            miniMapRenderer.DrawMiniMap(map);

        // (5) プレイヤー / 敵の自動スポーン（デバッグ用途）
        FindAnyObjectByType<PlayerSpawner>()?.SpawnPlayer(GetRandomFloorPosition());
        FindAnyObjectByType<EnemySpawner>()?.SpawnEnemy(GetRandomFloorPosition());

        // (6) ミニマップの敵アイコン更新
        miniMapRenderer.ForceRefreshEnemies();
    }

    /// <summary>
    /// ランダムな部屋を複数生成し、map を床タイルに置き換える。
    /// ・他の部屋と重なっていないか判定
    /// ・部屋の領域を Floor に変更
    /// </summary>
    private void CreateRooms()
    {
        int attempts = 0;

        // 部屋数が揃うか、試行回数が上限に達するまで繰り返す
        while (rooms.Count < roomCount && attempts < roomCount * 10)
        {
            attempts++;

            // ランダムサイズの部屋を生成
            int w = Random.Range(roomMinSize, roomMaxSize + 1);
            int h = Random.Range(roomMinSize, roomMaxSize + 1);
            int x = Random.Range(1, width - w - 1);
            int y = Random.Range(1, height - h - 1);

            Room newRoom = new Room() { x = x, y = y, w = w, h = h };
            RectInt rNew = newRoom.ToRect();

            // 既存の部屋と重なっていないか確認
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

            // 部屋の登録
            rooms.Add(newRoom);

            // map 配列を床に変更
            for (int ix = x; ix < x + w; ix++)
            {
                for (int iy = y; iy < y + h; iy++)
                {
                    map[ix, iy] = TileType.Floor;
                }
            }
        }
    }

    /// <summary>
    /// 生成した部屋を通路で繋ぐ。
    /// ・部屋中心を X 座標でソートし、左から順に接続
    /// ・L 字通路を掘る
    /// </summary>
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

    // <summary>
    /// 2点間を通路（Floor）で繋ぐ。
    /// X → Y の順で L 字型に掘るシンプルな実装。
    /// </summary>
    private void DigCorridor(int x1, int y1, int x2, int y2)
    {
        int x = x1;
        int y = y1;

        // X方向に掘る
        while (x != x2)
        {
            map[x, y] = TileType.Floor;
            x += (x2 > x) ? 1 : -1;
        }

        // Y方向に掘る
        while (y != y2)
        {
            map[x, y] = TileType.Floor;
            y += (y2 > y) ? 1 : -1;
        }

        // 最終地点も床に
        map[x, y] = TileType.Floor;
    }

    /// <summary>
    /// map 配列の内容を Tilemap に反映させる。
    /// 床タイルと壁タイルをそれぞれの Tilemap へ配置する。
    /// </summary>
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
    /// ランダムな「床」タイルの座標を取得する。
    /// プレイヤーや敵のスポーンなどに使用。
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
            Debug.LogError("床タイルがありません。マップ生成に失敗しています。");
            return Vector2Int.zero;
        }

        return floors[Random.Range(0, floors.Count)];
    }
}
