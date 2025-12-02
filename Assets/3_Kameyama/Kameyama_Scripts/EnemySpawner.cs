using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 敵キャラクターの生成（スポーン）を管理するクラス。
/// 生成された敵は内部リストで保持され、後から参照可能。
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("敵の種類リスト（任意の数だけ登録可能）")]
    [SerializeField]
    private List<GameObject> enemyPrefabs = new List<GameObject>();

    /// <summary>
    /// 生成された敵のインスタンスを保持
    /// </summary>
    private List<GameObject> enemies = new List<GameObject>();

    /// <summary>
    /// 単体生成。敵の種類を index で指定。
    /// </summary>
    public GameObject SpawnEnemy(Vector2Int pos, int typeIndex)
    {
        if (typeIndex < 0 || typeIndex >= enemyPrefabs.Count)
        {
            Debug.LogError($"EnemySpawner: 不正な敵タイプ index={typeIndex}");
            return null;
        }

        GameObject prefab = enemyPrefabs[typeIndex];
        GameObject enemy = Instantiate(prefab);

        enemy.transform.position = new Vector3(pos.x, pos.y, 0);

        enemies.Add(enemy);
        return enemy;
    }

    /// <summary>
    /// 敵をまとめて生成
    /// </summary>
    public void SpawnEnemies(Func<Vector2Int> getRandomPos, int typeIndex, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2Int pos = getRandomPos();   // ← ここで毎回ランダム
            SpawnEnemy(pos, typeIndex);
        }
    }

    /// <summary>
    /// 生成済みの敵リストを読み取り専用で取得
    /// 外部からの誤操作でリストが変更されないよう IReadOnlyList を返す
    /// </summary>
    /// <returns>生成済み敵の読み取り専用リスト</returns>
    public IReadOnlyList<GameObject> GetEnemies() => enemies;
}
