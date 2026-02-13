using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStatus : MonoBehaviour
{
    // シングルトンインスタンス
    public static PlayerStatus instance;

    public BaseStatus status = new BaseStatus(20, 10, 1);

    // ★ 追加：シーンを跨いで保持するHP
    public static int SavedHP = -1;

    int bonusATK = 0;
    int bonusHP = 0;
    int bonusRange = 0;

    float critChance = 0f;
    float passiveMultiplier = 1f;
    float useAttackBoost = 1f;

    // float → int に変更
    int critChancePercent = 0;

    // 表示・計算用
    public float CritChance => critChancePercent * 0.01f;

    // ======================
    // ステータス取得
    // ======================
    public int Attack
        => status.ATK + Mathf.RoundToInt(bonusATK * passiveMultiplier);

    public int MaxHP
        => status.MAX_HP + Mathf.RoundToInt(bonusHP * passiveMultiplier);

    public int Range
        => status.RANGE + bonusRange + RangeBonus;

    //public float CritChance => critChance * passiveMultiplier;
    public float PassiveMultiplier => passiveMultiplier;
    public float UseAttackBoost => useAttackBoost;

    public int RangeBonus { get; private set; } = 0;

    public Action OnHPChanged;

    public Vector2Int facingDir = Vector2Int.down;

    public GameObject playerDamagePopupPrefab;

    void Awake()
    {
        instance = this;

        // ★ シーン遷移後なら復元
        if (SavedHP >= 0)
        {
            status.HP = Mathf.Min(SavedHP, MaxHP);
        }
    }

    private void Start()
    {
        CardInventory.Instance?.ReapplyAllPassiveEffects();

        // ★ その「あと」で HP を復元する
        if (SavedHP >= 0)
        {
            status.HP = Mathf.Min(SavedHP, MaxHP);
            OnHPChanged?.Invoke();
        }
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
                int beforeMax = MaxHP;   // 変更前の最大HPを保存

                bonusHP += card.buffValue;

                int afterMax = MaxHP;    // 変更後の最大HP

                // ★ 増えた差分だけ現在HPも回復
                int diff = afterMax - beforeMax;
                status.HP += diff;

                // 念のため上限クランプ
                status.HP = Mathf.Min(status.HP, afterMax);

                OnHPChanged?.Invoke();
                break;

            case BuffType.Range:
                bonusRange += card.buffValue;
                break;

            case BuffType.CritChance:
                //critChance += card.buffValue * 0.01f;
                critChancePercent += card.buffValue;
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
                //critChance -= card.buffValue * 0.01f;
                critChancePercent -= card.buffValue;
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
        SoundManager.Instance.PlaySE(SE.Heal);
        int heal = Mathf.RoundToInt(baseAmount * passiveMultiplier);
        status.HP = Mathf.Min(status.HP + heal, MaxHP);

        ShowDamagePopup(heal);
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
        ShowDamagePopup(amount);
        OnHPChanged?.Invoke();

        if (status.IsDead())
        {
            Debug.Log("プレイヤー死亡！");
            ResetStatusForNewGame();
            DungeonGenerator.CurrentFloor = 1;
            SceneManager.LoadScene("Title");
        }
    }

    void ShowDamagePopup(int damage)
    {
        if (playerDamagePopupPrefab == null) return;

        GameObject popup = Instantiate(
            playerDamagePopupPrefab,
            transform.position + Vector3.up,
            Quaternion.identity
        );

        popup.GetComponent<DamagePopup>().Setup(damage);
    }

    public void ResetStatusForNewGame()
    {
        // 保存HPを無効化
        SavedHP = -1;

        // バフ全解除
        bonusATK = 0;
        bonusHP = 0;
        bonusRange = 0;
        critChance = 0f;
        passiveMultiplier = 1f;
        useAttackBoost = 1f;
        RangeBonus = 0;

        // HPを最大に戻す
        status.HP = MaxHP;
        OnHPChanged?.Invoke();

        Debug.Log("プレイヤーステータスをリセット");
    }
}
