using UnityEngine;

public class CardInventoryUIManager : MonoBehaviour
{
    public GameObject consumableUI;
    public GameObject passiveUI;

    void Update()
    {
        // 使い切りカードUI表示 → Qキー
        if (Input.GetKeyDown(KeyCode.Q))
        {
            consumableUI.SetActive(true);

            Debug.Log("使い切りカードUIを表示");

            passiveUI.SetActive(false);
        }

        // バフカードUI表示 → Zキー
        if (Input.GetKeyDown(KeyCode.Z))
        {
            passiveUI.SetActive(true);

            Debug.Log("バフカードUIを表示");

            consumableUI.SetActive(false);
        }

        // カードUI非表示 → Xキー
        if (Input.GetKeyDown(KeyCode.X))
        {
            consumableUI.SetActive(false);
            passiveUI.SetActive(false);

            Debug.Log("カードUIを非表示");
        }
    }
}
