using UnityEngine;
using System;
using System.Collections.Generic;

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
    [Header("敵プレハブのリスト")]
    [CustomLabel("出現させたい敵プレハブのリスト"),SerializeField]
    private List<GameObject> enemyPrefabs = new List<GameObject>();

    [Header("スポーン設定")]
    [CustomLabel("スポーンの出現敵・数の設定"),SerializeField]
    private List<SpawnInfo> spawnSettings = new List<SpawnInfo>();

    [Header("ボス設定")]
    [SerializeField]
    private GameObject bossPrefab;

    /// <summary>
    /// 生成済みの敵を保持
    /// </summary>
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    // 敵種類ごとの生成数カウント
    private Dictionary<string, int> enemyNameCounter = new Dictionary<string, int>();

    private DungeonGenerator dungeon;

    private MiniMapRenderer miniMap;

    void Start()
    {
        // DungeonGenerator をシーンから取得
        dungeon = FindAnyObjectByType<DungeonGenerator>();
        miniMap = FindAnyObjectByType<MiniMapRenderer>();

        if (dungeon == null)
        {
            Debug.LogError("EnemySpawnerController: DungeonGenerator がシーンにありません");
            return;
        }

        // ボス階層なら雑魚を出さない
        if (DungeonGenerator.CurrentFloor == 3)
        {
            SpawnBoss();
        }
        else
        {
            SpawnAllEnemies();// 起動時に全ての敵をまとめて生成
        }

        // 敵生成が終わったあとに通知
        if (miniMap != null)
            miniMap.SetEnemies(spawnedEnemies);
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
    /// <param name="pos">生成位置（グリッド座標）</param>
    /// <param name="prefab">生成する敵のプレハブ</param>
    public GameObject SpawnEnemy(Vector2Int pos, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("EnemySpawnerController: enemyPrefab が null です");
            return null;
        }

        // プレハブから生成
        GameObject enemy = Instantiate(prefab);

        // 元のプレハブ名
        string baseName = prefab.name;

        // カウント管理
        if (!enemyNameCounter.ContainsKey(baseName))
            enemyNameCounter[baseName] = 0;

        enemyNameCounter[baseName]++;

        // SlimeA, SlimeB, SlimeC ...
        char suffix = (char)('A' + enemyNameCounter[baseName] - 1);

        // 名前設定
        enemy.name = baseName + suffix;

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

    /// <summary>
    /// ボスを1体だけスポーンする。
    /// 雑魚敵は一切生成しない。
    /// </summary>
    private void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("BossPrefab が設定されていません");
            return;
        }

        // 部屋中央に配置
        Vector2Int bossPos = new Vector2Int(9, 7);

        GameObject boss = Instantiate(bossPrefab);
        boss.transform.position = new Vector3(bossPos.x, bossPos.y, 0);

        spawnedEnemies.Add(boss);
    }
}

/// <summary>
/// Inspector 上で設定する「敵種類 & 数」
/// </summary>
[Serializable]
public class SpawnInfo
{
    [Tooltip("出現させたい敵のプレハブ")]
    public GameObject enemyPrefab;

    [Tooltip("出現させたい敵の数")]
    public int count;
}
