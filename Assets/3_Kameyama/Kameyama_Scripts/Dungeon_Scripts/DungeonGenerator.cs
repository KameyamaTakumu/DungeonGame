using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// ダンジョン生成および移動判定に使用するタイル種別
/// map[x, y] の状態を表し、
/// ・通行可能か
/// ・描画するタイルは何か
/// などの判定に利用される
/// </summary>
public enum TileType
{
    /// <summary>
    /// 壁タイル
    /// ・通行不可
    /// ・Tilemap では wallTile として描画される
    /// </summary>
    Wall,
    /// <summary>
    /// 床タイル
    /// ・通行可能
    /// ・プレイヤー／敵のスポーン位置として利用される
    /// ・Tilemap では floorTile として描画される
    /// </summary>
    Floor,
    /// <summary>
    /// 下り階段
    /// ・通行可能
    /// ・次のフロアへの移動地点として利用される
    /// ・Tilemap では floorTile として描画される
    StepsDown
}

/// <summary>
/// ダンジョン生成を担当するクラス
/// ・ランダムな部屋の生成
/// ・部屋間を結ぶ通路の作成
/// ・Tilemap への描画
/// ・ミニマップ描画やプレイヤー／敵の自動配置
/// ・壁タイルの自動タイル適用
/// など、マップ生成に必要な一連の処理を統括します
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    // インスタンス
    public static DungeonGenerator instance;

    // ================================
    // 生成パラメータ
    // ================================

    [Header("マップサイズ")]
    // ここは public にして外部から参照できるようにする
    [CustomLabel("横幅（タイル数）"), SerializeField]
    public int width = 64;
    [CustomLabel("縦幅（タイル数）"), SerializeField]
    public int height = 64;

    [Header("部屋パラメータ")]
    [CustomLabel("生成する部屋の最大数"), SerializeField]
    private int roomCount = 8;
    [CustomLabel("部屋の最小サイズ"), SerializeField]
    private int roomMinSize = 4;
    [CustomLabel("部屋の最大サイズ"), SerializeField]
    private int roomMaxSize = 10;

    [Header("階段設定")]
    [CustomLabel("このマップに下り階段を生成する")]
    [SerializeField]
    private bool generateStepsDown = true;

    [Header("タイルマップ関連")]
    [CustomLabel("床タイルを描画するタイルマップ"), SerializeField]
    private Tilemap floorTilemap;
    [CustomLabel("壁タイルを描画するタイルマップ"), SerializeField]
    private Tilemap wallTilemap;

    [Header("タイル関連")]
    [CustomLabel("床として使用するタイルチップ"), SerializeField]
    private TileBase floorTile;
    [CustomLabel("壁として使用するタイルチップ"), SerializeField]
    private TileBase wallTile;
    [CustomLabel("下り階段として使用するタイルチップ"), SerializeField]
    private TileBase stepsDownTile;

    [Header("ミニマップ")]
    [CustomLabel("ミニマップ描画コンポーネント"), SerializeField]
    private MiniMapRenderer miniMapRenderer;

    [Header("デバッグ用")]
    [CustomLabel("現在のフロア番号"), SerializeField]
    public static int CurrentFloor = 1;

    // マップデータ（TileType の 2 次元配列）
    // ここを基準に Tilemap やミニマップ描画を行う
    public TileType[,] map;

    /// <summary>
    /// 内部で使用する部屋情報クラス
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
        instance = this;

        Generate();

        SoundManager.Instance.PlayBGM(BGM.Dungeon);
    }

    private void Update()
    {
        // デバッグ用: Rキーで再生成
        if (Input.GetKeyDown(KeyCode.R))
        {
            CurrentFloor = 1;
        }
    }

    /// <summary>
    /// ダンジョン生成のメイン処理
    /// ・初期化
    /// ・部屋生成
    /// ・通路生成
    /// ・Tilemap 反映
    /// ・ミニマップ描画
    /// ・プレイヤー／敵のスポーン
    /// ・壁タイルの自動タイル適用
    /// </summary>
    private void Generate()
    {
        // ボス階層なら専用生成
        if (CurrentFloor == 3)
        {
            GenerateBossFloor();
            return;
        }

        // 2Dマップ配列を全て「壁」で初期化
        map = new TileType[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = TileType.Wall;
            }
        }

        // 生成済み部屋リストをクリア
        rooms.Clear();

        // (1) ランダムに部屋を生成
        CreateRooms();

        // (2) 部屋同士を通路で繋ぐ
        ConnectRooms();

        // (2.5) 壁幅1のストライプを自動除去
        RemoveThinWalls();

        // (3) Tilemap へ反映
        RenderMap();

        // (4) ミニマップ描画
        if (miniMapRenderer != null)
        {
            miniMapRenderer.DrawMiniMap(map);
        }

        // (5) プレイヤー
        FindAnyObjectByType<PlayerSpawner>()?.SpawnPlayer(GetRandomFloorPosition());

        // (6) ミニマップの敵アイコン更新
        miniMapRenderer.ForceRefreshEnemies();

        // (7) 壁タイルの自動タイル適用
        FindAnyObjectByType<WallAutoTilePainter>()?.ApplyAutoTiles(map);

        FindAnyObjectByType<FloorAutoTilePainter>()?.Apply(map);

        // (8) 階段設置（設定次第）
        if (generateStepsDown)
        {
            PlaceStepDown();
        }
    }

    /// <summary>
    /// ボス専用フロアを生成する。
    /// 12×12 の1部屋構成
    /// 通路なし
    /// 雑魚敵なし
    /// </summary>
    private void GenerateBossFloor()
    {
        width = 18;
        height = 10;

        map = new TileType[width, height];

        // 全体を壁で初期化
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = TileType.Wall;
            }
        }

        // 中央に1部屋を作成
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                map[x, y] = TileType.Floor;
            }
        }

        RenderMap();

        // プレイヤーを左下寄りに配置
        FindAnyObjectByType<PlayerSpawner>()
            ?.SpawnPlayer(new Vector2Int(3, 3));

        // 壁オートタイル
        FindAnyObjectByType<WallAutoTilePainter>()
            ?.ApplyAutoTiles(map);

        // 床オートタイル
        FindAnyObjectByType<FloorAutoTilePainter>()
            ?.Apply(map);
    }


    /// <summary>
    /// ランダムな部屋を複数生成し、map を床タイルに置き換える
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
                // 外周 1 マス余裕をつけて判定する
                RectInt expanded = new RectInt(r.x - 1, r.y - 1, r.w + 2, r.h + 2);

                if (expanded.Overlaps(rNew))
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
    /// 生成した部屋を通路で繋ぐ
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

    /// <summary>
    /// 2点間を通路（Floor）で繋ぐ
    /// X → Y の順で L 字型に掘るシンプルな実装
    /// </summary>
    /// <param name="x1">開始点 X 座標</param>
    /// <param name="y1">開始点 Y 座標</param>
    /// <param name="x2">終了点 X 座標</param>
    /// <param name="y2">終了点 Y 座標</param>
    private void DigCorridor(int x1, int y1, int x2, int y2)
    {
        // 開始地点
        int x = x1; int y = y1;

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
    /// 壁幅1の薄い壁を除去する
    /// </summary>
    private void RemoveThinWalls()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (map[x, y] != TileType.Wall) continue;

                // ---- 横方向の薄い壁（幅1） ----
                bool thinHorizontal =
                    map[x - 1, y] == TileType.Floor &&
                    map[x + 1, y] == TileType.Floor;

                // ---- 縦方向の薄い壁（幅1） ----
                bool thinVertical =
                    map[x, y - 1] == TileType.Floor &&
                    map[x, y + 1] == TileType.Floor;

                if (thinHorizontal || thinVertical)
                {
                    map[x, y] = TileType.Floor; // 壁を潰す
                }
            }
        }
    }

    /// <summary>
    /// 生成された部屋のどこかに下り階段を配置する
    /// </summary>
    private void PlaceStepDown()
    {
        if (rooms.Count == 0)
        {
            Debug.LogWarning("部屋が無いため階段を配置できません。");
            return;
        }

        // ▼ランダムな部屋に階段を置く
        Room lastRoom = rooms[Random.Range(0, rooms.Count)];
        //Room lastRoom = rooms[rooms.Count - 1];

        // 部屋の中のランダムな床座標
        int x = Random.Range(lastRoom.x + 1, lastRoom.x + lastRoom.w - 1);
        int y = Random.Range(lastRoom.y + 1, lastRoom.y + lastRoom.h - 1);

        // map データを階段タイルに変更
        map[x, y] = TileType.StepsDown;

        // Tilemap 描画
        floorTilemap.SetTile(new Vector3Int(x, y, 0), stepsDownTile);

        //Debug.Log($"下り階段を配置: {x},{y}");

        // ▼トリガー用オブジェクトを生成
        GameObject trigger = new GameObject("StepsDownTrigger");
        trigger.transform.position = new Vector3(x, y, 0); // 中心合わせ

        BoxCollider2D col = trigger.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        // ▼判定用のスクリプトを追加
        trigger.AddComponent<StepsDownTrigger>();
    }

    /// <summary>
    /// map 配列の内容を Tilemap に反映させる
    /// 床タイルと壁タイルをそれぞれの Tilemap へ配置する
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
                else if (map[x, y] == TileType.StepsDown)
                {
                    floorTilemap.SetTile(pos, stepsDownTile);
                }
                else
                {
                    wallTilemap.SetTile(pos, wallTile);
                }
            }
        }
    }

    /// <summary>
    /// ランダムな「床」タイルの座標を取得する
    /// プレイヤーや敵のスポーンなどに使用
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

    public void ResetFloorNumber()
    {
        CurrentFloor = 1;
    }
}
