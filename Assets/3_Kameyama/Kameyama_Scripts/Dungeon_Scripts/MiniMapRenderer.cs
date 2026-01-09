using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// ダンジョン内でのプレイヤー視界および探索状態を
/// Texture2D ベースでミニマップとして描画するコンポーネント
///
/// 【主な機能】
/// - DungeonGenerator から DrawMiniMap(map) を受けて初期化
/// - プレイヤー位置の変化に応じて視界（FOV）を再計算
/// - 発見済み／未発見／現在視認中 の状態を色と透明度で表現
/// - 敵アイコンをミニマップ上に自動反映
///
/// Texture2D を使用することで、UI とは独立した柔軟なビジュアル制御を可能にしている
/// </summary>
public class MiniMapRenderer : MonoBehaviour
{
    // ======================
    // UI 参照
    // ======================

    [Header("UI")]
    [CustomLabel("ミニマップ表示に使用する RawImage"), SerializeField]
    private RawImage      minimapImage;   // 生成した Texture2D を割り当てる
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
    private List<GameObject>    enemies    = new List<GameObject>();

    // ======================
    // 描画設定
    // ======================

    [Header("見た目")]
    [CustomLabel("1タイルを何ピクセルで描画するか")]
    [Tooltip("値が大きいほど粗くなる。")]
    private int pixelScale = 4;

    [Header("タイル色設定")]
    [CustomLabel("床タイルの表示色")]
    private Color floorColor     = new Color(0.85f, 0.85f, 0.85f);
    [CustomLabel("探索済みだが現在視界外のタイルの色")]
    private Color discoveredTint = new Color(0.35f, 0.35f, 0.35f);

    // 未探索は完全透明
    private readonly Color clearColor = new Color(0f, 0f, 0f, 0f);

    // ======================
    // 視界設定
    // ======================

    [Header("視界")]
    [CustomLabel("プレイヤーの視界半径（円形）")]
    private int viewRadius = 8;

    [Header("壁表示")]
    [SerializeField]
    private Color wallLineColor = Color.black;

    // ======================
    // 内部状態
    // ======================

    private TileType[,] map;
    private int         mapW, mapH;
    private Texture2D   tex;

    // discovered[x,y] = 一度でも視界に入ったか
    private bool[,] discovered;
    // visibleNow[x,y] = 現在視界に入っているか
    private bool[,] visibleNow;

    // 最後に記録したプレイヤー位置（1タイル単位
    private Vector2Int lastPlayerTile = new Vector2Int(int.MinValue, int.MinValue);

    // Player オブジェクト参照
    private GameObject player;

    private bool enemyInitialized = false;

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
    }

    /// <summary>
    /// ミニマップの初期化処理。
    /// DungeonGenerator からマップ配列が渡され、
    /// マップサイズに応じて Texture2D を生成する
    /// </summary>
    public void DrawMiniMap(TileType[,] sourceMap)
    {
        if (sourceMap == null) return;

        map  = sourceMap;

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

        // 発見済み／視界情報配列初期化
        discovered = new bool[mapW, mapH];
        visibleNow = new bool[mapW, mapH];

        // 未探索は完全透明の状態で初期化
        ClearTextureTransparent();

        player = GameObject.FindGameObjectWithTag("Player");

        // 初回描画
        ForceRecalculateFOVAndDraw();

        // ★ これを追加
        ForceRefreshEnemies();
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
    /// Bresenham のアルゴリズムを用いて LOS 判定
    /// 壁に到達するまで視界が通るかをチェックする
    /// </summary>
    private bool HasLineOfSight(int x0, int y0, int x1, int y1)
    {
        // Bresenham の直線上の各タイルをチェック
        foreach (var pt in BresenhamLine(x0, y0, x1, y1))
        {
            // 始点は無視
            if (pt.x == x0 && pt.y == y0) continue;

            // 壁に到達したら視界が通らない
            if (map[pt.x, pt.y] == TileType.Wall) return false;
        }
        return true;
    }

    /// <summary>
    /// Bresenham の直線生成
    /// タイルベースで直線上の座標列を返す
    /// </summary>
    /// <returns>直線上のタイル座標列</returns>
    /// <param name="x0">始点X座標</param>
    /// <param name="y0">始点Y座標</param>
    /// <param name="x1">終点X座標</param>
    /// <param name="y1">終点Y座標</param>
    private IEnumerable<Vector2Int> BresenhamLine(int x0, int y0, int x1, int y1)
    {
        // 差分計算
        int dx  = Mathf.Abs(x1 - x0);
        int dy  = Mathf.Abs(y1 - y0);

        // 進行方向
        int sx  = x0 < x1 ? 1 : -1;
        int sy  = y0 < y1 ? 1 : -1;

        // 誤差初期値
        int err = dx - dy;

        int x = x0, y = y0;

        while (true)
        {
            // 座標を返す
            yield return new Vector2Int(x, y);

            // 終点に到達したら終了
            if (x == x1 && y == y1) break;

            // 誤差計算
            int e2 = 2 * err;
            // 誤差に応じて座標を進める
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 <  dx) { err += dx; y += sy; }
        }
    }

    /// <summary>
    /// Texture 全体を透明クリアで初期化
    /// 未探索タイルが黒く見えてしまう問題を防ぐ
    /// </summary>
    private void ClearTextureTransparent()
    {
        // 全ピクセル透明で初期化
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                tex.SetPixel(x, y, clearColor);
            }
        }

        tex.Apply();
    }

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

                // 円形の範囲外は除外
                if (dx * dx + dy * dy > viewRadius * viewRadius) continue;

                // 視界が通れば発見/可視扱い
                if (HasLineOfSight(p.x, p.y, tx, ty))
                {
                    visibleNow[tx, ty] = true;
                    discovered[tx, ty] = true;
                }
            }
        }

        // =============================
        // 描画処理
        // =============================

        // 全タイルを走査
        for (int x = 0; x < mapW; x++)
        {
            for (int y = 0; y < mapH; y++)
            {
                Color c;

                if (!discovered[x, y])
                {
                    // 未発見は完全透明
                    c = clearColor;
                }
                else
                {
                    // 発見済み → フロアタイル
                    c = floorColor;

                    // 視界外 → 暗くして見せる
                    if (!visibleNow[x, y])
                        c = Color.Lerp(c, discoveredTint, 0.5f);
                }

                // ピクセルスケールに応じて塗る
                int baseX = x * pixelScale;
                int baseY = y * pixelScale;

                // 塗りつぶし
                for (int px = 0; px < pixelScale; px++)
                {
                    for (int py = 0; py < pixelScale; py++)
                    {
                        tex.SetPixel(baseX + px, baseY + py, c);
                    }
                }
            }
        }

        // =============================
        // 壁アウトライン描画（★ 正しい場所）
        // =============================
        for (int x = 0; x < mapW; x++)
        {
            for (int y = 0; y < mapH; y++)
            {
                if (!discovered[x, y]) continue;
                if (map[x, y] != TileType.Floor) continue;

                bool N = IsWall(x, y + 1);
                bool S = IsWall(x, y - 1);
                bool E = IsWall(x + 1, y);
                bool W = IsWall(x - 1, y);

                int baseX = x * pixelScale;
                int baseY = y * pixelScale;

                if (N)
                    for (int px = 0; px < pixelScale; px++)
                        tex.SetPixel(baseX + px, baseY + pixelScale - 1, wallLineColor);

                if (S)
                    for (int px = 0; px < pixelScale; px++)
                        tex.SetPixel(baseX + px, baseY, wallLineColor);

                if (E)
                    for (int py = 0; py < pixelScale; py++)
                        tex.SetPixel(baseX + pixelScale - 1, baseY + py, wallLineColor);

                if (W)
                    for (int py = 0; py < pixelScale; py++)
                        tex.SetPixel(baseX, baseY + py, wallLineColor);
            }
        }

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

        // ミニマップ上の UV 座標へ変換
        float u = (t.x + 0.5f) / mapW;
        float v = (t.y + 0.5f) / mapH;

        // 位置計算
        Vector2 size = minimapRect.sizeDelta;
        float px = (u - 0.5f) * size.x;
        float py = (v - 0.5f) * size.y;

        // アイコン位置更新
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
        // 既存アイコン削除
        foreach (var icon in enemyIcons)
            Destroy(icon.gameObject);

        enemyIcons.Clear();

        // 敵数に合わせて新規生成
        foreach (var enemy in enemies)
        {
            var icon = Instantiate(enemyIconPrefab, minimapRect);
            icon.gameObject.SetActive(true); // 敵数に合わせて新規生成
            enemyIcons.Add(icon);
        }
    }

    /// <summary>
    /// 敵アイコンの位置をミニマップ上に反映。
    /// 視界内の敵のみ表示し、視界外は非表示とする。
    /// </summary>
    private void UpdateEnemyIcons()
    {
        // 敵数とアイコン数が違えば作り直す
        if (enemyIcons.Count != enemies.Count)
        {
            InitEnemyIcons();
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            var icon  = enemyIcons[i];

            if (enemy == null)
            {
                // 敵が消滅していればアイコンも非表示
                icon.gameObject.SetActive(false);
                continue;
            }

            Vector2Int tile = WorldToTile(enemy.transform.position);

            // 視界に入っていない → 表示しない
            if (!visibleNow[tile.x, tile.y])
            {
                icon.gameObject.SetActive(false);
                continue;
            }

            // 視界内 → 表示
            icon.gameObject.SetActive(true);

            // ミニマップ UI 座標へ変換
            float u = (tile.x + 0.5f) / mapW;
            float v = (tile.y + 0.5f) / mapH;

            // 位置計算
            Vector2 size = minimapRect.sizeDelta;
            float px = (u - 0.5f) * size.x;
            float py = (v - 0.5f) * size.y;

            // アイコン位置更新
            icon.anchoredPosition = new Vector2(px, py);
        }
    }

    public void SetEnemies(IReadOnlyList<GameObject> list)
    {
        enemies.Clear();
        enemies.AddRange(list);
        InitEnemyIcons();
    }
}
