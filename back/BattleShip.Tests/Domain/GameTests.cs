using BattleShip.Models;

namespace BattleShip.Tests.Domain;

public class GameTests
{
    private static Board SingleShipBoard(Coordinate cell) =>
        BuildBoard(new Ship("Torpilleur", [cell]));

    private static Board BuildBoard(params Ship[] ships)
    {
        var board = new Board();
        foreach (var ship in ships)
            board.PlaceShip(ship);
        return board;
    }

    [Fact]
    public void Shoot_Sur_Un_Manque_Declenche_Le_Tir_De_Lordinateur()
    {
        var playerBoard = SingleShipBoard(new Coordinate(9, 9));
        var computerBoard = SingleShipBoard(new Coordinate(9, 9));
        var game = new Game(playerBoard, computerBoard, new StubComputerStrategy(new Coordinate(0, 0)));

        var turn = game.Shoot(new Coordinate(0, 0));

        Assert.Equal(ShotOutcome.Miss, turn.PlayerShot.Outcome);
        Assert.NotNull(turn.ComputerShot);
        Assert.Equal(1, playerBoard.ShotsFired);
        Assert.Equal(GamePhase.InProgress, game.Phase);
    }

    [Fact]
    public void Shoot_Qui_Coule_Le_Dernier_Navire_Adverse_Termine_La_Partie_Sans_Riposte()
    {
        var playerBoard = SingleShipBoard(new Coordinate(5, 5));
        var computerBoard = SingleShipBoard(new Coordinate(2, 2));
        var game = new Game(playerBoard, computerBoard, new StubComputerStrategy(new Coordinate(0, 0)));

        var turn = game.Shoot(new Coordinate(2, 2));

        Assert.Equal(ShotOutcome.Sunk, turn.PlayerShot.Outcome);
        Assert.Null(turn.ComputerShot);
        Assert.Equal(GamePhase.PlayerWon, game.Phase);
        Assert.Equal(0, playerBoard.ShotsFired);
    }

    [Fact]
    public void Shoot_Sur_Case_Deja_Jouee_Ne_Compte_Pas_Comme_Un_Nouveau_Coup_Et_Ne_Declenche_Pas_De_Riposte()
    {
        var playerBoard = SingleShipBoard(new Coordinate(9, 9));
        var computerBoard = SingleShipBoard(new Coordinate(9, 9));
        var game = new Game(playerBoard, computerBoard, new StubComputerStrategy(new Coordinate(0, 0)));
        game.Shoot(new Coordinate(1, 1));
        var shotsAfterFirstTurn = playerBoard.ShotsFired;

        var replay = game.Shoot(new Coordinate(1, 1));

        Assert.Equal(ShotOutcome.AlreadyPlayed, replay.PlayerShot.Outcome);
        Assert.Null(replay.ComputerShot);
        Assert.Equal(shotsAfterFirstTurn, playerBoard.ShotsFired);
    }

    [Fact]
    public void Shoot_Apres_La_Fin_De_Partie_Leve_Une_Exception()
    {
        var playerBoard = SingleShipBoard(new Coordinate(5, 5));
        var computerBoard = SingleShipBoard(new Coordinate(2, 2));
        var game = new Game(playerBoard, computerBoard, new StubComputerStrategy(new Coordinate(0, 0)));
        game.Shoot(new Coordinate(2, 2));

        Assert.Throws<InvalidOperationException>(() => game.Shoot(new Coordinate(3, 3)));
    }
}
