using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ボス撃破時のフェードアウト＆ED遷移
/// </summary>
public class BossFadeOut : MonoBehaviour
{
    [Header("フェード設定")]
    public float fadeDuration = 2.0f;
    public string edSceneName = "EDScene";

    SpriteRenderer[] renderers;

    void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void StartFadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    // フェードアウトコルーチン
    IEnumerator FadeOutCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            foreach (var r in renderers)
            {
                Color c = r.color;
                c.a = alpha;
                r.color = c;
            }

            yield return null;
        }

        SceneManager.LoadScene(edSceneName);
    }
}
