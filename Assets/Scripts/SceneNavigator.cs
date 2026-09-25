using UnityEngine.SceneManagement;

public static class SceneNavigator
{
    private const string MainSceneName = "MainScene";
    private const string LevelSceneName = "LevelScene";

    public static void LoadMainScene()
    {
        SceneManager.LoadScene(MainSceneName);
    }

    public static void LoadLevelScene()
    {
        SceneManager.LoadScene(LevelSceneName);
    }
}
