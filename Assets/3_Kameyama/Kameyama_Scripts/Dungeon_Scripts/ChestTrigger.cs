using UnityEngine;
using UnityEngine.Tilemaps;

public class ChestTrigger : MonoBehaviour
{
    public ChestCardMode mode;
    bool opened = false;

    // ★ 追加：自分が置かれているタイル座標
    private Vector3Int tilePos;

    void Start()
    {
        // ワールド座標 → タイル座標へ変換して保持
        tilePos = DungeonGenerator.instance
            .GetComponent<DungeonGenerator>()
            .GetChestTilePosition(transform.position);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (opened) return;
        if (!collision.CompareTag("Player")) return;

        opened = true;

        CardInventory inventory = CardInventory.Instance;

        CardSelectUI selectUI = FindFirstObjectByType<CardSelectUI>(FindObjectsInactive.Include);
        if (selectUI == null)
        {
            Debug.LogError("CardSelectUI がシーンに存在しません");
            return;
        }

        CardInventoryUIController uiCtrl = FindFirstObjectByType<CardInventoryUIController>();
        if (uiCtrl == null)
        {
            Debug.LogError("CardInventoryUIController がシーンに存在しません");
            return;
        }

        CardDataBase database = uiCtrl.database;

        // 抽選
        CardData[] options = ChestCardLottery(database, mode);

        selectUI.Open(inventory, options, () =>
        {
            DungeonGenerator.instance.ClearChestTile(tilePos);
            Destroy(gameObject);
        });
    }

    CardData[] ChestCardLottery(CardDataBase db, ChestCardMode mode)
    {
        CardType type;

        if (mode == ChestCardMode.BuffOnly)
            type = CardType.Buff;
        else if (mode == ChestCardMode.UseOnly)
            type = CardType.Use;
        else
            type = (Random.value < 0.5f) ? CardType.Buff : CardType.Use;

        var list = db.GetCards(type);

        CardData[] result = new CardData[3];
        for (int i = 0; i < 3; i++)
            result[i] = list[Random.Range(0, list.Length)];

        return result;
    }
}
