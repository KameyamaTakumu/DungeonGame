using System;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public BaseStatus status = new BaseStatus(20, 10, 1);

    int bonusATK = 0;
    int bonusHP = 0;

    public int Attack => status.ATK + bonusATK;
    public int Range => status.RANGE;
    public int MaxHP => status.MAX_HP + bonusHP;

    public Action OnHPChanged;

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
