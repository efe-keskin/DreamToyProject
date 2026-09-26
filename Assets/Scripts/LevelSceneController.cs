using UnityEngine;

public class LevelSceneController : MonoBehaviour
{
    [SerializeField] private GridGenerator board;
    [SerializeField] private WinCelebration winCelebration;
    [SerializeField] private FailPopup failPopup;

    private bool resultShown;

    void Awake()
    {
        board.Initialize(LevelLoader.Load(LevelProgression.CurrentLevel));
        board.BoardSettled += EvaluateResult;
    }

    void OnDestroy()
    {
        board.BoardSettled -= EvaluateResult;
    }

    private void EvaluateResult()
    {
        if (resultShown)
            return;

        if (!board.HasRemainingObstacles())
            Win();
        else if (board.MovesRemaining == 0)
            Lose();
    }

    private void Win()
    {
        resultShown = true;
        board.DisableInput();
        LevelProgression.CompleteCurrentLevel();
        winCelebration.Play(SceneNavigator.LoadMainScene);
    }

    private void Lose()
    {
        resultShown = true;
        board.DisableInput();
        failPopup.Show(SceneNavigator.LoadLevelScene, SceneNavigator.LoadMainScene);
    }

#if UNITY_EDITOR
    [ContextMenu("Debug/Force Win")]
    private void DebugForceWin()
    {
        if (!resultShown)
            Win();
    }

    [ContextMenu("Debug/Force Lose")]
    private void DebugForceLose()
    {
        if (!resultShown)
            Lose();
    }
#endif
}
