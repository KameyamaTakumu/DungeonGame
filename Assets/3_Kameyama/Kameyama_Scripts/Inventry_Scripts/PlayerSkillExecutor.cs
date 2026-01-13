using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillExecutor : MonoBehaviour
{
    PlayerStatus playerStatus;

    private CardInventoryUIController cardInventoryUIController;

    TurnManager turnManager;

    void Awake()
    {
        playerStatus = FindFirstObjectByType<PlayerStatus>();

        cardInventoryUIController = FindFirstObjectByType<CardInventoryUIController>();

        turnManager = FindFirstObjectByType<TurnManager>();
    }

    public void ExecuteCardSkill(CardData card)
    {
        //if (card.cardType != CardType.Use)
        //    return;

        HighlightManager.instance.Clear();

        if (playerStatus == null)
        {
            Debug.LogError("PlayerStatus が見つかりません");
            return;
        }

        Debug.Log($"カード使用: {card.cardName}");

        // UIを閉じる
        cardInventoryUIController?.HideAllUI();

        // =========================
        // ★ バフカード
        // =========================
        if (card.cardType == CardType.Buff)
        {
            playerStatus.ApplyBuff(card);
            Debug.Log($"バフカード適用: {card.buffType}");

            PlayerInputLock.Instance.Unlock();
            EndPlayerTurn();
            return;
        }

        // ★ ここで効果タイプ判定
        switch (card.useEffectType)
        {
            case UseEffectType.Attack:
                ExecuteAttack(card);
                break;

            case UseEffectType.Heal:
                ExecuteHeal(card);
                break;

            case UseEffectType.StunAttack:
                ExecuteStunAttack(card);
                break;
        }

        // ★ UIフェーズ終了
        PlayerInputLock.Instance.Unlock();

        // ★ カード使用 = 行動終了
        EndPlayerTurn();
    }

    void EndPlayerTurn()
    {
        if (turnManager == null)
        {
            Debug.LogError("TurnManager が見つかりません");
            return;
        }

        if (!turnManager.isPlayerTurn)
        {
            Debug.Log("すでにプレイヤーターンではありません");
            return;
        }

        Debug.Log("カード使用によりプレイヤーのターン終了");
        turnManager.PlayerTurn();
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
        playerStatus.Heal(card.healAmount);
        Debug.Log($"HP回復: +{card.healAmount}");
    }

    void ExecuteStunAttack(CardData card)
    {
        Vector2Int origin = Vector2Int.RoundToInt(playerStatus.transform.position);

        foreach (var pos in GetCardRangeTiles(card))
        {
            var target = CombatManager.GetObjectAt(pos);
            if (target == null) continue;

            //EnemyStatus enemy = target.GetComponent<EnemyStatus>();
            EnemyStatus enemy = target.GetComponentInParent<EnemyStatus>();
            if (enemy != null)
            {
                enemy.TakeDamage(CalculateDamage(card));
                enemy.ApplyStun(card.stunTurn);
            }
        }
    }

    int AttackAround(Vector2Int origin, CardData card)
    {
        int range = GetEffectiveRange(card);

        int hitCount = 0;

        int damage = CalculateDamage(card); // ★ ここで1回計算

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector2Int checkPos = origin + new Vector2Int(x, y);
                GameObject target = CombatManager.GetObjectAt(checkPos);

                if (target == null) continue;

                //EnemyStatus enemy = target.GetComponent<EnemyStatus>();
                EnemyStatus enemy = target.GetComponentInParent<EnemyStatus>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    hitCount++;
                }
            }
        }

        return hitCount;
    }

    int AttackLine(Vector2Int origin, CardData card)
    {
        int range = GetEffectiveRange(card);

        int hitCount = 0;
        Vector2Int dir = playerStatus.facingDir;

        int damage = CalculateDamage(card); // ★ ここで1回計算

        for (int i = 1; i <= range; i++)
        {
            Vector2Int checkPos = origin + dir * i;
            GameObject target = CombatManager.GetObjectAt(checkPos);

            if (target == null) continue;

            //EnemyStatus enemy = target.GetComponent<EnemyStatus>();
            EnemyStatus enemy = target.GetComponentInParent<EnemyStatus>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                hitCount++;
            }
        }

        return hitCount;
    }

    int CalculateDamage(CardData card)
    {
        int damage = playerStatus.GetUseCardDamage(card.damage);

        if (UnityEngine.Random.value < playerStatus.CritChance)
        {
            damage = Mathf.RoundToInt(damage * 1.5f);
            Debug.Log("クリティカル！");
        }

        return damage;
    }

    /// <summary>
    /// カードの攻撃範囲を取得（ハイライト用）
    /// </summary>
    public List<Vector2Int> GetCardRangeTiles(CardData card)
    {
        int range = GetEffectiveRange(card);

        Vector2Int origin = Vector2Int.RoundToInt(playerStatus.transform.position);
        List<Vector2Int> tiles = new List<Vector2Int>();

        switch (card.rangeType)
        {
            case CardRangeType.Around:
                for (int x = -range; x <= range; x++)
                {
                    for (int y = -range; y <= range; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        tiles.Add(origin + new Vector2Int(x, y));
                    }
                }
                break;

            case CardRangeType.Line:
                Vector2Int dir = playerStatus.facingDir;
                for (int i = 1; i <= range; i++)
                {
                    tiles.Add(origin + dir * i);
                }
                break;
        }

        return tiles;
    }

    int GetEffectiveRange(CardData card)
    {
        return card.range;
    }
}
