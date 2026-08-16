using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// ダンジョン内でのプレイヤー視界および探索状態を
/// Texture2D ベースでミニマップとして描画するコンポーネント
/// </summary>
public class MiniMapRenderer : MonoBehaviour
{
    // ======================
    // UI 参照
    // ======================

    [Header("UI")]
    [CustomLabel("ミニマップ表示に使用する RawImage"), SerializeField]
    private RawImage minimapImage;   // 生成した Texture2D を割り当てる
    [CustomLabel("ミニマップの RectTransform"), SerializeField]
    private RectTransform minimapRect;    // 位置変換に使用する
    [CustomLabel("プレイヤー位置を示す UI アイコン"), SerializeField]
    private RectTransform playerIcon;

    // ======================
    // 敵アイコン
    // ======================

    [Header("敵アイコン")]
    [CustomLabel("敵の位置を示す UI アイコン"), SerializeField]
    private RectTransform enemyIconPrefab;
    // 敵ごとのアイコンインスタンスを管理
    private List<RectTransform> enemyIcons = new List<RectTransform>();
    // 現在の敵オブジェクト参照リスト
    private List<GameObject> enemies = new List<GameObject>();

    // ======================
    // 階段アイコン
    // ======================

    [Header("階段アイコン")]
    [CustomLabel("下り階段を示す UI アイコン"), SerializeField]
    private RectTransform stepsDownIconPrefab;

    private List<RectTransform> stepsDownIcons = new List<RectTransform>();
    private List<Vector2Int> stepsDownTiles = new List<Vector2Int>();

    // ======================
    // 描画設定
    // ======================

    [Header("見た目")]
    [CustomLabel("1タイルを何ピクセルで描画するか"), Tooltip("値が大きいほど粗くなる。"), SerializeField]
    private int pixelScale = 4;

    [Header("タイル色設定")]
    [CustomLabel("床タイルの表示色"), SerializeField]
    private Color floorColor = new Color(0.85f, 0.85f, 0.85f);
    [CustomLabel("探索済みだが現在視界外のタイルの色"), SerializeField]
    private Color discoveredTint = new Color(0.35f, 0.35f, 0.35f);

    // 未探索は完全透明
    private readonly Color clearColor = new Color(0f, 0f, 0f, 0f);

    // ======================
    // 視界設定
    // ======================

    [Header("視界")]
    [CustomLabel("プレイヤーの視界半径（円形）"), SerializeField]
    private int viewRadius = 8;

    [Header("壁表示")]
    [SerializeField]
    private Color wallLineColor = Color.black;

    // ======================
    // 内部状態
    // ======================

    private TileType[,] map;
    private int mapW, mapH;
    private Texture2D tex;

    // discovered[x,y] = 一度でも視界に入ったか
    private bool[,] discovered;
    // visibleNow[x,y] = 現在視界に入っているか
    private bool[,] visibleNow;

    // 最後に記録したプレイヤー位置
    private Vector2Int lastPlayerTile = new Vector2Int(int.MinValue, int.MinValue);

    // Player オブジェクト参照
    private GameObject player;

    private bool enemyInitialized = false;

    // デバッグ用ギズモ表示トグル（OnDrawGizmos を直接呼ぶのを避ける）
    private bool debugGizmosToggle = false;

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }

    void Update()
    {
        // 初期化前は処理しない
        if (map == null || tex == null) return;

        if (player == null) return;

        if (!enemyInitialized)
        {
            var found = GameObject.FindGameObjectsWithTag("Enemy");
            if (found.Length > 0)
            {
                ForceRefreshEnemies();
                enemyInitialized = true;
            }
        }

        // プレイヤー位置の変化を検知
        Vector2Int playerTile = WorldToTile(player.transform.position);

        // 位置が変わったら視界を更新
        if (playerTile != lastPlayerTile)
        {
            lastPlayerTile = playerTile;
            ForceRecalculateFOVAndDraw();
        }

        // UI アイコン座標更新
        if (playerIcon != null)
        {
            UpdatePlayerIconPosition(playerTile);
        }

        // 敵アイコン更新
        UpdateEnemyIcons();

        UpdateStepsDownIcons();

#if UNITY_EDITOR
        // デバッグ用のミニマップ全開放関数
        if (Input.GetKeyDown(KeyCode.M))
        {
            AllViewMiniMap();
        }

        // デバッグ用視界範囲のギズモ表示トグル
        if (Input.GetKeyDown(KeyCode.N))
        {
            debugGizmosToggle = !debugGizmosToggle;
        }
#endif
    }

    /// <summary>
    /// ミニマップの初期化処理。
    /// DungeonGenerator からマップ配列が渡され、
    /// マップサイズに応じて Texture2D を生成する
    /// </summary>
    public void DrawMiniMap(TileType[,] sourceMap)
    {
        if (sourceMap == null) return;

        map = sourceMap;

        // マップサイズ取得
        mapW = map.GetLength(0);
        mapH = map.GetLength(1);

        // テクスチャサイズ計算
        int texW = Mathf.Max(1, mapW * pixelScale);
        int texH = Mathf.Max(1, mapH * pixelScale);

        // ミニマップ用テクスチャ生成
        tex = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        // RawImage に割り当て
        if (minimapImage != null)
        {
            minimapImage.texture = tex;
            minimapImage.color = Color.white;// 透明処理が正しく機能するため必須
        }

        // RectTransform 参照が未設定なら RawImage から取得
        if (minimapRect == null && minimapImage != null)
        {
            minimapRect = minimapImage.rectTransform;
        }

        // 発見済み配列の初期作成
        discovered = new bool[mapW, mapH];
        visibleNow = new bool[mapW, mapH];

        // 先に階段座標を取得してアイコン生成
        CollectStepsDownTiles();
        InitStepsDownIcons();

        // 未探索は完全透明の状態で初期化
        ClearTextureTransparent();

        player = GameObject.FindGameObjectWithTag("Player");

        // 視界更新
        ForceRecalculateFOVAndDraw();

        // 敵アイコン
        ForceRefreshEnemies();
    }

    private void UpdateStepsDownIcons()
    {
        for (int i = 0; i < stepsDownTiles.Count; i++)
        {
            var tile = stepsDownTiles[i];
            var icon = stepsDownIcons[i];

            // まだ一度も発見していなければ非表示
            if (!discovered[tile.x, tile.y])
            {
                icon.gameObject.SetActive(false);
                continue;
            }

            // 発見済みなら常に表示
            icon.gameObject.SetActive(true);

            // ミニマップ座標へ変換
            float u = (tile.x + 0.5f) / mapW;
            float v = (tile.y + 0.5f) / mapH;

            Vector2 size = minimapRect.sizeDelta;
            float px = (u - 0.5f) * size.x;
            float py = (v - 0.5f) * size.y;

            icon.anchoredPosition = new Vector2(px, py);
        }
    }

    /// <summary>
    /// 敵タグを再取得し、アイコンを再生成する
    /// 敵の再ポップ時にも使用可能
    /// </summary>
    public void ForceRefreshEnemies()
    {
        enemies.Clear();
        var found = GameObject.FindGameObjectsWithTag("Enemy");

        enemies.AddRange(found);
        InitEnemyIcons();
    }

    /// <summary>
    /// map から StepsDown タイル座標を収集
    /// </summary>
    private void CollectStepsDownTiles()
    {
        stepsDownTiles.Clear();

        for (int x = 0; x < mapW; x++)
        {
            for (int y = 0; y < mapH; y++)
            {
                if (map[x, y] == TileType.StepsDown)
                {
                    stepsDownTiles.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    private void InitStepsDownIcons()
    {
        foreach (var icon in stepsDownIcons)
            if (icon != null) Destroy(icon.gameObject);

        stepsDownIcons.Clear();

        if (stepsDownIconPrefab == null || minimapRect == null)
        {
            if (stepsDownIconPrefab == null) Debug.LogWarning("stepsDownIconPrefab is not assigned.");
            if (minimapRect == null) Debug.LogWarning("minimapRect is not assigned.");
            return;
        }

        foreach (var pos in stepsDownTiles)
        {
            var icon = Instantiate(stepsDownIconPrefab, minimapRect);
            icon.gameObject.SetActive(true);
            stepsDownIcons.Add(icon);
        }
    }

    /// <summary>
    /// 外部から強制的にミニマップを再描画したいとき用
    /// </summary>
    public void ForceRedraw() => ForceRecalculateFOVAndDraw();

    /// <summary>
    /// ワールド座標（整数グリッド）をタイル座標に変換
    /// 範囲外に出ないよう Clamp
    /// </summary>
    /// <param name="worldPos">ワールド座標</param>
    private Vector2Int WorldToTile(Vector2 worldPos)
    {
        // 整数化
        int tx = Mathf.RoundToInt(worldPos.x);
        int ty = Mathf.RoundToInt(worldPos.y);

        // 範囲外に出ないよう Clamp
        tx = Mathf.Clamp(tx, 0, mapW - 1);
        ty = Mathf.Clamp(ty, 0, mapH - 1);

        return new Vector2Int(tx, ty);
    }

    /// <summary>
    /// 壁かどうか（範囲外は壁扱い）
    /// </summary>
    private bool IsWall(int x, int y)
    {
        if (x < 0 || y < 0 || x >= mapW || y >= mapH)
            return true;

        return map[x, y] == TileType.Wall;
    }

    /// <summary>
    /// プレイヤー位置を基準に FOV を再計算し、
    /// ミニマップ全体を再描画する
    /// </summary>
    private void ForceRecalculateFOVAndDraw()
    {
        if (player == null) return;

        Vector2Int p = WorldToTile(player.transform.position);

        // 現在視認中の情報をリセット
        for (int x = 0; x < mapW; x++)
        {
            for (int y = 0; y < mapH; y++)
            {
                visibleNow[x, y] = false;
            }
        }

        // 視界範囲の矩形領域
        int minX = Mathf.Max(0, p.x - viewRadius);
        int maxX = Mathf.Min(mapW - 1, p.x + viewRadius);
        int minY = Mathf.Max(0, p.y - viewRadius);
        int maxY = Mathf.Min(mapH - 1, p.y + viewRadius);

        // 円形視界 + LOS（Line of Sight）
        for (int tx = minX; tx <= maxX; tx++)
        {
            for (int ty = minY; ty <= maxY; ty++)
            {
                int dx = tx - p.x;
                int dy = ty - p.y;

                if (dx * dx + dy * dy > viewRadius * viewRadius) continue;

                if (HasLineOfSight(p.x, p.y, tx, ty))
                {
                    visibleNow[tx, ty] = true;
                    discovered[tx, ty] = true;
                }
            }
        }

        // =============================
        // 描画処理（SetPixels バッファを使用）
        // =============================

        if (tex == null) return;

        int texW = tex.width;
        int texH = tex.height;
        Color[] pixels = new Color[texW * texH];

        // 初期化（全ピクセルを透明に）
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clearColor;

        // 全タイルを走査してピクセルバッファに色をセット
        for (int x = 0; x < mapW; x++)
        {
            for (int y = 0; y < mapH; y++)
            {
                Color c;

                if (!discovered[x, y])
                {
                    c = clearColor;
                }
                else
                {
                    c = floorColor;
                    if (!visibleNow[x, y])
                        c = Color.Lerp(c, discoveredTint, 0.5f);
                }

                int baseX = x * pixelScale;
                int baseY = y * pixelScale;

                for (int px = 0; px < pixelScale; px++)
                {
                    for (int py = 0; py < pixelScale; py++)
                    {
                        int ix = baseX + px;
                        int iy = baseY + py;
                        if (ix < 0 || iy < 0 || ix >= texW || iy >= texH) continue;
                        pixels[iy * texW + ix] = c;
                    }
                }
            }
        }

        // 壁アウトライン描画（ピクセルバッファへ直接）
        for (int x = 0; x < mapW; x++)
        {
            for (int y = 0; y < mapH; y++)
            {
                if (!discovered[x, y]) continue;
                if (map[x, y] == TileType.Wall) continue;

                bool N = IsWall(x, y + 1);
                bool S = IsWall(x, y - 1);
                bool E = IsWall(x + 1, y);
                bool W = IsWall(x - 1, y);

                int baseX = x * pixelScale;
                int baseY = y * pixelScale;

                if (N)
                {
                    int py = baseY + pixelScale - 1;
                    for (int px = 0; px < pixelScale; px++)
                    {
                        int ix = baseX + px;
                        if (ix < 0 || py < 0 || ix >= texW || py >= texH) continue;
                        pixels[py * texW + ix] = wallLineColor;
                    }
                }

                if (S)
                {
                    int py = baseY;
                    for (int px = 0; px < pixelScale; px++)
                    {
                        int ix = baseX + px;
                        if (ix < 0 || py < 0 || ix >= texW || py >= texH) continue;
                        pixels[py * texW + ix] = wallLineColor;
                    }
                }

                if (E)
                {
                    int ix = baseX + pixelScale - 1;
                    for (int py = 0; py < pixelScale; py++)
                    {
                        int iy = baseY + py;
                        if (ix < 0 || iy < 0 || ix >= texW || iy >= texH) continue;
                        pixels[iy * texW + ix] = wallLineColor;
                    }
                }

                if (W)
                {
                    int ix = baseX;
                    for (int py = 0; py < pixelScale; py++)
                    {
                        int iy = baseY + py;
                        if (ix < 0 || iy < 0 || ix >= texW || iy >= texH) continue;
                        pixels[iy * texW + ix] = wallLineColor;
                    }
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
    }

    /// <summary>
    /// プレイヤーアイコンの UI 上の座標を更新
    /// ミニマップのサイズを基準に算出
    /// </summary>
    /// <param name="t">タイル座標</param>
    private void UpdatePlayerIconPosition(Vector2Int t)
    {
        if (minimapRect == null || playerIcon == null) return;

        float u = (t.x + 0.5f) / mapW;
        float v = (t.y + 0.5f) / mapH;

        Vector2 size = minimapRect.sizeDelta;
        float px = (u - 0.5f) * size.x;
        float py = (v - 0.5f) * size.y;

        playerIcon.anchoredPosition = new Vector2(px, py);
    }

    // -------------------------------------------------------------------
    // 敵管理
    // -------------------------------------------------------------------

    /// <summary>
    /// 敵アイコンを再生成（既存アイコンは破棄）
    /// 敵数の変化に対応
    /// </summary>
    private void InitEnemyIcons()
    {
        foreach (var icon in enemyIcons)
            if (icon != null) Destroy(icon.gameObject);

        enemyIcons.Clear();

        if (enemyIconPrefab == null || minimapRect == null)
        {
            if (enemyIconPrefab == null) Debug.LogWarning("enemyIconPrefab is not assigned.");
            if (minimapRect == null) Debug.LogWarning("minimapRect is not assigned.");
            return;
        }

        foreach (var enemy in enemies)
        {
            var icon = Instantiate(enemyIconPrefab, minimapRect);
            icon.gameObject.SetActive(true);
            enemyIcons.Add(icon);
        }
    }

    /// <summary>
    /// 敵アイコンの位置をミニマップ上に反映。
    /// 視界内の敵のみ表示し、視界外は非表示とする。
    /// </summary>
    private void UpdateEnemyIcons()
    {
        // 敵数とアイコン数が違えば作り直す（重いので1度だけ再生成)
        if (enemyIcons.Count != enemies.Count)
        {
            InitEnemyIcons();
            return; // 次フレームで整合後に位置更新する
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            var icon = enemyIcons[i];

            if (enemy == null)
            {
                icon.gameObject.SetActive(false);
                continue;
            }

            Vector2Int tile = WorldToTile(enemy.transform.position);

            if (!visibleNow[tile.x, tile.y])
            {
                icon.gameObject.SetActive(false);
                continue;
            }

            icon.gameObject.SetActive(true);

            float u = (tile.x + 0.5f) / mapW;
            float v = (tile.y + 0.5f) / mapH;

            Vector2 size = minimapRect.sizeDelta;
            float px = (u - 0.5f) * size.x;
            float py = (v - 0.5f) * size.y;

            icon.anchoredPosition = new Vector2(px, py);
        }
    }

    public void SetEnemies(IReadOnlyList<GameObject> list)
    {
        enemies.Clear();
        enemies.AddRange(list);
        InitEnemyIcons();
    }

    // デバッグ用にミニマップを全開放する処理
    private void AllViewMiniMap()
    {
        for (int x = 0; x < mapW; x++)
        {
            for (int y = 0; y < mapH; y++)
            {
                if (map[x, y] != TileType.Wall)
                {
                    discovered[x, y] = true;
                    visibleNow[x, y] = true;
                }
            }
        }

        ForceRedraw();
    }

    // ギズモ表示（Unity によって呼ばれる）
    private void OnDrawGizmos()
    {
        if (!debugGizmosToggle) return;
        if (player == null) return;
        Vector2Int p = WorldToTile(player.transform.position);
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawWireSphere(new Vector3(p.x, p.y, 0), viewRadius);
    }

    /// <summary>
    /// テクスチャ全体を透明クリアで初期化（SetPixels 化）
    /// </summary>
    private void ClearTextureTransparent()
    {
        if (tex == null) return;
        Color[] pixels = new Color[tex.width * tex.height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clearColor;
        tex.SetPixels(pixels);
        tex.Apply();
    }

    /// <summary>
    /// Bresenham のアルゴリズムを HasLineOfSight 内で直接実行（アロケーションゼロ）
    /// </summary>
    private bool HasLineOfSight(int x0, int y0, int x1, int y1)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        int x = x0, y = y0;

        while (true)
        {
            if (!(x == x0 && y == y0))
            {
                if (x < 0 || y < 0 || x >= mapW || y >= mapH) return false;
                if (map[x, y] == TileType.Wall) return false;
            }

            if (x == x1 && y == y1) break;

            int e2 = err * 2;
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 < dx) { err += dx; y += sy; }
        }

        return true;
    }
}
