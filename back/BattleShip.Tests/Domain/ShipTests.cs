using BattleShip.Models;

namespace BattleShip.Tests.Domain;

public class ShipTests
{
    [Fact]
    public void Constructeur_Sans_Case_Leve_Une_Exception()
    {
        Assert.Throws<ArgumentException>(() => new Ship("Torpilleur", []));
    }

    [Fact]
    public void RegisterHit_Sur_Une_Case_Hors_Du_Navire_Leve_Une_Exception()
    {
        var ship = new Ship("Torpilleur", [new Coordinate(0, 0), new Coordinate(0, 1)]);

        Assert.Throws<InvalidOperationException>(() => ship.RegisterHit(new Coordinate(5, 5)));
    }

    [Fact]
    public void IsSunk_Est_Vrai_Quand_Toutes_Les_Cases_Sont_Touchees()
    {
        var ship = new Ship("Torpilleur", [new Coordinate(0, 0), new Coordinate(0, 1)]);

        ship.RegisterHit(new Coordinate(0, 0));
        Assert.False(ship.IsSunk);

        ship.RegisterHit(new Coordinate(0, 1));
        Assert.True(ship.IsSunk);
    }
}
