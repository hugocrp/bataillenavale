namespace BattleShip.Models;

public sealed class ExpertComputerStrategy(Random random) : IComputerStrategy
{
    public Coordinate ChooseTarget(Board targetBoard) =>
        ChooseTargetingMove(targetBoard) ?? ChooseByProbability(targetBoard);

    private static Coordinate? ChooseTargetingMove(Board targetBoard)
    {
        foreach (var ship in targetBoard.Ships)
        {
            if (ship.IsSunk || ship.Hits.Count == 0)
                continue;

            if (ship.Hits.Count >= 2)
            {
                var lineEnd = ChooseLineEnd(targetBoard, ship.Hits);
                if (lineEnd is not null)
                    return lineEnd;
            }

            foreach (var hit in ship.Hits)
            {
                foreach (var neighbor in Neighbors(hit))
                {
                    if (neighbor.IsWithin(targetBoard.Size) && !targetBoard.HasBeenShotAt(neighbor))
                        return neighbor;
                }
            }
        }

        return null;
    }

    private static Coordinate? ChooseLineEnd(Board targetBoard, IReadOnlyCollection<Coordinate> hits)
    {
        var ordered = hits.ToList();
        var sameRow = ordered.All(h => h.Row == ordered[0].Row);
        var sameCol = ordered.All(h => h.Col == ordered[0].Col);
        if (!sameRow && !sameCol)
            return null;

        ordered = sameRow ? ordered.OrderBy(h => h.Col).ToList() : ordered.OrderBy(h => h.Row).ToList();
        var first = ordered[0];
        var last = ordered[^1];
        var before = sameRow ? first with { Col = first.Col - 1 } : first with { Row = first.Row - 1 };
        var after = sameRow ? last with { Col = last.Col + 1 } : last with { Row = last.Row + 1 };

        foreach (var candidate in new[] { before, after })
        {
            if (candidate.IsWithin(targetBoard.Size) && !targetBoard.HasBeenShotAt(candidate))
                return candidate;
        }

        return null;
    }

    private Coordinate ChooseByProbability(Board targetBoard)
    {
        var remainingSizes = targetBoard.Ships.Where(s => !s.IsSunk).Select(s => s.Size).ToList();
        var scores = new Dictionary<Coordinate, int>();

        foreach (var size in remainingSizes)
        {
            for (var row = 0; row < targetBoard.Size; row++)
            {
                for (var col = 0; col < targetBoard.Size; col++)
                {
                    AccumulatePlacement(targetBoard, scores, row, col, size, rowStep: 0, colStep: 1);
                    AccumulatePlacement(targetBoard, scores, row, col, size, rowStep: 1, colStep: 0);
                }
            }
        }

        if (scores.Count == 0)
            return FallbackRandom(targetBoard);

        var best = scores.Values.Max();
        var candidates = scores.Where(kv => kv.Value == best).Select(kv => kv.Key).ToList();
        return candidates[random.Next(candidates.Count)];
    }

    private static void AccumulatePlacement(
        Board targetBoard, Dictionary<Coordinate, int> scores, int row, int col, int size, int rowStep, int colStep)
    {
        var cells = new List<Coordinate>(size);
        for (var i = 0; i < size; i++)
        {
            var cell = new Coordinate(row + rowStep * i, col + colStep * i);
            if (!cell.IsWithin(targetBoard.Size) || targetBoard.HasBeenShotAt(cell))
                return;

            cells.Add(cell);
        }

        foreach (var cell in cells)
            scores[cell] = scores.GetValueOrDefault(cell) + 1;
    }

    private Coordinate FallbackRandom(Board targetBoard)
    {
        Coordinate candidate;
        do
        {
            candidate = new Coordinate(random.Next(targetBoard.Size), random.Next(targetBoard.Size));
        } while (targetBoard.HasBeenShotAt(candidate));

        return candidate;
    }

    private static IEnumerable<Coordinate> Neighbors(Coordinate origin)
    {
        yield return origin with { Row = origin.Row - 1 };
        yield return origin with { Row = origin.Row + 1 };
        yield return origin with { Col = origin.Col - 1 };
        yield return origin with { Col = origin.Col + 1 };
    }
}
