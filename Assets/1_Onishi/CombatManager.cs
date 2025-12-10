using UnityEngine;

/// <summary>
/// 戦闘時の判定処理をまとめた静的クラス。
/// プレイヤー・敵の位置関係を取得したり、
/// 指定方向の n マス先にいるユニットを検索するロジックを担当する。
///
/// 責務分離：
/// ・UnitManager：ユニット一覧の管理
/// ・CombatManager：戦闘判定（位置・当たり判定など）
///
/// SRPG / ローグライク等のマス計算を想定した汎用設計。
/// </summary>
public static class CombatManager
{
    /// <summary>
    /// 指定 origin（基準位置）から dir（方向）に distance マス進んだ位置に
    /// プレイヤー or 敵が存在するか調べ、見つかったオブジェクトを返す。
    ///
    /// direction: 右(Vector2Int.right), 左(Vector2Int.left),
    ///            上(Vector2Int.up), 下(Vector2Int.down) を想定。
    /// 
    /// 例：
    /// origin=(3,1), dir=(1,0), distance=2 → (5,1) をチェック
    /// </summary>
    /// <param name="origin">検索の起点となるグリッド座標</param>
    /// <param name="dir">進行方向ベクトル（整数）</param>
    /// <param name="distance">何マス先を調べるか</param>
    /// <returns>該当位置にいるユニット。存在しなければ null</returns>
    public static GameObject GetObjectInLine(Vector2Int origin, Vector2Int dir, int distance)
    {
        Vector2Int checkPos = origin + dir * distance;

        // プレイヤーをチェック
        foreach (var obj in UnitManager.instance.players)
        {
            if (IsAt(obj, checkPos))
                return obj;
        }

        // 敵をチェック
        foreach (var obj in UnitManager.instance.enemies)
        {
            if (IsAt(obj, checkPos))
                return obj;
        }

        return null;
    }


    /// <summary>
    /// GameObject が指定したグリッド座標と一致しているか判定する。
    /// transform.position を整数グリッドとして扱う形。
    /// </summary>
    private static bool IsAt(GameObject obj, Vector2Int pos)
    {
        Vector2 p = obj.transform.position;

        return Mathf.RoundToInt(p.x) == pos.x &&
               Mathf.RoundToInt(p.y) == pos.y;
    }


    /// <summary>
    /// 指定グリッドにプレイヤーが居るかどうかを簡易判定する。
    /// 主に敵 AI や攻撃判定で使用。
    /// </summary>
    public static bool IsPlayerAt(Vector2Int pos)
    {
        var p = GameObject.FindGameObjectWithTag("Player").transform.position;
        Vector2Int pg = Vector2Int.RoundToInt(p);

        return pg == pos;
    }
}
