using UnityEngine;
using UnityEngine.UI;
public class HPBarController : MonoBehaviour
{
    public Slider hpSlider; // Sliderコンポーネント
    private int maxHp = 100; // 最大HP
    private int MaxHP; // 現在のHP
    void Start()
    {
        UpdateHPBar();
    }
    public void TakeDamage(int damage)
    {
        MaxHP -= damage;
        MaxHP = Mathf.Clamp(MaxHP, 0, maxHp); // HPが0未満にならないよう制限
        UpdateHPBar();
    }
    private void UpdateHPBar()
    {
        hpSlider.value = (float)MaxHP / maxHp; // HP割合をSliderに反映
    }
}