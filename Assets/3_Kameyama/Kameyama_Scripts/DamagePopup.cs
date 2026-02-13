using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public TextMeshPro text;
    public float moveUpSpeed = 1f;
    public float lifeTime = 1f;

    public void Setup(int damage)
    {
        text.text = damage.ToString();
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += Vector3.up * moveUpSpeed * Time.deltaTime;
    }
}
