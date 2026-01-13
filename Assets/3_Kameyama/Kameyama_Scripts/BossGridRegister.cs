using UnityEngine;

public class BossGridRegister : MonoBehaviour
{
    Vector2Int gridPos;

    void Start()
    {
        gridPos = Vector2Int.RoundToInt(transform.position);
        UnitManager.instance.RegisterEnemy(gameObject);
    }

    void OnDestroy()
    {
        UnitManager.instance.UnregisterEnemy(GetComponent<EnemyMovement>());
    }
}
