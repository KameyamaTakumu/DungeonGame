using System;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    // シングルトンインスタンス
    public static PlayerStatus instance;

    public BaseStatus status = new BaseStatus(20, 10, 1);

    int bonusATK = 0;
    int bonusHP = 0;
    int bonusRange = 0;

    float critChance = 0f;
    float passiveMultiplier = 1f;
    float useAttackBoost = 1f;

    // ======================
    // ステータス取得
    // ======================
    public int Attack
        => status.ATK + Mathf.RoundToInt(bonusATK * passiveMultiplier);

    public int MaxHP
        => status.MAX_HP + Mathf.RoundToInt(bonusHP * passiveMultiplier);

    public int Range
        => status.RANGE + bonusRange + RangeBonus;

    public float CritChance => critChance;
    public float PassiveMultiplier => passiveMultiplier;
    public float UseAttackBoost => useAttackBoost;

    public int RangeBonus { get; private set; } = 0;

    public Action OnHPChanged;

    public Vector2Int facingDir = Vector2Int.down;

    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        CardInventory.Instance?.ReapplyAllPassiveEffects();
    }

    // ======================
    // ★ 消費カード用最終攻撃力
    // ======================
    public int GetUseCardDamage(int cardBaseDamage)
    {
        int buffed = cardBaseDamage + bonusATK;
        buffed = Mathf.RoundToInt(buffed * passiveMultiplier);
        buffed = Mathf.RoundToInt(buffed * useAttackBoost);
        return buffed;
    }

    // ======================
    // バフ適用
    // ======================
    public void ApplyBuff(CardData card)
    {
        switch (card.buffType)
        {
            case BuffType.Attack:
                bonusATK += card.buffValue;
                break;

            case BuffType.HP:
                bonusHP += card.buffValue;
                //status.HP += card.buffValue; // 即時回復（倍率なし）
                OnHPChanged?.Invoke();
                break;

            case BuffType.Range:
                bonusRange += card.buffValue;
                break;

            case BuffType.CritChance:
                critChance += card.buffValue * 0.01f;
                break;

            case BuffType.PassiveMultiplier:
                passiveMultiplier *= card.buffMultiplier;
                RecalculateHPAfterMultiplier();
                break;

            case BuffType.UseAttackBoost:
                useAttackBoost *= card.buffMultiplier;
                break;
        }

        Debug.Log($"バフ適用: {card.buffType}");
    }

    // ======================
    // バフ解除
    // ======================
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
                OnHPChanged?.Invoke();
                break;

            case BuffType.Range:
                bonusRange -= card.buffValue;
                break;

            case BuffType.CritChance:
                critChance -= card.buffValue * 0.01f;
                break;

            case BuffType.PassiveMultiplier:
                passiveMultiplier /= card.buffMultiplier;
                RecalculateHPAfterMultiplier();
                break;

            case BuffType.UseAttackBoost:
                useAttackBoost /= card.buffMultiplier;
                break;
        }

        Debug.Log($"バフ解除: {card.buffType}");
    }

    // ======================
    // HP操作
    // ======================
    public void Heal(int baseAmount)
    {
        int heal = Mathf.RoundToInt(baseAmount * passiveMultiplier);
        status.HP = Mathf.Min(status.HP + heal, MaxHP);
        OnHPChanged?.Invoke();
    }

    void RecalculateHPAfterMultiplier()
    {
        status.HP = Mathf.Min(status.HP, MaxHP);
        OnHPChanged?.Invoke();
    }

    // ======================
    // ダメージ
    // ======================
    public void TakeDamage(int amount)
    {
        status.TakeDamage(amount);
        OnHPChanged?.Invoke();

        if (status.IsDead())
        {
            Debug.Log("プレイヤー死亡！");
        }
    }
}
