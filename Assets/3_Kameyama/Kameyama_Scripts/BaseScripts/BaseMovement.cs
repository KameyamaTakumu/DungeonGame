using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D),typeof(BoxCollider2D))]
public class BaseMovement : MonoBehaviour
{
    [Header("1マスの大きさ")]
    public float cellSize = 1f;

    [Header("1マス移動の時間")]
    public float moveTime = 0.1f;

    [HideInInspector] public bool isMoving = false;

    protected DungeonGenerator dungeon;
    protected BattleManager bm;

    protected virtual void Awake()
    {
        dungeon = FindFirstObjectByType<DungeonGenerator>();
        bm = FindFirstObjectByType<BattleManager>();

        if (dungeon == null) Debug.LogError("DungeonGenerator がシーンにありません！");
        if (bm == null) Debug.LogError("BattleManager がシーンにありません！");
    }

    /// <summary>
    /// 移動可能チェック → OKなら MoveToCell を呼ぶ
    /// </summary>
    public virtual bool TryMove(int mx, int my, bool debugMove = false)
    {
        if (isMoving) return false;

        Vector3 pos = transform.position;
        int cx = Mathf.RoundToInt(pos.x);
        int cy = Mathf.RoundToInt(pos.y);

        int nx = cx + mx;
        int ny = cy + my;

        // 範囲外
        if (nx < 0 || ny < 0 || nx >= dungeon.width || ny >= dungeon.height)
            return false;

        // 壁
        if (dungeon.map[nx, ny] == TileType.Wall)
            return false;

        StartCoroutine(MoveToCell(new Vector2(nx, ny), debugMove));
        return true;
    }

    /// <summary>
    /// コルーチンで1マス分移動
    /// </summary>
    protected virtual IEnumerator MoveToCell(Vector2 target, bool debugMove = false)
    {
        isMoving = true;

        Vector3 start = transform.position;
        Vector3 end = new Vector3(target.x, target.y, start.z);

        float t = 0;
        while (t < moveTime)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, t / moveTime);
            yield return null;
        }

        transform.position = end;
        isMoving = false;

        OnMoveFinished(debugMove);
    }

    /// <summary>
    /// 派生クラスで移動後の処理を書き換える
    /// </summary>
    protected virtual void OnMoveFinished(bool debugMove) { }
}
