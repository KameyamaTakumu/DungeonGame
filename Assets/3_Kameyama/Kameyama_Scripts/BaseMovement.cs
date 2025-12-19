using UnityEngine;
using System.Collections;

/// <summary>
/// グリッド（マス）単位での移動処理を提供する基底クラス。
/// Rigidbody2D および BoxCollider2D を必要とするため、
/// コンポーネントの付け忘れ防止として RequireComponent を付与。
/// </summary>
[RequireComponent(typeof(Rigidbody2D),typeof(BoxCollider2D))]
public class BaseMovement : MonoBehaviour
{
    [CustomLabel("1マスのサイズ")]
    public float cellSize = 1f;
    [CustomLabel("1マス移動にかかる時間")]
    public float moveTime = 0.1f;

    /// <summary>
    /// 移動コルーチン実行中かどうかのフラグ。
    /// 多重移動を防ぐため外部から参照可能。
    /// </summary>
    [HideInInspector] public bool isMoving = false;

    /// <summary>ダンジョン情報（地形・マップ）への参照。</summary>
    protected DungeonGenerator dungeon;
    /// <summary>バトル管理クラスへの参照。</summary>
    protected TurnManager    tm;

    /// <summary>
    /// 必要な依存コンポーネント（DungeonGenerator / BattleManager）の参照取得。
    /// いずれかが見つからない場合はデバッグログで通知。
    /// </summary>
    protected virtual void Awake()
    {
        dungeon = FindFirstObjectByType<DungeonGenerator>();
        tm      = FindFirstObjectByType<TurnManager>();

        if (dungeon == null) Debug.LogError("DungeonGenerator がシーン内に存在しません。"); 
        if (tm      == null) Debug.LogError("TurnManager がシーン内に存在しません。"); 
    }

    /// <summary>
    /// 指定方向へ移動可能か判定し、可能であれば移動処理を開始する。
    /// 成功時：true、移動不可または移動中：false を返す。
    /// </summary>
    /// <param name="mx">X方向の移動量（-1/0/1）</param>
    /// <param name="my">Y方向の移動量（-1/0/1）</param>
    /// <param name="debugMove">デバッグ用フラグ。派生クラス側で使用。</param>
    public virtual bool TryMove(int mx, int my, bool debugMove = false)
    {
        // 移動中は新たな移動を受け付けない
        if (isMoving) return false;

        // 現在の位置をグリッド座標へ変換
        Vector3 pos = transform.position;
        int cx = Mathf.RoundToInt(pos.x);
        int cy = Mathf.RoundToInt(pos.y);

        // 移動先（グリッド座標）
        int nx = cx + mx;
        int ny = cy + my;

        // マップ外チェック
        if (nx < 0 || ny < 0 || nx >= dungeon.width || ny >= dungeon.height)
        {
            return false;
        }

        // 壁タイルは移動不可
        if (dungeon.map[nx, ny] == TileType.Wall)
        {
            return false;
        }

        // コルーチンでの移動を開始
        StartCoroutine(MoveToCell(new Vector2(nx, ny), debugMove));
        return true;
    }

    /// <summary>
    /// 指定されたマス座標までの移動をコルーチンで実行する。
    /// t / moveTime を用いて補間するため、一定速度の移動となる。
    /// </summary>
    /// <param name="target">移動先のグリッド座標</param>
    /// <param name="debugMove">移動完了時のコールバック用フラグ</param>
    protected virtual IEnumerator MoveToCell(Vector2 target, bool debugMove = false)
    {
        isMoving = true;

        Vector3 start = transform.position;
        Vector3 end   = new Vector3(target.x, target.y, start.z);

        float t = 0;

        // moveTime 秒かけて線形補間で座標移動
        while (t < moveTime)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, t / moveTime);
            yield return null;
        }

        // 誤差を避けるため最終座標を明示的に設定
        transform.position = end;
        isMoving = false;

        // 移動完了時の追加処理（派生クラスでオーバーライド可能）
        OnMoveFinished(debugMove);
    }

    /// <summary>
    /// 移動完了時に呼び出されるフックメソッド。
    /// 派生クラスで独自の移動後処理を実装できるように設計。
    /// </summary>
    protected virtual void OnMoveFinished(bool debugMove) { }
}
