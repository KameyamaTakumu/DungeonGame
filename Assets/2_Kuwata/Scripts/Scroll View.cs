using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScrollView : MonoBehaviour
{
    public GameObject DebugText;
    public GameObject DebugWindow;
    public int logcnt = 0;

    private TextMeshProUGUI _logText;

    void Awake()
    {
        Application.logMessageReceived += LoggedCb;  // ログ出力時のコールバックを登録
        _logText = DebugText.GetComponent<TextMeshProUGUI>();
    }

    // ▼▼▼ 追加: オブジェクトが破壊されたら、ログの購読を解除する ▼▼▼
    void OnDestroy()
    {
        Application.logMessageReceived -= LoggedCb;
    }

    // Start と Updateは省略

    public void LoggedCb(string logstr, string stacktrace, LogType type)
    {
        if (logcnt > 300)
        {
            int index = _logText.text.IndexOf("\n");
            _logText.text = _logText.text.Substring(index + 1);
        }
        else
        {
            logcnt++;
        }

        _logText.text += logstr;
        _logText.text += "\n";
        // 常にTextの最下部（最新）を表示するように強制スクロール
        DebugWindow.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
    }
}