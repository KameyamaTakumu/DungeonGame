using UnityEngine;
using UnityEngine.UI;
public class HPBarController : MonoBehaviour
{
    [SerializeField] Slider hpSlider;
    private PlayerStatus playerStatus;
    void Start()
    {
        playerStatus = FindFirstObjectByType<PlayerStatus>(); 
        hpSlider.minValue = 0;
        hpSlider.maxValue = playerStatus.status.MAX_HP;

        playerStatus.OnHPChanged += UpdateHPBar;

        UpdateHPBar();
    }
    //void Update()
    //{
    //    UpdateHPBar();
    //}

    void UpdateHPBar()
    {
        hpSlider.value = playerStatus.status.HP ;
    }
}