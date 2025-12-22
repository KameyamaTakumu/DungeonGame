using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 追加
using TMPro;


public class TitleManager : MonoBehaviour
{
    [SerializeField] GameObject optionPanel;
    [SerializeField] TextMeshProUGUI titleText;

    int colorIndex = 0;
    Color[] colors = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
    };

    public void OnStartButton()
    {
        SceneManager.LoadScene("GameScene"); // "GameScene" の部分はシーンの名前に変更
    }
    [SerializeField]GameObject GameObject;

    public void OnOptionButton()
    {
        GameObject.SetActive(true);

        ChangeTitleColor();
    }

    public void OnBatuButton()
    {
        GameObject.SetActive(false);
    }

    void ChangeTitleColor()
    {
        titleText.color = colors[colorIndex];

        colorIndex++;
        if (colorIndex >= colors.Length)
        {
            colorIndex = 0;
        }
    }
}
