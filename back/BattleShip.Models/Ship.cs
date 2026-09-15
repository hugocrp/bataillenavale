namespace BattleShip.Models;

public sealed class Ship
{
    private readonly HashSet<Coordinate> _hits = [];

    public Ship(string name, IReadOnlyList<Coordinate> cells)
    {
        if (cells.Count == 0)
            throw new ArgumentException("Un navire doit occuper au moins une case.", nameof(cells));

        Name = name;
        Cells = cells;
    }

    public string Name { get; }
    public IReadOnlyList<Coordinate> Cells { get; }
    public int Size => Cells.Count;
    public bool IsSunk => _hits.Count == Cells.Count;

    public bool Occupies(Coordinate coordinate) => Cells.Contains(coordinate);

    public void RegisterHit(Coordinate coordinate)
    {
        if (!Occupies(coordinate))
            throw new InvalidOperationException("Cette case n'appartient pas au navire.");

        _hits.Add(coordinate);
    }
}
