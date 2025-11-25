using UnityEngine;

/// <summary>
/// プレイヤーをスムーズに追従するカメラ
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;  // プレイヤー
    [SerializeField] private float smooth = 5f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, target.position.x, smooth * Time.deltaTime);
        pos.y = Mathf.Lerp(pos.y, target.position.y, smooth * Time.deltaTime);

        transform.position = pos;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }
}
