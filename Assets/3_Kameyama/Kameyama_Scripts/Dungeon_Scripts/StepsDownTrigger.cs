using UnityEngine;

public class StepsDownTrigger : MonoBehaviour
{
    private bool isPlayerInside = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;

            // ▼UI を開く（シングルトンなどで管理している前提）
            StepsDownUI.Instance.Open(() => {
                // 「はい」のとき呼ばれる
                SceneChanger.Instance.LoadNextFloor();
            });
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;

            // プレイヤーが離れたら UI を閉じる
            StepsDownUI.Instance.Close();
        }
    }
}
