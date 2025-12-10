using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーおよび敵ユニットを一元管理するクラス。
/// 戦闘処理全体からアクセスされ、
/// 敵検索・プレイヤー検索・ユニット削除などを担当する。
///
/// 基本設計：
/// ・UnitManager はシーンに1つだけ存在するシングルトン
/// ・プレイヤーと敵のリストを保持し、
///   CombatManager などのシステムから参照される
/// </summary>
public class UnitManager : MonoBehaviour
{
    /// <summary>
    /// シングルトンインスタンス。
    /// 他のクラスから UnitManager に簡易アクセスするために使用。
    /// </summary>
    public static UnitManager instance;

    [Header("Unit Lists")]
    [Tooltip("シーン内のプレイヤーユニット一覧")]
    public List<GameObject> players = new List<GameObject>();

    [Tooltip("シーン内の敵ユニット一覧")]
    public List<GameObject> enemies = new List<GameObject>();


    private void Awake()
    {
        // シングルトンとして自身を登録
        instance = this;
    }


    /// <summary>
    /// プレイヤーをリストに登録する。
    /// 重複登録を防ぐため、存在チェックを行う。
    /// </summary>
    /// <param name="p">追加するプレイヤーオブジェクト</param>
    public void RegisterPlayer(GameObject p)
    {
        if (!players.Contains(p))
        {
            players.Add(p);
        }
    }


    /// <summary>
    /// 敵をリストに登録する。
    /// 重複登録を防止。
    /// </summary>
    /// <param name="e">追加する敵オブジェクト</param>
    public void RegisterEnemy(GameObject e)
    {
        if (!enemies.Contains(e))
        {
            enemies.Add(e);
        }
    }


    /// <summary>
    /// 渡されたオブジェクトをプレイヤー/敵リストの両方から削除する。
    /// ユニットが死亡した場合などに使用。
    /// </summary>
    /// <param name="obj">削除対象ユニット</param>
    public void RemoveUnit(GameObject obj)
    {
        players.Remove(obj);
        enemies.Remove(obj);
    }
}
