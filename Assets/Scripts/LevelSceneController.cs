using UnityEngine;

public class LevelSceneController : MonoBehaviour
{
    [SerializeField] private GridGenerator board;

    void Awake()
    {
        board.Initialize(LevelLoader.Load(LevelProgression.CurrentLevel));
    }

    [ContextMenu("Complete Level")]
    public void OnLevelWon()
    {
        LevelProgression.CompleteCurrentLevel();
        SceneNavigator.LoadMainScene();
    }
}
