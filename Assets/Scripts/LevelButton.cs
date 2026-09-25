using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SceneNavigator.LoadLevelScene);
    }

    void Start()
    {
        label.text = LevelProgression.IsFinished ? "Finished" : $"Level {LevelProgression.CurrentLevel}";
        button.interactable = !LevelProgression.IsFinished;
    }
}
