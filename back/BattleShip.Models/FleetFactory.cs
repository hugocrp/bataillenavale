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

    public static readonly IReadOnlyList<(string Name, int Size)> StormFleet =
    [
        ("Porte-avions", 5),
        ("Croiseur", 4),
        ("Croiseur II", 4),
        ("Contre-torpilleur", 3),
        ("Contre-torpilleur II", 3),
        ("Sous-marin", 3),
        ("Torpilleur", 2),
        ("Torpilleur II", 2)
    ];

    public const int StormBoardSize = 12;
    public const int StormMineCount = 3;

    public static Board CreateRandomBoard(Random random) => CreateRandomBoard(random, Board.DefaultSize, StandardFleet);

    public static Board CreateStormBoard(Random random)
    {
        var board = CreateRandomBoard(random, StormBoardSize, StormFleet);
        PlaceRandomMines(board, random, StormMineCount);
        return board;
    }

    private static Board CreateRandomBoard(Random random, int size, IReadOnlyList<(string Name, int Size)> fleet)
    {
        var board = new Board(size);

        foreach (var (name, shipSize) in fleet)
        {
            board.PlaceShip(CreateRandomShip(name, shipSize, board, random));
        }

        return board;
    }

    public static Board CreateManualBoard(IReadOnlyList<ShipPlacement> placements)
    {
        if (placements.Count != StandardFleet.Count)
            throw new InvalidOperationException($"La flotte doit compter exactement {StandardFleet.Count} navires.");

        var board = new Board();
        var remaining = StandardFleet.ToList();

        foreach (var placement in placements)
        {
            var index = remaining.FindIndex(s => s.Name == placement.Name);
            if (index < 0)
                throw new InvalidOperationException($"Navire inconnu ou déjà placé : « {placement.Name} ».");

            var (name, size) = remaining[index];
            remaining.RemoveAt(index);

            var cells = BuildCells(placement.Origin.Row, placement.Origin.Col, size, placement.Orientation);
            board.PlaceShip(new Ship(name, cells));
        }

        return board;
    }

    private static void PlaceRandomMines(Board board, Random random, int count)
    {
        var mines = new HashSet<Coordinate>();
        while (mines.Count < count)
        {
            var candidate = new Coordinate(random.Next(board.Size), random.Next(board.Size));
            if (board.Ships.Any(s => s.Occupies(candidate)) || !mines.Add(candidate))
                continue;

            board.PlaceMine(candidate);
        }
    }

    private static Ship CreateRandomShip(string name, int size, Board board, Random random)
    {
        while (true)
        {
            var orientation = random.Next(2) == 0 ? Orientation.Horizontal : Orientation.Vertical;
            var originRow = random.Next(board.Size);
            var originCol = random.Next(board.Size);

            var cells = BuildCells(originRow, originCol, size, orientation);
            if (cells.Any(c => !c.IsWithin(board.Size)))
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
