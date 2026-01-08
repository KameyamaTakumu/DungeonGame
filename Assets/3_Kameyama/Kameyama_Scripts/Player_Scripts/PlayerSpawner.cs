using UnityEngine;

/// <summary>
/// プレイヤーキャラクターの生成（スポーン）を管理するクラス。
/// 生成後、カメラ追従対象も自動で設定することで、
/// プレイヤー生成とカメラ連動を簡潔に管理できる設計。
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [CustomLabel("プレイヤーのPrefab")]
    [SerializeField]
    private GameObject playerPrefab;

    private CameraFollow cameraFollow;

    // 実際に生成されたプレイヤーのインスタンスを保持
    private GameObject playerInstance;

    /// <summary>
    /// 指定グリッド位置にプレイヤーを生成、または既存インスタンスを移動。
    /// カメラが設定されている場合は自動で追従対象を設定する。
    /// </summary>
    /// <param name="pos">生成位置（グリッド座標）</param>
    public void SpawnPlayer(Vector2Int pos)
    {
        //// プレイヤー未生成時は生成
        //if (playerInstance == null)
        //{
        //    playerInstance = Instantiate(playerPrefab);
        //}

        //// 生成または既存プレイヤーを指定位置へ配置
        //playerInstance.transform.position = new Vector3(pos.x, pos.y, 0);

        //// カメラ追従スクリプトがシーン内にある場合は追従対象を設定
        //if (cameraFollow == null)            
        //{ 
        //    cameraFollow = FindAnyObjectByType<CameraFollow>();
        //    cameraFollow.SetTarget(playerInstance.transform);
        //}

        // プレイヤー未生成時は生成
        if (playerInstance == null)
        {
            playerInstance = FindFirstObjectByType<PlayerStatus>()?.gameObject;

            if (playerInstance == null)
            {
                playerInstance = Instantiate(playerPrefab);
            }
        }

        // 生成または既存プレイヤーを指定位置へ配置
        playerInstance.transform.position = new Vector3(pos.x, pos.y, 0);

        // カメラ追従スクリプトがシーン内にある場合は追従対象を設定
        if (cameraFollow == null)
        {
            cameraFollow = FindAnyObjectByType<CameraFollow>();
            cameraFollow.SetTarget(playerInstance.transform);
        }
    }

    /// <summary>
    /// 生成済みプレイヤーの取得
    /// </summary>
    /// <returns>プレイヤー GameObject インスタンス</returns>
    public GameObject GetPlayer() => playerInstance;
}
