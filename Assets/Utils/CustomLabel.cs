/*
 * インスペクタに表示される変数名を好きな文字に置き換えるカスタムエディタ
 * CustomLabelAttribute.cs : Ver. 2.1.0
 * Written by Takashi Sowa
 * 
 * ▼使い方：以下のように記述すればインスペクタに表示される「variable」が「変数名」に置き換わる
 * [CustomLabel("変数名")]
 * public int variable = 0;//[SerializeField]を使用している場合でも利用可
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

public class CustomLabel : PropertyAttribute
{
    public readonly GUIContent Label;
    public CustomLabel(string label)
    {
        Label = new GUIContent(label);
    }
}

#if UNITY_EDITOR
// どのコンポーネントにも適用
[CanEditMultipleObjects]
[CustomEditor(typeof(UnityEngine.Object), true)]
public class CustomLabelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;

            // スクリプト参照フィールド（m_Script）はそのまま表示
            if (prop.name == "m_Script")
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(prop, true);
                GUI.enabled = true;
                continue;
            }

            // フィールド情報を取得
            var field = target.GetType().GetField(prop.name,
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.Instance);

            if (field != null)
            {
                var labelAttr = (CustomLabel[])field.GetCustomAttributes(typeof(CustomLabel), false);
                if (labelAttr.Length > 0)
                {
                    // フィールド名だけ置換（要素はUnity標準のまま描画される）
                    EditorGUILayout.PropertyField(prop, labelAttr[0].Label, true);
                    continue;
                }
            }

            // 通常の描画
            EditorGUILayout.PropertyField(prop, true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
