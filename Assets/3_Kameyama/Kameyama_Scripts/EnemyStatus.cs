using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    [System.Serializable]
    public class EnemyAttackData
    {
        public string attackName;
        public int damage;
        public int knockbackY;   // 0ならノックバックなし
        public int stunTurn;     // 0ならスタンなし
    }

    [Header("Enemy Base Status")]
    private DropSystem dropSystem;
    private PlayerInventory playerInventory;
    public BaseStatus status = new BaseStatus(10, 5, 1);
    int stunRemain = 0;

    private CardInventory cardInventory;

    public bool isBoss = false;


    private void Start()
    {
        // ※ シーン内にDropSystemは1つだけ付いている前提
        dropSystem = FindFirstObjectByType<DropSystem>();
        playerInventory = FindFirstObjectByType<PlayerInventory>();
        // ※ プレイヤーがシーンに1人いる前提

        cardInventory = FindFirstObjectByType<CardInventory>();

        if (dropSystem == null)
            Debug.LogError($"{name}: DropSystem が付いていません");

        if (playerInventory == null)
            Debug.LogError("PlayerInventory がシーンに存在しません");

        if (cardInventory == null)
            Debug.LogError("CardInventory がシーンに存在しません");
    }


    public void TakeDamage(int amount)
    {
        status.TakeDamage(amount);
        Debug.Log($"敵HP: {status.HP}");

        if (status.IsDead())
        {
            Debug.Log("敵死亡！");

            // ★ UnitManager のリストから削除
            UnitManager.instance.enemies.Remove(gameObject);

            Die();
        }
    }

    /// <summary>
    /// 攻撃データを受け取り、効果を適用する
    /// </summary>
    public void ApplyAttackEffect(EnemyAttackData attack)
    {
        Debug.Log($"攻撃受信: {attack.attackName}");

        TakeDamage(attack.damage);

        if (attack.stunTurn > 0)
        {
            ApplyStun(attack.stunTurn);
        }

        if (attack.knockbackY != 0)
        {
            transform.position += new Vector3(0, attack.knockbackY, 0);
        }
    }

    public void ApplyStun(int turn)
    {
        stunRemain = Mathf.Max(stunRemain, turn);
        Debug.Log($"{name} は {turn} ターン硬直！");
    }

    public bool IsStunned()
    {
        return stunRemain > 0;
    }

    //public void OnTurnStart()
    //{
    //    if (stunRemain > 0)
    //        stunRemain--;
    //}

    // ★ ターン開始時に呼ぶ
    public bool ConsumeStun()
    {
        if (stunRemain > 0)
        {
            stunRemain--;
            return true; // このターンは行動不能
        }
        return false;
    }

    /// <summary>
    /// 敵が死亡したときの処理
    /// </summary>
    private void Die()
    {
        Debug.Log($"{name} は倒れた！");

        if (isBoss)
        {
            HandleBossDefeated();
            return;
        }

        EnemyMovement mv = GetComponent<EnemyMovement>();
        if (mv != null)
        {
            UnitManager.instance.UnregisterEnemy(mv);
        }

        CardType? type = dropSystem.GetCardRewardType();

        if (!type.HasValue)
        {
            Debug.Log("カード報酬なし");
            Destroy(gameObject);
            return;
        }

        // ★ インベントリ満杯チェック
        if (IsInventoryFull(type.Value))
        {
            Debug.Log($"カードインベントリ満杯のため報酬スキップ: {type.Value}");
            Destroy(gameObject);
            return;
        }

        Debug.Log($"カード報酬タイプ: {type.Value}");

        // ★ ターン停止
        TurnManager.Instance.isWaitingCardSelect = true;

        var ui = FindFirstObjectByType<CardInventoryUIController>();
        if (ui != null)
            ui.ShowRandomSelect(type.Value);

        Destroy(gameObject);
    }

    void HandleBossDefeated()
    {
        // プレイヤー操作をロック
        TurnManager.Instance.isInputLocked = true;

        // ボスAI停止（念のため）
        var bossController = GetComponent<BossController>();
        if (bossController != null)
            bossController.enabled = false;

        // フェードアウト開始
        BossFadeOut fade = GetComponent<BossFadeOut>();
        if (fade != null)
        {
            fade.StartFadeOut();
        }
        else
        {
            Debug.LogError("BossFadeOut がアタッチされていません");
        }
    }


    bool IsInventoryFull(CardType type)
    {
        if (cardInventory == null) return true;

        if (type == CardType.Buff)
            return cardInventory.passiveCards.Count >= cardInventory.passiveLimit;

        return cardInventory.consumableCards.Count >= cardInventory.consumableLimit;
    }
}
