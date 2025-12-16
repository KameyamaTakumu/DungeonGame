using UnityEngine;
using UnityEngine.UI;
public class HPBarController : MonoBehaviour
{
    [SerializeField] Slider hpSlider;
    [SerializeField] PlayerStatus playerStatus;
    void Start()
    {
        
        UpdateHPBar();
    }
    void Update()
    {
        UpdateHPBar();
    }

    void UpdateHPBar()
    {
        float ratio = (float)playerStatus.status.HP / playerStatus.MaxHP;
        hpSlider.value = ratio;
    }
}