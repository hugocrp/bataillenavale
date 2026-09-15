namespace BattleShip.Models;

public static class FleetFactory
{
    public static readonly IReadOnlyList<(string Name, int Size)> StandardFleet =
    [
        ("Porte-avions", 5),
        ("Croiseur", 4),
        ("Contre-torpilleur", 3),
        ("Sous-marin", 3),
        ("Torpilleur", 2)
    ];

    public static Board CreateRandomBoard(Random random)
    {
        var board = new Board();

        foreach (var (name, size) in StandardFleet)
        {
            board.PlaceShip(CreateRandomShip(name, size, board, random));
        }

        return board;
    }

    private static Ship CreateRandomShip(string name, int size, Board board, Random random)
    {
        while (true)
        {
            var orientation = random.Next(2) == 0 ? Orientation.Horizontal : Orientation.Vertical;
            var originRow = random.Next(Board.Size);
            var originCol = random.Next(Board.Size);

            var cells = BuildCells(originRow, originCol, size, orientation);
            if (cells.Any(c => !c.IsWithin(Board.Size)))
                continue;

            if (cells.Any(c => board.Ships.Any(existing => existing.Occupies(c))))
                continue;

            return new Ship(name, cells);
        }
    }

    private static List<Coordinate> BuildCells(int row, int col, int size, Orientation orientation)
    {
        var cells = new List<Coordinate>(size);
        for (var i = 0; i < size; i++)
        {
            cells.Add(orientation == Orientation.Horizontal
                ? new Coordinate(row, col + i)
                : new Coordinate(row + i, col));
        }

        return cells;
    }
}
