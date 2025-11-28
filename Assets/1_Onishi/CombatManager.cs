using UnityEngine;

public static class CombatManager
{
    /// <summary>
    /// グリッド上で「origin → dir 方向に n マス先」にオブジェクトが存在するか調査する。
    /// </summary>
    /// <param name="origin">現在位置（整数グリッド）</param>
    /// <param name="dir">方向（例：Vector2Int(1,0)）</param>
    /// <param name="distance">調べるマス数</param>
    /// <returns>見つけたオブジェクト。いなければ null。</returns>
    public static GameObject GetObjectInLine(Vector2Int origin, Vector2Int dir, int distance)
    {
        Vector2Int checkPos = origin + dir * distance;

        // プレイヤーと敵の両方を調べる
        foreach (var obj in UnitManager.instance.players)
        {
            if (IsAt(obj, checkPos))
                return obj;
        }

        foreach (var obj in UnitManager.instance.enemies)
        {
            if (IsAt(obj, checkPos))
                return obj;
        }

        return null;
    }


    /// <summary>
    /// 
    private static bool IsAt(GameObject obj, Vector2Int pos)
    {
        Vector2 p = obj.transform.position;

        return Mathf.RoundToInt(p.x) == pos.x &&
               Mathf.RoundToInt(p.y) == pos.y;
    }

}

