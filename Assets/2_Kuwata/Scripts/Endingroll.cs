using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Endingroll : MonoBehaviour
{
    Vector3 Staffrollposition;
    public RectTransform rectTransform;
    public float Endpos;

    public string returnSceneName = "Title"; // –ß‚è‚½‚¢ƒV[ƒ“–¼
    private bool isFinished = false;         // 1‰ñ‚¾‚¯Às‚·‚é‚½‚ß

    public float scrollSpeed = 80f; // 1•b‚ ‚½‚è‚ÌˆÚ“®—Ê

    public SceneObject sceneObject;

    void Start()
    {
        Staffrollposition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        if (isFinished) return;

        if (Staffrollposition.y < Endpos)
        {
            Staffrollposition.y += scrollSpeed * Time.deltaTime;
            rectTransform.anchoredPosition = Staffrollposition;
        }
        else
        {
            isFinished = true;
            SceneManager.LoadScene(returnSceneName);
        }
    }

    public void OnclickToTitle()
    {
        SceneManager.LoadScene(sceneObject);
    }
}