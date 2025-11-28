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

        // シーン中の全対象（敵・プレイヤー）を検索して位置を比較
        // 実際には GameManager が持つ List にすると高速（後述）
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Unit");

        foreach (var obj in allObjects)
        {
            Vector2 pos = obj.transform.position;

            if (Mathf.RoundToInt(pos.x) == checkPos.x &&
                Mathf.RoundToInt(pos.y) == checkPos.y)
            {
                return obj; // 見つかった
            }
        }

        return null;
    }
}

