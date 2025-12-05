using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 敵の種類リスト・スポーン設定・生成処理を一括管理するコントローラ。
/// DungeonGenerator からランダム床座標を取得し、
/// 設定に従って敵をまとめて生成する。
///
/// 【役割】
/// ・敵のプレハブ管理
/// ・どの敵を何体出すか（SpawnInfo）の管理
/// ・ダンジョン生成後に敵をまとめてスポーン
/// ・生成した敵の参照保持
/// </summary>
public class EnemySpawnerController : MonoBehaviour
{
    [Header("敵プレハブのリスト（プルダウンの選択肢になる）")]
    [SerializeField]
    private List<GameObject> enemyPrefabs = new List<GameObject>();

    [Header("スポーン設定")]
    [SerializeField]
    private List<SpawnInfo> spawnSettings = new List<SpawnInfo>();

    /// <summary>
    /// 生成済みの敵を保持
    /// </summary>
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private DungeonGenerator dungeon;

    void Start()
    {
        // DungeonGenerator をシーンから取得
        dungeon = FindAnyObjectByType<DungeonGenerator>();

        if (dungeon == null)
        {
            Debug.LogError("EnemySpawnerController: DungeonGenerator がシーンにありません");
            return;
        }

        SpawnAllEnemies(); // 起動時に全ての敵をまとめて生成
    }

    /// <summary>
    /// 設定された全スポーン情報に基づいて敵を生成する
    /// </summary>
    private void SpawnAllEnemies()
    {
        foreach (var info in spawnSettings)
        {
            // 指定数だけ敵を生成
            for (int i = 0; i < info.count; i++)
            {
                Vector2Int pos = dungeon.GetRandomFloorPosition();
                SpawnEnemy(pos, info.enemyPrefab);
            }
        }
    }

    /// <summary>
    /// 単体の敵を生成してリストに追加
    /// </summary>
    public GameObject SpawnEnemy(Vector2Int pos, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("EnemySpawnerController: enemyPrefab が null です");
            return null;
        }

        // プレハブから生成
        GameObject enemy = Instantiate(prefab);

        // ワールド座標に変換して配置
        enemy.transform.position = new Vector3(pos.x, pos.y, 0);

        // 参照を保持
        spawnedEnemies.Add(enemy);

        return enemy;
    }

    /// <summary>
    /// 生成された敵一覧を読み取り専用で返す
    /// </summary>
    public IReadOnlyList<GameObject> GetSpawnedEnemies() => spawnedEnemies;
}

/// <summary>
/// Inspector 上で設定する「敵種類 & 数」
/// </summary>
[Serializable]
public class SpawnInfo
{
    [Tooltip("enemyPrefabs の index。0 なら最初の敵プレハブ")]
    public GameObject enemyPrefab;

    [Tooltip("生成する数")]
    public int count;
}
