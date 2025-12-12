using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    [Header("Enemy Base Status")]
    public BaseStatus status = new BaseStatus(10, 5, 1);

    public void TakeDamage(int amount)
    {
        status.TakeDamage(amount);
        Debug.Log($"“GHP: {status.HP}");

        if (status.IsDead())
        {
            Debug.Log("“G€–SI");
            Die();
        }
    }

    /// <summary>
    /// “G‚ª€–S‚µ‚½‚Æ‚«‚Ìˆ—
    /// </summary>
    private void Die()
    {
        Debug.Log($"{name} ‚Í“|‚ê‚½I");
        Destroy(gameObject);
    }
}
