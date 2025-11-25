using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Texture2D を使ったミニマップ描画クラス
/// - DungeonGenerator から DrawMiniMap(map) を呼ぶことで初期化されます
/// - プレイヤー移動ごとに視界（FOV）を再計算してミニマップを更新
/// </summary>
public class MiniMapRenderer : MonoBehaviour
{
    [Header("UI")]
    public RawImage minimapImage;
    public RectTransform minimapRect;
    public RectTransform playerIcon;

    [Header("敵アイコン")]
    public RectTransform enemyIconPrefab;  // ★ アイコンのプレハブ
    private List<RectTransform> enemyIcons = new List<RectTransform>();
    private List<GameObject> enemies = new List<GameObject>();

    [Header("見た目")]
    public int pixelScale = 4;

    public Color floorColor = new Color(0.85f, 0.85f, 0.85f);

    // ★ 壁を透明にする（Alpha = 0）
    public Color wallColor = new Color(0f, 0f, 0f, 0f);

    // 未探索は透明（Alpha = 0）
    private readonly Color clearColor = new Color(0f, 0f, 0f, 0f);  // ★追加

    public Color discoveredTint = new Color(0.35f, 0.35f, 0.35f);

    [Header("視界")]
    public int viewRadius = 8;

    // 内部
    private TileType[,] map;
    private int mapW, mapH;
    private Texture2D tex;
    private bool[,] discovered;
    private bool[,] visibleNow;
    private Vector2Int lastPlayerTile = new Vector2Int(int.MinValue, int.MinValue);
    private GameObject player;

    public void DrawMiniMap(TileType[,] sourceMap)
    {
        if (sourceMap == null) return;

        map = sourceMap;
        mapW = map.GetLength(0);
        mapH = map.GetLength(1);

        int texW = Mathf.Max(1, mapW * pixelScale);
        int texH = Mathf.Max(1, mapH * pixelScale);

        tex = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        if (minimapImage != null)
        {
            minimapImage.texture = tex;

            // ★ RawImage の色は白（Alpha=255）にしておく必要がある
            minimapImage.color = Color.white;
        }

        if (minimapRect == null && minimapImage != null)
            minimapRect = minimapImage.rectTransform;

        discovered = new bool[mapW, mapH];
        visibleNow = new bool[mapW, mapH];

        // ★ 未探索は透明にする（黒ではない）
        ClearTextureTransparent();

        player = GameObject.FindGameObjectWithTag("Player");

        ForceRecalculateFOVAndDraw();
    }

    void Update()
    {
        if (map == null || tex == null) return;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player == null) return;

        Vector2Int playerTile = WorldToTile(player.transform.position);

        if (playerTile != lastPlayerTile)
        {
            lastPlayerTile = playerTile;
            ForceRecalculateFOVAndDraw();
        }

        if (playerIcon != null)
            UpdatePlayerIconPosition(playerTile);

        UpdateEnemyIcons(); // ★ 敵アイコン更新
    } 

    private Vector2Int WorldToTile(Vector2 worldPos)
    {
        int tx = Mathf.RoundToInt(worldPos.x);
        int ty = Mathf.RoundToInt(worldPos.y);

        tx = Mathf.Clamp(tx, 0, mapW - 1);
        ty = Mathf.Clamp(ty, 0, mapH - 1);

        return new Vector2Int(tx, ty);
    }

    // =============================
    // ★ 透明クリアに変更（最重要）
    // =============================
    private void ClearTextureTransparent()
    {
        for (int x = 0; x < tex.width; x++)
            for (int y = 0; y < tex.height; y++)
                tex.SetPixel(x, y, clearColor);

        tex.Apply();
    }

    private void ForceRecalculateFOVAndDraw()
    {
        if (player == null) return;

        Vector2Int p = WorldToTile(player.transform.position);

        for (int x = 0; x < mapW; x++)
            for (int y = 0; y < mapH; y++)
                visibleNow[x, y] = false;

        int minX = Mathf.Max(0, p.x - viewRadius);
        int maxX = Mathf.Min(mapW - 1, p.x + viewRadius);
        int minY = Mathf.Max(0, p.y - viewRadius);
        int maxY = Mathf.Min(mapH - 1, p.y + viewRadius);

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

        for (int x = 0; x < mapW; x++)
        {
            for (int y = 0; y < mapH; y++)
            {
                Color c;

                if (!discovered[x, y])
                {
                    c = clearColor;         // ★ 未発見は透明
                }
                else
                {
                    if (map[x, y] == TileType.Floor)
                        c = floorColor;
                    else
                        c = wallColor;      // ★ 壁は透明

                    if (!visibleNow[x, y])
                        c = Color.Lerp(c, discoveredTint, 0.5f);
                }

                int baseX = x * pixelScale;
                int baseY = y * pixelScale;
                for (int px = 0; px < pixelScale; px++)
                    for (int py = 0; py < pixelScale; py++)
                        tex.SetPixel(baseX + px, baseY + py, c);
            }
        }

        tex.Apply();
    }

    private bool HasLineOfSight(int x0, int y0, int x1, int y1)
    {
        foreach (var pt in BresenhamLine(x0, y0, x1, y1))
        {
            if (pt.x == x0 && pt.y == y0) continue;
            if (map[pt.x, pt.y] == TileType.Wall) return false;
        }
        return true;
    }

    private System.Collections.Generic.IEnumerable<Vector2Int> BresenhamLine(int x0, int y0, int x1, int y1)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        int x = x0, y = y0;

        while (true)
        {
            yield return new Vector2Int(x, y);
            if (x == x1 && y == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 < dx) { err += dx; y += sy; }
        }
    }

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

    public void ForceRefreshEnemies()
    {
        enemies.Clear();
        enemies.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        InitEnemyIcons();
    }

    private void InitEnemyIcons()
    {
        // 既存アイコンがあれば削除
        foreach (var icon in enemyIcons)
            Destroy(icon.gameObject);

        enemyIcons.Clear();

        // 敵の数だけアイコン作成
        foreach (var enemy in enemies)
        {
            var icon = Instantiate(enemyIconPrefab, minimapRect);
            icon.gameObject.SetActive(false); // 初期は非表示
            enemyIcons.Add(icon);
        }
    }

    private void UpdateEnemyIcons()
    {
        // ★ 敵数とアイコン数が違えば作り直す
        if (enemyIcons.Count != enemies.Count)
        {
            InitEnemyIcons();
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

            // ★ 視界内のみ表示
            if (!visibleNow[tile.x, tile.y])
            {
                icon.gameObject.SetActive(false);
                continue;
            }

            icon.gameObject.SetActive(true);

            // ミニマップ座標に変換
            float u = (tile.x + 0.5f) / mapW;
            float v = (tile.y + 0.5f) / mapH;

            Vector2 size = minimapRect.sizeDelta;
            float px = (u - 0.5f) * size.x;
            float py = (v - 0.5f) * size.y;

            icon.anchoredPosition = new Vector2(px, py);
        }
    }


    public void ForceRedraw() => ForceRecalculateFOVAndDraw();
}
