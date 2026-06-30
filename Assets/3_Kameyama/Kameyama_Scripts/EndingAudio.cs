using UnityEngine;

public class EndingAudio : MonoBehaviour
{
    void Start()
    {
        SoundManager.Instance.PlayBGM(BGM.Ending);
    }
}
