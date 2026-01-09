using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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

    [SerializeField] private TextMeshProUGUI bgmValueText;
    [SerializeField] private TextMeshProUGUI seValueText;

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

        // スライダーの初期化
        bgmSlider.onValueChanged.AddListener(value => bgmValueText.text = (value * 100).ToString("0"));
        seSlider.onValueChanged.AddListener(value => seValueText.text = (value * 100).ToString("0"));

        // 初期値設定
        bgmSlider.value = SoundManager.Instance.GetBGMVolume();
        seSlider.value = SoundManager.Instance.GetSEVolume();

        // リスナーを追加（Inspectorからは外す）
        bgmSlider.onValueChanged.AddListener(SoundManager.Instance.SetBGMVolume);
        seSlider.onValueChanged.AddListener(SoundManager.Instance.SetSEVolume);

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
            soundManager.PlaySE(SE.Test_SE);

        tutorialPanel1.SetActive(false);
        tutorialPanel2.SetActive(true);
    }

    public void CloseTutorial()
    {
        if (soundManager != null)
            soundManager.PlaySE(SE.Test_SE);

        tutorialPanel1.SetActive(false);
        tutorialPanel2.SetActive(false);
    }

    // ===== タイトルに戻る =====
    public void ReturnToTitle()
    {
        if (soundManager != null)
        {
            soundManager.PlaySE(SE.Test_SE);
            soundManager.StopBGM(true);
        }

        SceneManager.LoadScene("Title");
    }
}
