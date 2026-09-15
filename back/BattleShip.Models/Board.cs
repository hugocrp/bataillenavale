namespace BattleShip.Models;

public sealed class Board
{
    public const int Size = 10;

    private readonly List<Ship> _ships = [];
    private readonly HashSet<Coordinate> _shots = [];
    private readonly HashSet<Coordinate> _misses = [];

    public IReadOnlyList<Ship> Ships => _ships;
    public int ShotsFired => _shots.Count;
    public bool AllShipsSunk => _ships.Count > 0 && _ships.All(ship => ship.IsSunk);

    public void PlaceShip(Ship ship)
    {
        foreach (var cell in ship.Cells)
        {
            if (!cell.IsWithin(Size))
                throw new InvalidOperationException($"Le navire « {ship.Name} » déborde de la grille.");

            if (_ships.Any(existing => existing.Occupies(cell)))
                throw new InvalidOperationException($"Le navire « {ship.Name} » chevauche un autre navire.");
        }

        _ships.Add(ship);
    }

    public bool HasBeenShotAt(Coordinate coordinate) => _shots.Contains(coordinate);

    public ShotOutcome ReceiveShot(Coordinate target)
    {
        if (!target.IsWithin(Size))
            throw new ArgumentOutOfRangeException(nameof(target), "La case visée est hors de la grille.");

        if (!_shots.Add(target))
            return ShotOutcome.AlreadyPlayed;

        var ship = _ships.FirstOrDefault(s => s.Occupies(target));
        if (ship is null)
        {
            _misses.Add(target);
            return ShotOutcome.Miss;
        }

        ship.RegisterHit(target);
        return ship.IsSunk ? ShotOutcome.Sunk : ShotOutcome.Hit;
    }

    public CellState GetState(Coordinate coordinate, bool revealShips)
    {
        var ship = _ships.FirstOrDefault(s => s.Occupies(coordinate));
        if (ship is not null)
        {
            if (ship.IsSunk) return CellState.Sunk;
            if (_shots.Contains(coordinate)) return CellState.Hit;
            return revealShips ? CellState.Ship : CellState.Unknown;
        }

        return _misses.Contains(coordinate) ? CellState.Miss : CellState.Unknown;
    }
}
