using System;
using System.Collections.Generic;

public enum GoalType
{
    Vase,
    Stone,
    Chalice
}

public class LevelGoals
{
    private readonly Dictionary<GoalType, List<Obstacle>> obstaclesByType = new Dictionary<GoalType, List<Obstacle>>();
    private readonly Dictionary<GoalType, int> remainingByType = new Dictionary<GoalType, int>();
    private readonly List<GoalType> activeTypes = new List<GoalType>();

    public event Action<GoalType, int> GoalChanged;

    public IReadOnlyList<GoalType> ActiveTypes => activeTypes;

    public bool IsComplete
    {
        get
        {
            foreach (int remaining in remainingByType.Values)
            {
                if (remaining > 0)
                    return false;
            }

            return true;
        }
    }

    public int GetRemaining(GoalType type)
    {
        return remainingByType.TryGetValue(type, out int remaining) ? remaining : 0;
    }

    public void Register(Obstacle obstacle)
    {
        GoalType type = obstacle.GoalType;

        if (!obstaclesByType.TryGetValue(type, out List<Obstacle> obstacles))
        {
            obstacles = new List<Obstacle>();
            obstaclesByType[type] = obstacles;
            activeTypes.Add(type);
            activeTypes.Sort();
        }

        obstacles.Add(obstacle);
        remainingByType[type] = CountRemaining(obstacles);
    }

    public void Refresh(Obstacle obstacle)
    {
        GoalType type = obstacle.GoalType;
        if (!obstaclesByType.TryGetValue(type, out List<Obstacle> obstacles))
            return;

        if (obstacle.IsCleared)
            obstacles.Remove(obstacle);

        int remaining = CountRemaining(obstacles);
        if (remaining == remainingByType[type])
            return;

        remainingByType[type] = remaining;
        GoalChanged?.Invoke(type, remaining);
    }

    private static int CountRemaining(List<Obstacle> obstacles)
    {
        int remaining = 0;

        foreach (Obstacle obstacle in obstacles)
            remaining += obstacle.RemainingGoalCount;

        return remaining;
    }
}
