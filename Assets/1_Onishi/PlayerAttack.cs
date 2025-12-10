using UnityEngine;

/// <summary>
/// プレイヤーの攻撃処理および攻撃予測（ハイライト表示）を管理するクラス。
/// グリッドベースの2Dゲームを想定し、プレイヤー位置を基準に
/// 指定方向の n マス先に攻撃を行う。
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("攻撃範囲を可視化するためのハイライトタイル（薄い黄色）")]
    public GameObject highlightPrefab;

    [Tooltip("攻撃を行う距離（例：1 → 1マス先に攻撃）")]
    public int attackRange = 1;

    // 現在表示中のハイライトインスタンス
    private GameObject currentHighlight;


    /// <summary>
    /// 指定方向へ攻撃を行う。
    /// </summary>
    /// <param name="dir">攻撃方向（上下左右）を示す Vector2Int</param>
    public void AttackForward(Vector2Int dir)
    {
        // プレイヤーの現在グリッド位置
        Vector2Int origin = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );

        // 指定方向に attackRange マス先のターゲット取得
        GameObject target = CombatManager.GetObjectInLine(origin, dir, attackRange);

        if (target != null)
        {
            Debug.Log($"敵 {target.name} に攻撃！");
            // TODO: ここにダメージ計算やノックバックなどの攻撃処理を記述
        }
        else
        {
            Debug.Log("攻撃は空振りしました。");
        }

        // 攻撃後はハイライトを消去
        ClearHighlight();
    }


    /// <summary>
    /// プレイヤーが攻撃方向を選択している時、
    /// 実際に攻撃が届くマスを視覚的に表示する。
    /// </summary>
    /// <param name="dir">攻撃方向</param>
    public void ShowHighlight(Vector2Int dir)
    {
        // 最新のハイライト以外は削除
        ClearHighlight();

        // 無方向の場合は生成しない
        if (dir == Vector2Int.zero) return;

        // プレイヤーの位置を基準に attackRange マス先を算出
        Vector2Int origin = Vector2Int.RoundToInt(transform.position);
        Vector2Int tilePos = origin + dir * attackRange;

        // 攻撃ターゲット地点にハイライトを生成
        currentHighlight = Instantiate(
            highlightPrefab,
            new Vector3(tilePos.x, tilePos.y, 0),
            Quaternion.identity
        );
    }


    /// <summary>
    /// 現在表示されている攻撃ハイライトを削除する。
    /// 攻撃実行時や方向変更時に呼び出される。
    /// </summary>
    public void ClearHighlight()
    {
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
            currentHighlight = null;
        }
    }
}
