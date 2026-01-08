using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public SceneObject sceneObject;

    public static SceneChanger Instance;

    void Awake()
    {
        Instance = this;
    }

    public void LoadNextFloor()
    {
        UnitManager.instance.ClearAllUnits();

        // Ÿ‚ÌƒV[ƒ“–¼‚É•ÏX‚·‚é
        SceneManager.LoadScene(sceneObject);
    }
}
