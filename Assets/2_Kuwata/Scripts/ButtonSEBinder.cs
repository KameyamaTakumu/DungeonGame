using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSEBinder:MonoBehaviour
{
    [System.Serializable]
    public struct ButtonSEPair
    {
        public Button button;
        public SE seType;
    }

    [Header("ボタンと再生するSEの組み合わせ")]
    [SerializeField] private List<ButtonSEPair> buttonSEList = new List<ButtonSEPair>();

    private void Start()
    {
        foreach (var pair in buttonSEList)
        {
            if (pair.button != null)
            {
                pair.button.onClick.AddListener(() => PlaySE(pair.seType, pair.button.name));
            }
        }
    }

    private void PlaySE(SE seType, string buttonName)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(seType);
            Debug.Log($"[ButtonSEBinder] '{buttonName}' ボタンで {seType} を再生しました。");
        }
        else
        {
            Debug.LogWarning("[ButtonSEBinder] SoundManagerが見つかりません。");
        }
    }
}
