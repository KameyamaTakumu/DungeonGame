using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager instance;

    public List<GameObject> players = new List<GameObject>();
    public List<GameObject> enemies = new List<GameObject>();

    void Awake()
    {
        instance = this;
    }

    public void RegisterPlayer(GameObject p)
    {
        if (!players.Contains(p))
            players.Add(p);
    }

    public void RegisterEnemy(GameObject e)
    {
        if (!enemies.Contains(e))
            enemies.Add(e);
    }

    public void RemoveUnit(GameObject obj)
    {
        players.Remove(obj);
        enemies.Remove(obj);
    }
}
