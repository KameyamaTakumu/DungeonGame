using UnityEngine;

public class Test_PlayerSpawner : MonoBehaviour
{
    [Header("プレイヤーのPrefab")]
    [SerializeField] private GameObject playerPrefab;
    [Header("敵のPrefab")]
    [SerializeField] private GameObject enemyPrefab;

    public Test_CameraFollow cameraFollow;

    private GameObject playerInstance;
    private GameObject enemyInstance;

    /// <summary>
    /// DungeonGenerator から呼ばれる
    /// </summary>
    public void SpawnPlayer(Vector2Int pos)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Test_PlayerSpawner：playerPrefab が設定されていません");
            return;
        }

        // 既に生成済みなら位置だけ更新
        if (playerInstance == null)
        {
            playerInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }

        // タイル中心に合わせる
        Vector3 spawnPos = new Vector3(pos.x, pos.y, 0f);
        playerInstance.transform.position = spawnPos;

        // カメラ追従を設定（null なら無視）
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(playerInstance.transform);
        }
        else
        {
            Debug.LogWarning("PlayerSpawner: cameraFollow がシーン内に設定されていません");
        }

        Debug.Log($"プレイヤーをスポーン: {spawnPos}");
    }

    public void SpawnEnemy(Vector2Int pos)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Test_PlayerSpawner：enemyPrefab が設定されていません");
            return;
        }

        // 既に生成済みなら位置だけ更新
        if (enemyInstance == null)
        {
            enemyInstance = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
        }

        // タイル中心に合わせる
        Vector3 spawnPos = new Vector3(pos.x, pos.y, 0f);
        enemyInstance.transform.position = spawnPos;

        Debug.Log($"敵をスポーン: {spawnPos}");
    }

    public GameObject GetPlayer() => playerInstance;
    public GameObject GetEnemy () => enemyInstance;
}
