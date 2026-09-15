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

    [Fact]
    public void Constructeur_Avec_Flotte_Manuelle_Conserve_Le_Plateau_Joueur_Fourni_Et_Genere_Un_Plateau_Adverse_Complet()
    {
        var playerBoard = SingleShipBoard(new Coordinate(0, 0));

        var game = new Game(playerBoard, random: new Random(1));

        Assert.Same(playerBoard, game.PlayerBoard);
        Assert.Equal(FleetFactory.StandardFleet.Count, game.ComputerBoard.Ships.Count);
    }

    [Fact]
    public void Toucher_Une_Mine_Fait_Sauter_Le_Prochain_Tour_Du_Joueur_Sans_Annuler_La_Riposte_Du_Tour_En_Cours()
    {
        var playerBoard = SingleShipBoard(new Coordinate(9, 9));
        var computerBoard = SingleShipBoard(new Coordinate(8, 8));
        computerBoard.PlaceMine(new Coordinate(0, 0));
        var game = new Game(playerBoard, computerBoard, new StubComputerStrategy(new Coordinate(1, 1)));

        var mineTurn = game.Shoot(new Coordinate(0, 0));
        Assert.Equal(ShotOutcome.MineHit, mineTurn.PlayerShot.Outcome);
        Assert.NotNull(mineTurn.ComputerShot);

        var skippedTurn = game.Shoot(new Coordinate(5, 5));

        Assert.Equal(ShotOutcome.TurnSkipped, skippedTurn.PlayerShot.Outcome);
        Assert.False(computerBoard.HasBeenShotAt(new Coordinate(5, 5)));
        Assert.NotNull(skippedTurn.ComputerShot);
    }

    [Fact]
    public void Ordinateur_Qui_Touche_Une_Mine_Saute_Son_Tour_Suivant()
    {
        var playerBoard = SingleShipBoard(new Coordinate(8, 8));
        playerBoard.PlaceMine(new Coordinate(0, 0));
        var computerBoard = SingleShipBoard(new Coordinate(9, 9));
        var game = new Game(playerBoard, computerBoard, new StubComputerStrategy(new Coordinate(0, 0)));

        var mineTurn = game.Shoot(new Coordinate(5, 5));
        Assert.Equal(ShotOutcome.MineHit, mineTurn.ComputerShot!.Outcome);

        var skippedComputerTurn = game.Shoot(new Coordinate(6, 6));

        Assert.Equal(ShotOutcome.Miss, skippedComputerTurn.PlayerShot.Outcome);
        Assert.Null(skippedComputerTurn.ComputerShot);
    }

    [Fact]
    public void Quand_Lordinateur_Coule_Le_Dernier_Navire_Du_Joueur_La_Partie_Se_Termine_Sur_Une_Defaite()
    {
        var playerBoard = SingleShipBoard(new Coordinate(3, 3));
        var computerBoard = SingleShipBoard(new Coordinate(5, 5));
        var game = new Game(playerBoard, computerBoard, new StubComputerStrategy(new Coordinate(3, 3)));

        var turn = game.Shoot(new Coordinate(0, 0));

        Assert.Equal(ShotOutcome.Sunk, turn.ComputerShot!.Outcome);
        Assert.Equal(GamePhase.ComputerWon, game.Phase);
        Assert.True(playerBoard.AllShipsSunk);
    }

    [Fact]
    public void Shoot_Hors_De_La_Grille_De_Cette_Partie_Leve_Une_Exception_Meme_Pendant_Un_Tour_Saute()
    {
        var playerBoard = SingleShipBoard(new Coordinate(9, 9));
        var computerBoard = SingleShipBoard(new Coordinate(8, 8));
        computerBoard.PlaceMine(new Coordinate(0, 0));
        var game = new Game(playerBoard, computerBoard, new StubComputerStrategy(new Coordinate(1, 1)));
        game.Shoot(new Coordinate(0, 0));

        Assert.Throws<ArgumentOutOfRangeException>(() => game.Shoot(new Coordinate(42, 0)));
    }
}
