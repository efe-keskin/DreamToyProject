using UnityEditor;
using UnityEngine;

public class SetCurrentLevelWindow : EditorWindow
{
    private int level = 1;
    private string confirmation;

    [MenuItem("Tools/Set Current Level")]
    private static void Open()
    {
        SetCurrentLevelWindow window = GetWindow<SetCurrentLevelWindow>(true, "Set Current Level");
        window.minSize = new Vector2(260f, 100f);
        window.level = LevelProgression.CurrentLevel;
    }

    private void OnGUI()
    {
        string saved = LevelProgression.IsFinished ? "Finished" : LevelProgression.CurrentLevel.ToString();
        EditorGUILayout.LabelField("Saved level", saved);

        level = Mathf.Clamp(EditorGUILayout.IntField("Level (1-" + LevelProgression.LevelCount + ")", level), 1, LevelProgression.LevelCount);

        if (GUILayout.Button("Apply"))
        {
            LevelProgression.SetCurrentLevel(level);
            confirmation = $"Current level set to {level}.";
            ShowNotification(new GUIContent(confirmation));
        }

        if (!string.IsNullOrEmpty(confirmation))
            EditorGUILayout.HelpBox(confirmation, MessageType.Info);
    }
}
