using System;
using UnityEngine;

public static class LevelLoader
{
    private const string LevelsFolder = "Levels";
    private const int MinGridSize = 6;
    private const int MaxGridSize = 10;

    public static LevelData Load(int levelNumber)
    {
        string path = $"{LevelsFolder}/level_{levelNumber:00}";
        TextAsset file = Resources.Load<TextAsset>(path);
        if (file == null)
            throw new FormatException($"Level file not found at Resources/{path}.json");

        LevelData level = JsonUtility.FromJson<LevelData>(file.text);
        Validate(level, levelNumber);
        return level;
    }

    private static void Validate(LevelData level, int levelNumber)
    {
        if (level.LevelNumber != levelNumber)
            throw Error(levelNumber, $"file says level_number {level.LevelNumber}");

        if (level.Width < MinGridSize || level.Width > MaxGridSize)
            throw Error(levelNumber, $"grid_width {level.Width} is outside the allowed range {MinGridSize}-{MaxGridSize}");

        if (level.Height < MinGridSize || level.Height > MaxGridSize)
            throw Error(levelNumber, $"grid_height {level.Height} is outside the allowed range {MinGridSize}-{MaxGridSize}");

        if (level.MoveCount <= 0)
            throw Error(levelNumber, $"invalid move_count {level.MoveCount}");

        if (level.CellCount != level.Width * level.Height)
            throw Error(levelNumber, $"grid has {level.CellCount} tokens, expected {level.Width * level.Height}");

        for (int y = 0; y < level.Height; y++)
        {
            for (int x = 0; x < level.Width; x++)
            {
                string token = level.GetToken(x, y);

                if (!LevelTokens.IsKnown(token))
                    throw Error(levelNumber, $"unknown token '{token}' at ({x},{y})");

                if (LevelTokens.TryGetChaliceBoxOffset(token, out Vector2Int offset))
                    ValidateChaliceBox(level, levelNumber, x - offset.x, y - offset.y, x, y);
            }
        }
    }

    private static void ValidateChaliceBox(LevelData level, int levelNumber, int anchorX, int anchorY, int x, int y)
    {
        for (int dx = 0; dx < 2; dx++)
        {
            for (int dy = 0; dy < 2; dy++)
            {
                int cellX = anchorX + dx;
                int cellY = anchorY + dy;
                bool inside = cellX >= 0 && cellX < level.Width && cellY >= 0 && cellY < level.Height;
                string token = inside ? level.GetToken(cellX, cellY) : null;

                if (!LevelTokens.TryGetChaliceBoxOffset(token, out Vector2Int offset) || offset != new Vector2Int(dx, dy))
                    throw Error(levelNumber, $"incomplete Chalice Box: token '{level.GetToken(x, y)}' at ({x},{y}) expects a matching part at ({cellX},{cellY})");
            }
        }
    }

    private static FormatException Error(int levelNumber, string message)
    {
        return new FormatException($"Level {levelNumber}: {message}");
    }
}
