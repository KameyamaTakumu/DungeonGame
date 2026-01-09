using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// <summary>
/// 音量設定・チュートリアル・タイトル戻りを管理するUI
/// </summary>
public class OptionMenuUI : MonoBehaviour
{
    [Header("オプションメニューパネル")]
    [SerializeField] private GameObject optionMenuPanel;

    [Header("音量設定パネル")]
    [SerializeField] private GameObject volumePanel;

    [Header("音量スライダー")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;

    [Header("チュートリアルパネル（2枚）")]
    [SerializeField] private GameObject tutorialPanel1;
    [SerializeField] private GameObject tutorialPanel2;

    private SoundManager soundManager;

    void Start()
    {
        // SoundManagerが存在しなければ何もしない
        soundManager = SoundManager.Instance;
        if (soundManager == null)
        {
            Debug.LogWarning("SoundManager が見つかりません");
            return;
        }

        // 初期値をSoundManagerから取得
        bgmSlider.value = soundManager.GetBGMVolume();
        seSlider.value = soundManager.GetSEVolume();

        tutorialPanel1.SetActive(false);
        tutorialPanel2.SetActive(false);
    }

    public void OpenPanel()
    {
        if (optionMenuPanel != null)
            optionMenuPanel.SetActive(true);

        PlayerInputLock.Instance.Lock();
    }

    public void ClosePanel()
    {
        if(optionMenuPanel != null)
            optionMenuPanel.SetActive(false);

        PlayerInputLock.Instance.Unlock();
    }

    // ===== 音量設定 =====
    public void OpenVolumePanel()
    {
        if(volumePanel != null)
            volumePanel.SetActive(true);
    }

    public void CloseVolumePanel()
    {
        if (volumePanel != null)
            volumePanel.SetActive(false);
    }

    public void OnChangeBGMVolume(float value)
    {
        if (soundManager == null) return;

        soundManager.SetBGMVolume(value);
        soundManager.PlaySE(SE.Test_SE);
    }

    public void OnChangeSEVolume(float value)
    {
        if (soundManager == null) return;

        soundManager.SetSEVolume(value);
        soundManager.PlaySE(SE.Test_SE);
    }

    // ===== チュートリアル =====
    public void OpenTutorial()
    {
        if (soundManager != null)
            soundManager.PlaySE(SE.CardUse);

        tutorialPanel1.SetActive(true);
        tutorialPanel2.SetActive(false);
    }

    public void NextTutorial()
    {
        if (soundManager != null)
            soundManager.PlaySE(SE.CardUse);

        tutorialPanel1.SetActive(false);
        tutorialPanel2.SetActive(true);
    }

    public void CloseTutorial()
    {
        if (soundManager != null)
            soundManager.PlaySE(SE.CardUse);

        tutorialPanel1.SetActive(false);
        tutorialPanel2.SetActive(false);
    }

    // ===== タイトルに戻る =====
    public void ReturnToTitle()
    {
        if (soundManager != null)
        {
            soundManager.PlaySE(SE.CardUse);
            soundManager.StopBGM(true);
        }

        SceneManager.LoadScene("Title");
    }
}
