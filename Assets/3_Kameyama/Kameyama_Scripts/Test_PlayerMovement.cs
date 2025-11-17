using UnityEngine;

/// <summary>
/// シンプルな2D移動。WASD/矢印キーで動く。
/// </summary>
public class Test_PlayerMovement : MonoBehaviour
{
    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 入力取得
        input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = input * moveSpeed; // Unity 6 なので linearVelocity 使用
    }
}
