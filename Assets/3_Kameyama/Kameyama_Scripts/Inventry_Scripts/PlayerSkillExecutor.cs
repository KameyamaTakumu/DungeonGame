using UnityEngine;

public class PlayerSkillExecutor : MonoBehaviour
{
    PlayerStatus playerStatus;

    void Awake()
    {
        playerStatus = FindFirstObjectByType<PlayerStatus>();
    }

    public void ExecuteCardSkill(CardData card)
    {
        if (card.cardType != CardType.Consumable)
            return;

        if (playerStatus == null)
        {
            Debug.LogError("PlayerStatus が見つかりません");
            return;
        }

        Debug.Log($"カード使用: {card.cardName}");

        // ★ プレイヤーのグリッド座標
        Vector3 pos = playerStatus.transform.position;
        Vector2Int origin = new Vector2Int(
            Mathf.FloorToInt(pos.x),
            Mathf.FloorToInt(pos.y)
        );

        int hitCount = 0;

        // ★ 自分の周囲をレンジ分チェック
        for (int x = -card.range; x <= card.range; x++)
        {
            for (int y = -card.range; y <= card.range; y++)
            {
                // 自分の足元はスキップしたい場合は有効
                if (x == 0 && y == 0)
                    continue;

                Vector2Int checkPos = origin + new Vector2Int(x, y);

                GameObject target = CombatManager.GetObjectAt(checkPos);
                if (target == null)
                    continue;

                EnemyStatus enemy = target.GetComponent<EnemyStatus>();
                if (enemy != null)
                {
                    enemy.TakeDamage(card.damage);
                    hitCount++;

                    Debug.Log(
                        $"敵 {target.name} に {card.damage} ダメージ！ " +
                        $"(pos={checkPos})"
                    );
                }
            }
        }

        if (hitCount == 0)
        {
            Debug.Log("範囲内に敵はいませんでした");
        }
        else
        {
            Debug.Log($"合計 {hitCount} 体の敵にヒット！");
        }
    }
}
