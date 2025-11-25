using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    private List<GameObject> enemies = new List<GameObject>();

    public GameObject SpawnEnemy(Vector2Int pos)
    {
        GameObject enemy = Instantiate(enemyPrefab);
        enemy.transform.position = new Vector3(pos.x, pos.y, 0);
        enemies.Add(enemy);
        return enemy;
    }

    public IReadOnlyList<GameObject> GetEnemies() => enemies;
}
