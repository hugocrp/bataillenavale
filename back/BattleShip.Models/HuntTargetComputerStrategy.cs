namespace BattleShip.Models;

public sealed class HuntTargetComputerStrategy(Random random) : IComputerStrategy
{
    public Coordinate ChooseTarget(Board targetBoard)
    {
        var adjacentTargets = FindAdjacentTargets(targetBoard);
        if (adjacentTargets.Count > 0)
            return adjacentTargets[random.Next(adjacentTargets.Count)];

        return ChooseHuntTarget(targetBoard);
    }

    private static List<Coordinate> FindAdjacentTargets(Board targetBoard)
    {
        var targets = new List<Coordinate>();

        foreach (var ship in targetBoard.Ships)
        {
            if (ship.IsSunk)
                continue;

            foreach (var hit in ship.Hits)
            {
                foreach (var neighbor in Neighbors(hit))
                {
                    if (neighbor.IsWithin(targetBoard.Size) && !targetBoard.HasBeenShotAt(neighbor) && !targets.Contains(neighbor))
                        targets.Add(neighbor);
                }
            }
        }

        return targets;
    }

    private static IEnumerable<Coordinate> Neighbors(Coordinate origin)
    {
        yield return origin with { Row = origin.Row - 1 };
        yield return origin with { Row = origin.Row + 1 };
        yield return origin with { Col = origin.Col - 1 };
        yield return origin with { Col = origin.Col + 1 };
    }

    private Coordinate ChooseHuntTarget(Board targetBoard)
    {
        var parityTargets = new List<Coordinate>();
        for (var row = 0; row < targetBoard.Size; row++)
        {
            for (var col = 0; col < targetBoard.Size; col++)
            {
                var candidate = new Coordinate(row, col);
                if ((row + col) % 2 == 0 && !targetBoard.HasBeenShotAt(candidate))
                    parityTargets.Add(candidate);
            }
        }

        if (parityTargets.Count > 0)
            return parityTargets[random.Next(parityTargets.Count)];

        Coordinate fallback;
        do
        {
            fallback = new Coordinate(random.Next(targetBoard.Size), random.Next(targetBoard.Size));
        } while (targetBoard.HasBeenShotAt(fallback));

        return fallback;
    }
}
