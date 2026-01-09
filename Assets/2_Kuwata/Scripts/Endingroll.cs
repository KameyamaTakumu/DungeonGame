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

    // Start is called before the first frame update
    void Start()
    {
        Staffrollposition = rectTransform.anchoredPosition;
    }

    // Update is called once per frame
    void Update()
    {
        //if (isFinished) return;

        //if (Staffrollposition.y < Endpos)
        //{
        //    Staffrollposition.y += 0.8f;
        //    rectTransform.anchoredPosition = Staffrollposition;
        //}
        //else
        //{
        //    isFinished = true;
        //    SceneManager.LoadScene(returnSceneName);
        //}
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
}