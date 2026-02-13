using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// マス目のハイライト演出を一括管理するクラス。
/// 攻撃範囲・移動可能マスなど、
/// 複数タイルの視覚的表示を担当する。
/// </summary>
public class HighlightManager : MonoBehaviour
{
    public static HighlightManager instance;

    [Header("Highlight Settings")]
    [Tooltip("ハイライト表示に使用するタイルオブジェクト（薄い黄色の四角スプライト）")]
    public GameObject highlightPrefab;
    public GameObject enemyHighlightPrefab;

    /// <summary>
    /// 現在シーン上に生成されているハイライトの一覧。
    /// Clear() で一括削除される。
    /// </summary>
    private readonly List<GameObject> highlights = new List<GameObject>();


    private void Awake()
    {
        instance = this;
    }


    /// <summary>
    /// 指定した複数のグリッド座標にハイライトタイルを生成する。
    /// 主に移動可能マスや攻撃可能マスの可視化に利用する。
    /// </summary>
    /// <param name="tiles">ハイライトを表示したいグリッド座標リスト</param>
    public void ShowTiles(List<Vector2Int> tiles)
    {
        // 既存のハイライトはすべて削除
        Clear();

        foreach (var t in tiles)
        {
            // ハイライトタイル生成
            GameObject obj = Instantiate(highlightPrefab);
            obj.transform.position = new Vector3(t.x, t.y, 0);

            highlights.Add(obj);
        }
    }

    public void ShowEnemyTiles(List<Vector2Int> tiles)
    {
        // 既存のハイライトはすべて削除
        Clear();
        foreach (var t in tiles)
        {
            // ハイライトタイル生成
            GameObject obj = Instantiate(enemyHighlightPrefab);
            obj.transform.position = new Vector3(t.x, t.y, 0);
            highlights.Add(obj);
        }
    }


    /// <summary>
    /// 現在表示中のすべてのハイライトを削除する。
    /// 攻撃方向変更時やターン終了時に呼び出される。
    /// </summary>
    public void Clear()
    {
        foreach (var h in highlights)
        {
            Destroy(h);
        }

        highlights.Clear();
    }
}
