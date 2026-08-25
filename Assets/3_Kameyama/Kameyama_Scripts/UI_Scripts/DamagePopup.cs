using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public TextMeshPro text;
    public float moveUpSpeed = 1f;
    public float lifeTime = 1f;
    private Vector3 moveDirection = Vector3.up;  // 方向を変数化
    private float elapsedTime = 0f;

    public void Setup(int damage)
    {
        text.text = damage.ToString();
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        transform.position += moveDirection * moveUpSpeed * Time.deltaTime;

        // フェードアウト効果を追加すると視覚的に良好
        if (text.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / lifeTime));
        }
    }
}
