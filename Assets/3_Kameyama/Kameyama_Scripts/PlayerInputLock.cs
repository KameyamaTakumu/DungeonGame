using UnityEngine;

/// <summary>
/// UI操作などでプレイヤー入力をロックするための管理クラス
/// </summary>
public class PlayerInputLock : MonoBehaviour
{
    public static PlayerInputLock Instance { get; private set; }

    /// <summary>
    /// UI操作などでプレイヤー入力がロックされているか
    /// </summary>
    public bool IsLocked { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Lock()
    {
        IsLocked = true;
    }

    public void Unlock()
    {
        IsLocked = false;
    }
}
