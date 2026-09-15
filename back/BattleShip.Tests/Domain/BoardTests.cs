using BattleShip.Models;

namespace BattleShip.Tests.Domain;

public class BoardTests
{
    [Fact]
    public void PlaceShip_HorsGrille_Leve_Une_Exception()
    {
        var board = new Board();
        var ship = new Ship("Torpilleur", [new Coordinate(0, 9), new Coordinate(0, 10)]);

        Assert.Throws<InvalidOperationException>(() => board.PlaceShip(ship));
    }

    [Fact]
    public void PlaceShip_Chevauchement_Leve_Une_Exception()
    {
        var board = new Board();
        board.PlaceShip(new Ship("Torpilleur", [new Coordinate(0, 0), new Coordinate(0, 1)]));
        var overlapping = new Ship("Sous-marin", [new Coordinate(0, 1), new Coordinate(1, 1)]);

        Assert.Throws<InvalidOperationException>(() => board.PlaceShip(overlapping));
    }

    [Fact]
    public void ReceiveShot_Sur_Case_Vide_Retourne_Miss()
    {
        var board = new Board();
        board.PlaceShip(new Ship("Torpilleur", [new Coordinate(0, 0), new Coordinate(0, 1)]));

        var outcome = board.ReceiveShot(new Coordinate(5, 5));

        Assert.Equal(ShotOutcome.Miss, outcome);
    }

    [Fact]
    public void ReceiveShot_Touche_Puis_Coule_Le_Navire()
    {
        var board = new Board();
        board.PlaceShip(new Ship("Torpilleur", [new Coordinate(0, 0), new Coordinate(0, 1)]));

        var first = board.ReceiveShot(new Coordinate(0, 0));
        var second = board.ReceiveShot(new Coordinate(0, 1));

        Assert.Equal(ShotOutcome.Hit, first);
        Assert.Equal(ShotOutcome.Sunk, second);
        Assert.True(board.AllShipsSunk);
    }

    [Fact]
    public void ReceiveShot_Sur_Une_Case_Deja_Jouee_Ne_Modifie_Pas_La_Partie()
    {
        var board = new Board();
        board.PlaceShip(new Ship("Torpilleur", [new Coordinate(0, 0), new Coordinate(0, 1)]));
        board.ReceiveShot(new Coordinate(3, 3));

        var replay = board.ReceiveShot(new Coordinate(3, 3));

        Assert.Equal(ShotOutcome.AlreadyPlayed, replay);
        Assert.Equal(1, board.ShotsFired);
    }

    [Fact]
    public void GetState_Ne_Revele_Pas_Les_Navires_Adverses_Non_Decouverts()
    {
        var board = new Board();
        board.PlaceShip(new Ship("Torpilleur", [new Coordinate(0, 0), new Coordinate(0, 1)]));

        var hiddenState = board.GetState(new Coordinate(0, 0), revealShips: false);
        var revealedState = board.GetState(new Coordinate(0, 0), revealShips: true);

        Assert.Equal(CellState.Unknown, hiddenState);
        Assert.Equal(CellState.Ship, revealedState);
    }
}
