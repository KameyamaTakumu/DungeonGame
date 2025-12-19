using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeUI : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;

    [SerializeField] private TextMeshProUGUI bgmValueText;
    [SerializeField] private TextMeshProUGUI seValueText;

    private void Awake()
    {
        // スライダーの値が変わったときにテキストを更新
        bgmSlider.onValueChanged.AddListener(value => bgmValueText.text = (value * 100).ToString("0"));
        seSlider.onValueChanged.AddListener(value => seValueText.text = (value * 100).ToString("0"));
    }

    void Start()
    {
        // 初期値を反映（SoundManagerから取得）
        if (SoundManager.Instance != null)
        {
            bgmSlider.value = SoundManager.Instance.GetBGMVolume();
            seSlider.value = SoundManager.Instance.GetSEVolume();

            // スライダーの値が変わったときにテキストを更新
            bgmSlider.onValueChanged.AddListener(value => bgmValueText.text = (value * 100).ToString("0"));
            seSlider.onValueChanged.AddListener(value => seValueText.text = (value * 100).ToString("0"));

            // リスナーを追加（Inspectorからは外す）
            bgmSlider.onValueChanged.AddListener(SoundManager.Instance.SetBGMVolume);
            seSlider.onValueChanged.AddListener(SoundManager.Instance.SetSEVolume);
        }
        else
        {
            Debug.LogError("SoundManager.Instance が見つかりません！");
        }
    }
}
