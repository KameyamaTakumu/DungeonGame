using UnityEngine;

public class PlayerSkillExecutor : MonoBehaviour
{
    public void ExecuteCardSkill(CardData card)
    {
        Debug.Log($"技発動：{card.cardName}");

        // ★ ここに技の内容を書く
        // 例：敵にダメージ、プレイヤーの回復、範囲攻撃など
    }
}
