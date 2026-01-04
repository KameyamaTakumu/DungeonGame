using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro;


public class TitleManager : MonoBehaviour
{
    [SerializeField] GameObject optionPanel;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] GameObject tutorial;
    [SerializeField] GameObject titleRog;

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
        SceneManager.LoadScene("1F_Scene"); // "GameScene" の部分はシーンの名前に変更
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

    public void OnTutorialButton()
    {
        tutorial.SetActive(true);
        titleRog.SetActive(false);
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
