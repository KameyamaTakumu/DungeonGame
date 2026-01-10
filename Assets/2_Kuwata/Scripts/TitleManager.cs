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
    [SerializeField] GameObject tutorial2; 
    [SerializeField] GameObject tutorial3;
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
        // ★ フロア番号リセット
        DungeonGenerator.instance?.ResetFloorNumber();

        // ★ 抽選回数リセット
        DropSystem.ResetCardDropCount();

        // ★ インベントリ初期化
        CardInventory.Instance?.ResetInventory();

        // ★ プレイヤーステータス初期化
        //PlayerStatus.instance?.ResetStatusForNewGame();

        // Unitmanager初期化
        UnitManager.instance?.ResetUnitManagerForNewGame();

        SceneManager.LoadScene("1F_Scene"); // "GameScene" の部分はシーンの名前に変更
    }

    [SerializeField]GameObject GameObject;

    public void OnOptionButton()
    {
        GameObject.SetActive(true);

        //ChangeTitleColor();
    }

    public void OnBatuButton()
    {
        GameObject.SetActive(false);
    }

    public void OnTutorialButton()
    {
        tutorial.SetActive(true);
    }

    public void OnRightButton()
    {
        tutorial.SetActive(false);
        tutorial2.SetActive(true);

    }

    public void OnRightButton2()
    {
        tutorial2.SetActive(false);
        tutorial3.SetActive(true);

    }


    public void OnLeftButton()
    {
        tutorial.SetActive(true);
        tutorial2.SetActive(false);
    }

    public void OnLeftButton2()
    {
        tutorial2.SetActive(true);
        tutorial3.SetActive(false);
    }



    public void OnBatu2Bottun()
    {
        tutorial3.SetActive(false);
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
