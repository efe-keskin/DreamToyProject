using UnityEditor;
using UnityEngine;

public static class ForceResultMenu
{
    [MenuItem("Tools/Force Win")]
    private static void ForceWin()
    {
        Object.FindAnyObjectByType<LevelSceneController>().DebugForceWin();
    }

    [MenuItem("Tools/Force Lose")]
    private static void ForceLose()
    {
        Object.FindAnyObjectByType<LevelSceneController>().DebugForceLose();
    }

    [MenuItem("Tools/Force Win", true)]
    [MenuItem("Tools/Force Lose", true)]
    private static bool IsLevelPlaying()
    {
        return Application.isPlaying && Object.FindAnyObjectByType<LevelSceneController>() != null;
    }
}
