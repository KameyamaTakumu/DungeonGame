using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 敵キャラクターの生成（スポーン）を管理するクラス。
/// 生成された敵は内部リストで保持され、後から参照可能。
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [CustomLabel("敵キャラクターのPrefab")]
    [SerializeField] 
    private GameObject enemyPrefab;

    /// <summary>
    /// 生成された敵のインスタンスを保持するリスト
    /// IReadOnlyList を通じて外部から参照可能
    /// </summary>
    private List<GameObject> enemies = new List<GameObject>();

    /// <summary>
    /// 指定位置に敵を生成し、内部リストに追加
    /// </summary>
    /// <param name="pos">生成位置（グリッド座標）</param>
    /// <returns>生成された敵の GameObject インスタンス</returns>
    public GameObject SpawnEnemy(Vector2Int pos)
    {
        // プレハブから敵を生成
        GameObject enemy = Instantiate(enemyPrefab);

        // 指定位置に配置（Z座標は 0 とする
        enemy.transform.position = new Vector3(pos.x, pos.y, 0);

        // 生成リストに追加して管理
        enemies.Add(enemy);

        return enemy;
    }

    /// <summary>
    /// 生成済みの敵リストを読み取り専用で取得
    /// 外部からの誤操作でリストが変更されないよう IReadOnlyList を返す
    /// </summary>
    /// <returns>生成済み敵の読み取り専用リスト</returns>
    public IReadOnlyList<GameObject> GetEnemies() => enemies;
}
