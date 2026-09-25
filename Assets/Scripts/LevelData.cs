using System;
using UnityEngine;

[Serializable]
public class LevelData
{
    [SerializeField] private int level_number;
    [SerializeField] private int grid_width;
    [SerializeField] private int grid_height;
    [SerializeField] private int move_count;
    [SerializeField] private string[] grid;

    public int LevelNumber => level_number;
    public int Width => grid_width;
    public int Height => grid_height;
    public int MoveCount => move_count;
    public int CellCount => grid?.Length ?? 0;

    public string GetToken(int x, int y)
    {
        return grid[y * grid_width + x];
    }
}
