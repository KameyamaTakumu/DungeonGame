using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    public CameraFollow cameraFollow;

    private GameObject playerInstance;

    public void SpawnPlayer(Vector2Int pos)
    {
        if (playerInstance == null)
            playerInstance = Instantiate(playerPrefab);

        playerInstance.transform.position = new Vector3(pos.x, pos.y, 0);

        if (cameraFollow != null)
            cameraFollow.SetTarget(playerInstance.transform);
    }

    public GameObject GetPlayer() => playerInstance;
}
