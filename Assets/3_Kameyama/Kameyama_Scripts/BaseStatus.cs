using UnityEngine;
using System.Collections;

/// <summary>
/// キャラ・敵共通の基本ステータス
/// </summary>
[System.Serializable]
public class BaseStatus
{
    public int HP;        // 現在HP
    public int MAX_HP;    // 最大HP
    public int ATK;      // 攻撃力（ダメージになる）
    public int RANGE;    // 攻撃が届く距離（1なら1マス先のみ、2なら2マス先のみ）

    public BaseStatus(int maxHp, int atk, int range)
    {
        MAX_HP = maxHp;
        HP = maxHp;   // 初期状態では最大HP
        ATK = atk;
        RANGE = range;
    }

    /// <summary>
    /// 回復（最大HPを超えない）
    /// </summary>
    public void Heal(int amount)
    {
        HP += amount;
        if (HP > MAX_HP) HP = MAX_HP;
    }

    /// <summary>
    /// ダメージを受ける
    /// </summary>
    public void TakeDamage(int amount)
    {
        HP -= amount;
        if (HP < 0) HP = 0;
    }

    /// <summary>
    /// 死亡判定
    /// </summary>
    public bool IsDead()
    {
        return HP <= 0;
    }
}
