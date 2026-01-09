using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Staffroll:MonoBehaviour
{
    public AudioSource bgm;
    public RectTransform rectTransform;
    public float startY;
    public float endY;
    public float musicLength;

    double startDspTime;

    void Start()
    {
        startY = rectTransform.anchoredPosition.y;
        musicLength = bgm.clip.length;
        startDspTime = AudioSettings.dspTime;

        bgm.Play();
    }

    void Update()
    {
        double elapsed = AudioSettings.dspTime - startDspTime;
        float t = Mathf.Clamp01((float)(elapsed / musicLength));

        float y = Mathf.Lerp(startY, endY, t);
        rectTransform.anchoredPosition =
            new Vector2(rectTransform.anchoredPosition.x, y);

        if (t >= 1f)
        {
            SceneManager.LoadScene("Title");
        }
    }

}
