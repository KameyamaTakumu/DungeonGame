using UnityEngine;

// カメラがターゲットを滑らかに追尾するスクリプト
public class CameraFollow : MonoBehaviour
{
    [CustomLabel("追尾するターゲット")]
    [SerializeField] private Transform target;
    [CustomLabel("追尾の滑らかさ")]
    [SerializeField] private float smooth = 5f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = transform.position;

        pos.x = Mathf.Lerp(pos.x, target.position.x, smooth * Time.deltaTime);
        pos.y = Mathf.Lerp(pos.y, target.position.y, smooth * Time.deltaTime);

        transform.position = pos;
    }

    // ターゲットを設定するメソッド
    public void SetTarget(Transform t)
    {
        target = t;
    }
}
