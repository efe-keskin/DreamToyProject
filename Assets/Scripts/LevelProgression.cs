using UnityEngine;

public static class LevelProgression
{
    public static int LevelCount => LevelLoader.LevelCount;

    private const string CurrentLevelKey = "CurrentLevel";
    private static int FinishedValue => LevelCount + 1;

    public static bool IsFinished => StoredLevel > LevelCount;

    public static int CurrentLevel => Mathf.Min(StoredLevel, LevelCount);

    public static void CompleteCurrentLevel()
    {
        if (!IsFinished)
            Save(StoredLevel + 1);
    }

    public static void SetCurrentLevel(int level)
    {
        Save(Mathf.Clamp(level, 1, FinishedValue));
    }

    private static int StoredLevel => Mathf.Clamp(PlayerPrefs.GetInt(CurrentLevelKey, 1), 1, FinishedValue);

    private static void Save(int level)
    {
        PlayerPrefs.SetInt(CurrentLevelKey, level);
        PlayerPrefs.Save();
    }
}
