using UnityEngine;
using UnityEngine.UI;

public class BGMManager:MonoBehaviour
{
    void Start()
    {
        SoundManager.Instance.PlayBGM(BGM.Test_BGM);
    }
}
