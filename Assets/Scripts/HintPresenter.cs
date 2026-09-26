using System.Collections.Generic;

public class HintPresenter
{
    private readonly GridGenerator board;

    public HintPresenter(GridGenerator board)
    {
        this.board = board;
    }

    public void Refresh()
    {
        HashSet<Cube> visited = new HashSet<Cube>();

        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                if (board.GetItem(x, y) is not Cube cube || visited.Contains(cube))
                    continue;

                List<Cube> group = board.FindCubeGroup(cube);
                SpecialItemKind hint = SpecialItemFactory.GetKindForGroupSize(group.Count);

                foreach (Cube member in group)
                {
                    visited.Add(member);
                    member.ShowHint(hint);
                }
            }
        }
    }

    public void ClearGroup(IEnumerable<Cube> group)
    {
        foreach (Cube member in group)
            member.ShowHint(SpecialItemKind.None);
    }

    public void Clear()
    {
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                if (board.GetItem(x, y) is Cube cube)
                    cube.ShowHint(SpecialItemKind.None);
            }
        }
    }
}
