using System.Collections.Generic;
using UnityEngine;

public class GoalPanelUI : MonoBehaviour
{
    [System.Serializable]
    private class GoalIcon
    {
        [SerializeField] private GoalType type;
        [SerializeField] private Sprite sprite;

        public GoalType Type => type;
        public Sprite Sprite => sprite;
    }

    private const int ItemsPerRow = 2;

    [SerializeField] private GridGenerator board;
    [SerializeField] private GoalItemUI itemPrefab;
    [SerializeField] private GoalIcon[] icons;
    [SerializeField] private Vector2 spacing = new Vector2(110f, 110f);
    [SerializeField] private float singleGoalScale = 1.3f;

    private readonly Dictionary<GoalType, GoalItemUI> items = new Dictionary<GoalType, GoalItemUI>();

    void OnEnable()
    {
        board.BoardCreated += Build;
        board.Goals.GoalChanged += UpdateItem;
    }

    void OnDisable()
    {
        board.BoardCreated -= Build;
        board.Goals.GoalChanged -= UpdateItem;
    }

    void Start()
    {
        Build();
    }

    private void Build()
    {
        IReadOnlyList<GoalType> types = board.Goals.ActiveTypes;

        for (int i = 0; i < types.Count; i++)
        {
            GoalType type = types[i];

            if (!items.TryGetValue(type, out GoalItemUI item))
            {
                item = Instantiate(itemPrefab, transform);
                item.SetIcon(GetIcon(type));
                items[type] = item;
            }

            Place(item, i, types.Count);
            item.SetRemaining(board.Goals.GetRemaining(type));
        }
    }

    private void UpdateItem(GoalType type, int remaining)
    {
        if (items.TryGetValue(type, out GoalItemUI item))
            item.SetRemaining(remaining);
    }

    private void Place(GoalItemUI item, int index, int count)
    {
        RectTransform rect = (RectTransform)item.transform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = GetPosition(index, count);
        rect.localScale = Vector3.one * (count == 1 ? singleGoalScale : 1f);
    }

    private Vector2 GetPosition(int index, int count)
    {
        int row = index / ItemsPerRow;
        int column = index % ItemsPerRow;
        int rowCount = (count + ItemsPerRow - 1) / ItemsPerRow;
        int itemsInRow = Mathf.Min(ItemsPerRow, count - row * ItemsPerRow);

        float x = (column - (itemsInRow - 1) / 2f) * spacing.x;
        float y = ((rowCount - 1) / 2f - row) * spacing.y;
        return new Vector2(x, y);
    }

    private Sprite GetIcon(GoalType type)
    {
        foreach (GoalIcon icon in icons)
        {
            if (icon.Type == type)
                return icon.Sprite;
        }

        Debug.LogWarning($"No goal icon assigned for {type}", this);
        return null;
    }
}
