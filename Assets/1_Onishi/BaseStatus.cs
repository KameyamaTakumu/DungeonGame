using UnityEngine;
using System.Collections;

/// <summary>
/// キャラ・敵共通の基本ステータス
/// </summary>
[System.Serializable]
public class BaseStatus
{
    public int HP;       // 体力
    public int ATK;      // 攻撃力（ダメージになる）
    public int RANGE;    // 攻撃が届く距離（1なら1マス先のみ、2なら2マス先のみ）

    public BaseStatus(int hp, int atk, int range)
    {
        HP = hp;
        ATK = atk;
        RANGE = range;
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
