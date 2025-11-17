using UnityEngine;

public class Test_PlayerSpawner : MonoBehaviour
{
    [Header("プレイヤーのPrefab")]
    [SerializeField] private GameObject playerPrefab;

    public Test_CameraFollow cameraFollow;

    private GameObject playerInstance;

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
            playerInstance = Instantiate(playerPrefab);
        }

        playerInstance.transform.position = new Vector3(pos.x, pos.y, 0);

        cameraFollow.SetTarget(playerInstance.transform);
    }

    public GameObject GetPlayer() => playerInstance;
}
