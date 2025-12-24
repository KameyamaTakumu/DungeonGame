using System;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public BaseStatus status = new BaseStatus(20, 10, 1);

    int bonusATK = 0;
    int bonusHP = 0;

    int bonusRange = 0;
    float critChance = 0f;
    float passiveMultiplier = 1f;
    float useAttackBoost = 1f;

    public int Attack => status.ATK + bonusATK;
    public int MaxHP => status.MAX_HP + bonusHP;
    public int Range => status.RANGE + bonusRange + RangeBonus;
    public float CritChance => critChance;
    public float PassiveMultiplier => passiveMultiplier;
    public float UseAttackBoost => useAttackBoost;

    public int RangeBonus { get; private set; } = 0;

    public Action OnHPChanged;

    public Vector2Int facingDir = Vector2Int.down; // 初期向き

    public void ApplyBuff(CardData card)
    {
        switch (card.buffType)
        {
            case BuffType.Attack:
                bonusATK += card.buffValue;
                break;

            case BuffType.HP:
                bonusHP += card.buffValue;
                status.HP += card.buffValue; // 即時回復
                break;

            case BuffType.Range:
                RangeBonus += card.buffValue;
                Debug.Log($"攻撃範囲バフ +{card.buffValue}");
                break;

            case BuffType.CritChance:
                critChance += card.buffValue * 0.01f;
                break;

            case BuffType.PassiveMultiplier:
                passiveMultiplier *= card.buffMultiplier;
                break;

            case BuffType.UseAttackBoost:
                useAttackBoost *= card.buffMultiplier;
                break;
        }

        Debug.Log($"バフ適用: {card.buffType} +{card.buffValue}");
    }

    public void RemoveBuff(CardData card)
    {
        switch (card.buffType)
        {
            case BuffType.Attack:
                bonusATK -= card.buffValue;
                break;

            case BuffType.HP:
                bonusHP -= card.buffValue;
                status.HP = Mathf.Min(status.HP, MaxHP);
                break;

            case BuffType.Range:
                RangeBonus -= card.buffValue;
                break;

            case BuffType.CritChance:
                critChance -= card.buffValue * 0.01f;
                break;

            case BuffType.PassiveMultiplier:
                passiveMultiplier /= card.buffMultiplier;
                break;

            case BuffType.UseAttackBoost:
                useAttackBoost /= card.buffMultiplier;
                break;
        }

        Debug.Log($"バフ解除: {card.buffType} -{card.buffValue}");
    }

    public void TakeDamage(int amount)
    {
        status.TakeDamage(amount);
        Debug.Log($"プレイヤーHP: {status.HP}");

        OnHPChanged?.Invoke();

        if (status.IsDead())
        {
            Debug.Log("プレイヤー死亡！");
            // TODO: ゲームオーバー処理
        }
    }
}
