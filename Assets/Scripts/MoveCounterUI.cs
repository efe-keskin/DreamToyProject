using TMPro;
using UnityEngine;

public class MoveCounterUI : MonoBehaviour
{
    [SerializeField] private GridGenerator board;
    [SerializeField] private TextMeshProUGUI movesText;

    void OnEnable()
    {
        board.MovesChanged += UpdateText;
    }

    void OnDisable()
    {
        board.MovesChanged -= UpdateText;
    }

    void Start()
    {
        UpdateText(board.MovesRemaining);
    }

    private void UpdateText(int moves)
    {
        movesText.text = moves.ToString();
    }
}
