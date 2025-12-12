using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    [Header("Enemy Base Status")]
    private DropSystem dropSystem;
    private PlayerInventory playerInventory;
    public BaseStatus status = new BaseStatus(10, 5, 1);

    //private void Start()
    //{
    //    dropSystem = GetComponent<DropSystem>();
    //    playerInventory = FindObjectOfType<PlayerInventory>();
    //    // ※ プレイヤーがシーンに1人いる前提
    //}


    public void TakeDamage(int amount)
    {
        status.TakeDamage(amount);
        Debug.Log($"敵HP: {status.HP}");

        if (status.IsDead())
        {
            Debug.Log("敵死亡！");
            Die();
        }
    }

    /// <summary>
    /// 敵が死亡したときの処理
    /// </summary>
    private void Die()
    {
        //// ドロップ抽選
        //var dropItem = dropSystem.GetWeightedDrop();

        //if (dropItem != null)
        //{
        //    // 直接入手させる
        //    playerInventory.AddItem(dropItem);
        //}


        Debug.Log($"{name} は倒れた！");
        Destroy(gameObject);
    }
}
