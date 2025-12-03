/*
 * SoundManagerEditor.cs
 * 
 * 概要：
 *   SoundManager 用のカスタムエディタ。
 *   Enum（BGM, SE）に対応した AudioClip リストを自動的に整列・表示する。
 * 
 * 機能：
 *   - Enum の要素数に応じて AudioClip リストを自動補完／削除。
 *   - Enum 名をラベルとして AudioClip フィールドを表示。
 *   - BGM / SE セクションを見やすく分離して表示。
 * 
 * 使用例：
 *   1. SoundManager.cs に以下のような Enum とフィールドを定義しておく。
 *      public enum BGM { Title, Stage1, Boss }
 *      public enum SE  { Click, Explosion, Damage }
 *      [SerializeField] private List<AudioClip> bgmList;
 *      [SerializeField] private List<AudioClip> seList;
 * 
 *   2. 本スクリプトを "Editor" フォルダ内に配置すると、
 *      インスペクタ上で Enum に対応した AudioClip スロットが自動的に生成される。
 * 
 * 注意事項：
 *   - Enum の順番や要素名を変更した場合、リスト要素の対応関係も再生成されます。
 *   - このスクリプトは UnityEditor 環境でのみ動作します（ビルド時には含まれません）。
 *   - SoundManager 側のリストは SerializeField（または public）として定義しておく必要があります。
 */

using Codice.CM.Common;
using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// SoundManager クラス専用のカスタムエディタ。
/// Enum に応じた AudioClip リストを動的に整列・描画する。
/// </summary>
public class SoundManagerEditor : Editor
{
    /// <summary>SoundManager.bgmList フィールド</summary>
    private SerializedProperty bgmListProp;

    /// <summary>SoundManager.seList フィールド</summary>
    private SerializedProperty seListProp;

    /// <summary>
    /// インスペクタ初期化時に SerializedProperty をキャッシュ。
    /// </summary>
    private void OnEnable()
    {
        bgmListProp = serializedObject.FindProperty("bgmList");
        seListProp = serializedObject.FindProperty("seList");
    }

    /// <summary>
    /// カスタムインスペクタ描画処理。
    /// BGM / SE 各リストを Enum 名で見やすく表示する。
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 通常のプロパティ（AudioSourceなど）を先に描画
        DrawPropertiesExcluding(serializedObject, "bgmList", "seList");

        // --- BGMリストセクション ---
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("BGM 設定", EditorStyles.boldLabel);
        DrawEnumList<BGM>(bgmListProp);

        // --- SEリストセクション ---
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("SE 設定", EditorStyles.boldLabel);
        DrawEnumList<SE>(seListProp);

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Enum に対応した AudioClip リストを整列・描画する共通関数。
    /// </summary>
    /// <typeparam name="T">対応する Enum 型（例：BGM, SE）</typeparam>
    /// <param name="listProp">対象となる SerializedProperty（AudioClipリスト）</param>
    private void DrawEnumList<T>(SerializedProperty listProp) where T : Enum
    {
        // Enum の全要素を取得
        var enumValues = (T[])Enum.GetValues(typeof(T));

        // 要素数を Enum の数に合わせる
        while (listProp.arraySize < enumValues.Length)
            listProp.InsertArrayElementAtIndex(listProp.arraySize);

        // 余分な要素があれば削除
        while (listProp.arraySize > enumValues.Length)
            listProp.DeleteArrayElementAtIndex(listProp.arraySize);

        // 各 Enum 名をラベルとして AudioClip フィールドを描画
        for (int i = 0; i < enumValues.Length; i++)
        {
            var element = listProp.GetArrayElementAtIndex(i);
            string enumName = enumValues[i].ToString();
            EditorGUILayout.PropertyField(element, new GUIContent(enumName));
        }
    }
}
