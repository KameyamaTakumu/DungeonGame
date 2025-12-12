using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public BaseStatus status = new BaseStatus(20, 10, 1);

    public void TakeDamage(int amount)
    {
        status.TakeDamage(amount);
        Debug.Log($"プレイヤーHP: {status.HP}");

        if (status.IsDead())
        {
            Debug.Log("プレイヤー死亡！");
            // TODO: ゲームオーバー処理
        }
    }
}
