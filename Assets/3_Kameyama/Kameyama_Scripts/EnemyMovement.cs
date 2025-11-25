using System;

/// <summary>
/// シンプルな2D移動。WASD/矢印キーで動く。
/// </summary>
public class EnemyMovement : BaseMovement
{
    public static EnemyMovement instance;
    public bool isAttacking = false;

    public Action onMoveFinished;

    protected override void Awake()
    {
        base.Awake();

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    protected override void OnMoveFinished(bool debugMove)
    {
        onMoveFinished?.Invoke();
    }
}
