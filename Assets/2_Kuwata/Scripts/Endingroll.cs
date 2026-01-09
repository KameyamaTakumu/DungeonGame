using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Endingroll : MonoBehaviour
{
    Vector3 Staffrollposition;
    public RectTransform rectTransform;
    public float Endpos;

    public string returnSceneName = "Title"; // ñﬂÇËÇΩÇ¢ÉVÅ[Éìñº
    private bool isFinished = false;         // 1âÒÇæÇØé¿çsÇ∑ÇÈÇΩÇﬂ

    void Start()
    {
        Staffrollposition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        if (isFinished) return;

        if (rectTransform.anchoredPosition.y < Endpos)
        {
            Staffrollposition.y += 1f;
            rectTransform.anchoredPosition = Staffrollposition;
        }
        else
        {
            isFinished = true;
            SceneManager.LoadScene(returnSceneName);
        }
    }
}