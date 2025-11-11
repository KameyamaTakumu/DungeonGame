/*
 * ▼使い方：以下のように記述すれば、インスペクタ上でシーンを選択できる * 
 *  public SceneObject SelectScene;// [SerializeField]を使用している場合でも利用可 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// シーンを管理するためのクラス
[System.Serializable]
public class SceneObject
{
    // 管理対象のシーン名
    [SerializeField]
    private string m_SceneName;

    // SceneObject から string へ暗黙変換（シーン名を返す）
    public static implicit operator string(SceneObject sceneObject)
    {
        return sceneObject.m_SceneName;
    }


    // string から SceneObject へ暗黙変換（シーン名を元に SceneObject を作成）
    public static implicit operator SceneObject(string sceneName)
    {
        return new SceneObject() { m_SceneName = sceneName };
    }
}

#if UNITY_EDITOR
// SceneObject をインスペクタで表示するためのカスタムプロパティ描画クラス
[CustomPropertyDrawer(typeof(SceneObject))]
public class SceneObjectEditor : PropertyDrawer
{
    // シーン名から SceneAsset を取得
    protected SceneAsset GetSceneObject(string sceneObjectName)
    {
        if (string.IsNullOrEmpty(sceneObjectName))
            return null;

        // BuildSettings に含まれているシーンを走査
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];

            // シーンパスにシーン名が含まれているか確認
            if (scene.path.IndexOf(sceneObjectName) != -1)
            {
                return AssetDatabase.LoadAssetAtPath(scene.path, typeof(SceneAsset)) as SceneAsset;
            }
        }

        // 見つからなかった場合は警告を出す
        Debug.Log("Scene [" + sceneObjectName + "] cannot be used. Add this scene to the 'Scenes in the Build' in the build settings.");
        return null;
    }

    // インスペクタの描画処理
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 現在のシーン名から SceneAsset を取得
        var sceneObj = GetSceneObject(property.FindPropertyRelative("m_SceneName").stringValue);

        // ObjectField でシーンアセットを選択できるようにする
        var newScene = EditorGUI.ObjectField(position, label, sceneObj, typeof(SceneAsset), false);
        if (newScene == null)
        {
            // シーンが選択されていない場合は空にする
            var prop = property.FindPropertyRelative("m_SceneName");
            prop.stringValue = "";
        }
        else
        {
            // シーン名が変更された場合のみ更新
            if (newScene.name != property.FindPropertyRelative("m_SceneName").stringValue)
            {
                var scnObj = GetSceneObject(newScene.name);

                // BuildSettings に含まれていないシーンの場合は警告を出す
                if (scnObj == null)
                {
                    Debug.LogWarning("The scene " + newScene.name + " cannot be used. To use this scene add it to the build settings for the project.");
                }
                else
                {
                    // シーン名を更新
                    var prop = property.FindPropertyRelative("m_SceneName");
                    prop.stringValue = newScene.name;
                }
            }
        }
    }
}
#endif
