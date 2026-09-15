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
                Assert.True(cell.IsWithin(Board.Size));
                Assert.True(occupied.Add(cell));
            }
        }
    }
}
