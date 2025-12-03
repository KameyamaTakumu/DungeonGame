using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SpawnInfo))]
public class EnemyPrefabDropdownDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 全体の行を開始
        EditorGUI.BeginProperty(position, label, property);

        var prefabProperty = property.FindPropertyRelative("enemyPrefab");
        var countProperty = property.FindPropertyRelative("count");

        // 1行の高さ
        float lineHeight = EditorGUIUtility.singleLineHeight;

        // enemyPrefab 用の Rect
        Rect prefabRect = new Rect(position.x, position.y, position.width, lineHeight);
        // count 用の Rect
        Rect countRect = new Rect(position.x, position.y + lineHeight + 2, position.width, lineHeight);

        // EnemySpawnerController を探す
        EnemySpawnerController controller = property.serializedObject.targetObject as EnemySpawnerController;
        if (controller == null)
        {
            EditorGUI.PropertyField(prefabRect, prefabProperty);
        }
        else
        {
            var prefabList = controller.GetType()
                                       .GetField("enemyPrefabs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                       .GetValue(controller) as List<GameObject>;

            if (prefabList != null && prefabList.Count > 0)
            {
                string[] names = prefabList.ConvertAll(p => p != null ? p.name : "null").ToArray();

                int currentIndex = prefabList.IndexOf(prefabProperty.objectReferenceValue as GameObject);
                if (currentIndex < 0) currentIndex = 0;

                int newIndex = EditorGUI.Popup(prefabRect, "Enemy Type", currentIndex, names);
                prefabProperty.objectReferenceValue = prefabList[newIndex];
            }
            else
            {
                EditorGUI.PropertyField(prefabRect, prefabProperty);
            }
        }

        // count を通常通り描画
        EditorGUI.PropertyField(countRect, countProperty);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2 + 4;
    }
}
