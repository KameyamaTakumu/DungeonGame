using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillExecutor : MonoBehaviour
{
    PlayerStatus playerStatus;


    private CardInventoryUIController cardInventoryUIController;

    void Awake()
    {
        playerStatus = FindFirstObjectByType<PlayerStatus>();

        cardInventoryUIController = FindFirstObjectByType<CardInventoryUIController>();
    }

    public void ExecuteCardSkill(CardData card)
    {
        //if (card.cardType != CardType.Use)
        //    return;

        //HighlightManager.instance.Clear(); // ★追加

        //if (playerStatus == null)
        //{
        //    Debug.LogError("PlayerStatus が見つかりません");
        //    return;
        //}

        //Debug.Log($"カード使用: {card.cardName}");

        //// UIを閉じる
        //if (cardInventoryUIController != null)
        //{
        //    cardInventoryUIController.HideAllUI();
        //}

        //Vector2Int origin = Vector2Int.RoundToInt(playerStatus.transform.position);
        //int hitCount = 0;

        //switch (card.rangeType)
        //{
        //    case CardRangeType.Around:
        //        hitCount = AttackAround(origin, card);
        //        break;

        //    case CardRangeType.Line:
        //        hitCount = AttackLine(origin, card);
        //        break;
        //}

        //if (hitCount == 0)
        //    Debug.Log("範囲内に敵はいませんでした");
        //else
        //    Debug.Log($"合計 {hitCount} 体の敵にヒット！");
        if (card.cardType != CardType.Use)
            return;

        HighlightManager.instance.Clear();

        if (playerStatus == null)
        {
            Debug.LogError("PlayerStatus が見つかりません");
            return;
        }

        Debug.Log($"カード使用: {card.cardName}");

        // UIを閉じる
        cardInventoryUIController?.HideAllUI();

        // ★ ここで効果タイプ判定
        switch (card.useEffectType)
        {
            case UseEffectType.Attack:
                ExecuteAttack(card);
                break;

            case UseEffectType.Heal:
                ExecuteHeal(card);
                break;
        }
    }

    void ExecuteAttack(CardData card)
    {
        Vector2Int origin = Vector2Int.RoundToInt(playerStatus.transform.position);
        int hitCount = 0;

        switch (card.rangeType)
        {
            case CardRangeType.Around:
                hitCount = AttackAround(origin, card);
                break;

            case CardRangeType.Line:
                hitCount = AttackLine(origin, card);
                break;
        }

        if (hitCount == 0)
            Debug.Log("範囲内に敵はいませんでした");
        else
            Debug.Log($"合計 {hitCount} 体の敵にヒット！");
    }

    void ExecuteHeal(CardData card)
    {
        int beforeHP = playerStatus.status.HP;

        playerStatus.status.HP = Mathf.Min(
            playerStatus.status.HP + card.healAmount,
            playerStatus.MaxHP
        );

        playerStatus.OnHPChanged?.Invoke();

        int healed = playerStatus.status.HP - beforeHP;

        Debug.Log($"HP回復: +{healed}");
    }

    int AttackAround(Vector2Int origin, CardData card)
    {
        int hitCount = 0;

        for (int x = -card.range; x <= card.range; x++)
        {
            for (int y = -card.range; y <= card.range; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector2Int checkPos = origin + new Vector2Int(x, y);
                GameObject target = CombatManager.GetObjectAt(checkPos);

                if (target == null) continue;

                EnemyStatus enemy = target.GetComponent<EnemyStatus>();
                if (enemy != null)
                {
                    enemy.TakeDamage(card.damage);
                    hitCount++;
                }
            }
        }

        return hitCount;
    }

    int AttackLine(Vector2Int origin, CardData card)
    {
        int hitCount = 0;
        Vector2Int dir = playerStatus.facingDir;

        for (int i = 1; i <= card.range; i++)
        {
            Vector2Int checkPos = origin + dir * i;
            GameObject target = CombatManager.GetObjectAt(checkPos);

            if (target == null) continue;

            EnemyStatus enemy = target.GetComponent<EnemyStatus>();
            if (enemy != null)
            {
                enemy.TakeDamage(card.damage);
                hitCount++;
            }
        }

        return hitCount;
    }

    /// <summary>
    /// カードの攻撃範囲を取得（ハイライト用）
    /// </summary>
    public List<Vector2Int> GetCardRangeTiles(CardData card)
    {
        Vector2Int origin = Vector2Int.RoundToInt(playerStatus.transform.position);
        List<Vector2Int> tiles = new List<Vector2Int>();

        switch (card.rangeType)
        {
            case CardRangeType.Around:
                for (int x = -card.range; x <= card.range; x++)
                {
                    for (int y = -card.range; y <= card.range; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        tiles.Add(origin + new Vector2Int(x, y));
                    }
                }
                break;

            case CardRangeType.Line:
                Vector2Int dir = playerStatus.facingDir;
                for (int i = 1; i <= card.range; i++)
                {
                    tiles.Add(origin + dir * i);
                }
                break;
        }

        return tiles;
    }
}
