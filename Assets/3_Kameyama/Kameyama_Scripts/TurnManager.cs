using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    private bool isPlayerTurn = true;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// プレイヤーが「歩いた」「攻撃した」ときに呼ぶ
    /// </summary>
    public void PlayerDidAction()
    {
        if (!isPlayerTurn) return;

        Debug.Log("プレイヤーのターン終了 ＞ 敵ターンへ");
        isPlayerTurn = false;

        EnemyTurn();
    }

    private void EnemyTurn()
    {
        Debug.Log("敵のターン");

        // --- 敵の行動（仮） ---
        Debug.Log("敵がプレイヤーに5ダメージ");

        // --- 敵ターン終了 ---
        isPlayerTurn = true;
        Debug.Log(" 敵ターン終了 ＞ プレイヤーターンへ");
    }
}
