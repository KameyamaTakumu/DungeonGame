using UnityEngine;

public class StepsDownTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
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
            // プレイヤーが離れたら UI を閉じる
            StepsDownUI.Instance.Close();
        }
    }
}
