using BattleShip.Models;

namespace BattleShip.Tests.Domain;

public class FleetFactoryTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(42)]
    [InlineData(1234)]
    public void CreateRandomBoard_Place_Toute_La_Flotte_Sans_Chevauchement_Ni_Debordement(int seed)
    {
        var board = FleetFactory.CreateRandomBoard(new Random(seed));

        Assert.Equal(FleetFactory.StandardFleet.Count, board.Ships.Count);

        var occupied = new HashSet<Coordinate>();
        foreach (var ship in board.Ships)
        {
            foreach (var cell in ship.Cells)
            {
                Assert.True(cell.IsWithin(Board.DefaultSize));
                Assert.True(occupied.Add(cell));
            }
        }
    }

    private static List<ShipPlacement> ValidPlacements() =>
    [
        new ShipPlacement("Porte-avions", new Coordinate(0, 0), Orientation.Horizontal),
        new ShipPlacement("Croiseur", new Coordinate(2, 0), Orientation.Horizontal),
        new ShipPlacement("Contre-torpilleur", new Coordinate(4, 0), Orientation.Horizontal),
        new ShipPlacement("Sous-marin", new Coordinate(6, 0), Orientation.Horizontal),
        new ShipPlacement("Torpilleur", new Coordinate(8, 0), Orientation.Horizontal)
    ];

    [Fact]
    public void CreateManualBoard_Avec_Placements_Valides_Place_Toute_La_Flotte()
    {
        var board = FleetFactory.CreateManualBoard(ValidPlacements());

        Assert.Equal(FleetFactory.StandardFleet.Count, board.Ships.Count);
        Assert.Equal(CellState.Ship, board.GetState(new Coordinate(0, 0), revealShips: true));
        Assert.Equal(CellState.Ship, board.GetState(new Coordinate(8, 1), revealShips: true));
    }

    [Fact]
    public void CreateManualBoard_Avec_Un_Nombre_De_Navires_Incorrect_Leve_Une_Exception()
    {
        var placements = ValidPlacements().Take(4).ToList();

        Assert.Throws<InvalidOperationException>(() => FleetFactory.CreateManualBoard(placements));
    }

    [Fact]
    public void CreateManualBoard_Avec_Un_Nom_De_Navire_Inconnu_Leve_Une_Exception()
    {
        var placements = ValidPlacements();
        placements[0] = placements[0] with { Name = "Sous-marin" };

        Assert.Throws<InvalidOperationException>(() => FleetFactory.CreateManualBoard(placements));
    }

    [Fact]
    public void CreateManualBoard_Avec_Chevauchement_Leve_Une_Exception()
    {
        var placements = ValidPlacements();
        placements[1] = placements[1] with { Origin = new Coordinate(0, 0) };

        Assert.Throws<InvalidOperationException>(() => FleetFactory.CreateManualBoard(placements));
    }

    [Fact]
    public void CreateManualBoard_Hors_Grille_Leve_Une_Exception()
    {
        var placements = ValidPlacements();
        placements[0] = placements[0] with { Origin = new Coordinate(0, 8) };

        Assert.Throws<InvalidOperationException>(() => FleetFactory.CreateManualBoard(placements));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(1234)]
    public void CreateStormBoard_Place_La_Flotte_Elargie_Et_Les_Mines_Sans_Chevauchement(int seed)
    {
        var board = FleetFactory.CreateStormBoard(new Random(seed));

        Assert.Equal(FleetFactory.StormBoardSize, board.Size);
        Assert.Equal(FleetFactory.StormFleet.Count, board.Ships.Count);

        var occupied = new HashSet<Coordinate>();
        foreach (var ship in board.Ships)
        {
            foreach (var cell in ship.Cells)
            {
                Assert.True(cell.IsWithin(board.Size));
                Assert.True(occupied.Add(cell));
            }
        }

        var minesRevealed = Enumerable.Range(0, board.Size)
            .SelectMany(row => Enumerable.Range(0, board.Size).Select(col => new Coordinate(row, col)))
            .Count(c => board.GetState(c, revealShips: true) == CellState.Mine);
        Assert.Equal(FleetFactory.StormMineCount, minesRevealed);
    }
}
