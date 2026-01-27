using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBarController : MonoBehaviour
{
    [SerializeField] Slider hpSlider;

    [SerializeField] TextMeshProUGUI hpText;

    private PlayerStatus playerStatus;

    void Start()
    {
        playerStatus = FindFirstObjectByType<PlayerStatus>(); 
        hpSlider.minValue = 0;
        hpSlider.maxValue = playerStatus.MaxHP;

        playerStatus.OnHPChanged += UpdateHPBar;

        UpdateHPBar();
    }

    public void UpdateHPBar()
    {
        
        hpSlider.maxValue = playerStatus.MaxHP;
        hpSlider.value = playerStatus.status.HP;

        hpText.text = $"{playerStatus.status.HP} / {playerStatus.MaxHP}";
    }
}