/*=========================================
 * 使い方はまず、BGMとSEのenumにそれぞれ追加してください。
 * そのあと、SoundManagerのInspector上で対応する場所に音を入れてください。
 * 
 * それで呼び出すときは、
 * 
 * SoundManager.Instance.PlayBGM(BGM.追加した名前);
 * SoundManager.Instance.PlaySE(SE.追加した名前);
 * 
 * と呼び出していただければ使用できます。
 *=========================================
*/
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

// BGMとSEのenumをここに追加していってください。
// 名前を追加すると自動的にSoundManagerのInspector上に表示されるようになります。
public enum BGM
{
    Test_BGM,
    Dungeon,
}

public enum SE
{
    Test_SE,
    Attack,
    critical,
    Heal,
    Death,
    CardUse,
    CardGet,
}
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM用AudioSource")]
    public AudioSource bgmSource; // BGM専用
    [Header("BGM用AudioSource")]
    public AudioSource seSource;  // SE専用

    [Header("BGMやSEをそれぞれ対応する場所に入れてください。")]
    [CustomLabel("BGM Clips")]
    public List<AudioClip> bgmList = new List<AudioClip>();
    [CustomLabel("SE Clips")]
    public List<AudioClip> seList = new List<AudioClip>();

    [Header("フェード設定")]
    [Range(0f, 5f)]
    public float fadeTime = 1f;

    private Coroutine fadeCoroutine;
    private int? currentBGMIndex = null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // BGM、SE再生
    public void PlayBGM(BGM index)
    {
        int idx = (int)index;
        if (currentBGMIndex == idx && bgmSource.isPlaying) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeBGM(idx));
    }

    private IEnumerator FadeBGM(int newIndex)
    {
        float t = 0f;

        // --- フェードアウト ---
        float initialVolume = bgmSource.volume;
        while (t < fadeTime)
        {
            bgmSource.volume = Mathf.Lerp(initialVolume, 0f, t / fadeTime);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        bgmSource.volume = 0f;
        bgmSource.Stop();

        // --- 新しいBGMを再生 ---
        if (newIndex < bgmList.Count && bgmList[newIndex] != null)
        {
            bgmSource.clip = bgmList[newIndex];
            bgmSource.volume = 0f; // 明示的にリセット
            bgmSource.Play();
            currentBGMIndex = newIndex;
        }
        else
        {
            fadeCoroutine = null;
            yield break;
        }

        // --- フェードイン ---
        t = 0f;
        while (t < fadeTime)
        {
            bgmSource.volume = Mathf.Lerp(0f, 1f, t / fadeTime);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        bgmSource.volume = 1f; // 最終音量を固定
        fadeCoroutine = null;
    }


    public void PlaySE(SE index)
    {
        int idx = (int)index;
        if (idx < seList.Count && seList[idx] != null)
        {
            seSource.PlayOneShot(seList[idx]);
        }
    }

    public void StopBGM(bool fade = true)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        if (fade)
            StartCoroutine(FadeOut());
        else
            bgmSource.Stop();

        currentBGMIndex = null;
    }

    private IEnumerator FadeOut()
    {
        float startVolume = bgmSource.volume;
        float t = 0;
        while (t < fadeTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.volume = startVolume;
    }

    // 音量設定
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSEVolume(float volume)
    {
        seSource.volume = Mathf.Clamp01(volume);
    }

    public float GetBGMVolume() => bgmSource.volume;
    public float GetSEVolume() => seSource.volume;
}
