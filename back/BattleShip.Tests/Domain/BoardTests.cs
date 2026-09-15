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

    [Fact]
    public void PlaceMine_Sur_Un_Navire_Leve_Une_Exception()
    {
        var board = new Board();
        board.PlaceShip(new Ship("Torpilleur", [new Coordinate(0, 0), new Coordinate(0, 1)]));

        Assert.Throws<InvalidOperationException>(() => board.PlaceMine(new Coordinate(0, 0)));
    }

    [Fact]
    public void PlaceMine_Deux_Fois_Sur_La_Meme_Case_Leve_Une_Exception()
    {
        var board = new Board();
        board.PlaceMine(new Coordinate(5, 5));

        Assert.Throws<InvalidOperationException>(() => board.PlaceMine(new Coordinate(5, 5)));
    }

    [Fact]
    public void ReceiveShot_Sur_Une_Mine_Retourne_MineHit_Et_Ne_Coule_Aucun_Navire()
    {
        var board = new Board();
        board.PlaceShip(new Ship("Torpilleur", [new Coordinate(0, 0), new Coordinate(0, 1)]));
        board.PlaceMine(new Coordinate(5, 5));

        var outcome = board.ReceiveShot(new Coordinate(5, 5));

        Assert.Equal(ShotOutcome.MineHit, outcome);
        Assert.False(board.AllShipsSunk);
    }

    [Fact]
    public void GetState_Ne_Revele_Pas_Une_Mine_Adverse_Non_Declenchee()
    {
        var board = new Board();
        board.PlaceMine(new Coordinate(2, 2));

        var hiddenState = board.GetState(new Coordinate(2, 2), revealShips: false);
        var revealedState = board.GetState(new Coordinate(2, 2), revealShips: true);

        Assert.Equal(CellState.Unknown, hiddenState);
        Assert.Equal(CellState.Mine, revealedState);
    }

    [Fact]
    public void GetState_Revele_Une_Mine_Declenchee_Meme_Sans_RevealShips()
    {
        var board = new Board();
        board.PlaceMine(new Coordinate(2, 2));
        board.ReceiveShot(new Coordinate(2, 2));

        var hiddenState = board.GetState(new Coordinate(2, 2), revealShips: false);
        var revealedState = board.GetState(new Coordinate(2, 2), revealShips: true);

        Assert.Equal(CellState.MineHit, hiddenState);
        Assert.Equal(CellState.MineHit, revealedState);
    }

    [Fact]
    public void Une_Mine_Deja_Declenchee_Ne_Peut_Pas_Etre_Retouchee()
    {
        var board = new Board();
        board.PlaceMine(new Coordinate(2, 2));
        board.ReceiveShot(new Coordinate(2, 2));

        var replay = board.ReceiveShot(new Coordinate(2, 2));

        Assert.Equal(ShotOutcome.AlreadyPlayed, replay);
    }

    [Fact]
    public void PlaceMine_HorsGrille_Leve_Une_Exception()
    {
        var board = new Board();

        Assert.Throws<InvalidOperationException>(() => board.PlaceMine(new Coordinate(0, 10)));
    }

    [Fact]
    public void ReceiveShot_HorsGrille_Leve_Une_Exception()
    {
        var board = new Board();

        Assert.Throws<ArgumentOutOfRangeException>(() => board.ReceiveShot(new Coordinate(10, 0)));
    }
}
