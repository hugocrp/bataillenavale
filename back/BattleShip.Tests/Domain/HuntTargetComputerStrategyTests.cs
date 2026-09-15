using BattleShip.Models;

namespace BattleShip.Tests.Domain;

public class HuntTargetComputerStrategyTests
{
    [Fact]
    public void ChooseTarget_Apres_Un_Tir_Touche_Vise_Une_Case_Adjacente()
    {
        var board = new Board();
        board.PlaceShip(new Ship("Croiseur", [new Coordinate(4, 4), new Coordinate(4, 5), new Coordinate(4, 6), new Coordinate(4, 7)]));
        board.ReceiveShot(new Coordinate(4, 5));

        var strategy = new HuntTargetComputerStrategy(new Random(1));
        var target = strategy.ChooseTarget(board);

        Coordinate[] expectedNeighbors =
        [
            new Coordinate(3, 5), new Coordinate(5, 5), new Coordinate(4, 4), new Coordinate(4, 6)
        ];
        Assert.Contains(target, expectedNeighbors);
    }

    [Fact]
    public void ChooseTarget_Ne_Vise_Jamais_Une_Case_Deja_Jouee()
    {
        var board = new Board();
        board.PlaceShip(new Ship("Torpilleur", [new Coordinate(0, 0), new Coordinate(0, 1)]));
        var strategy = new HuntTargetComputerStrategy(new Random(2));

        for (var i = 0; i < Board.DefaultSize * Board.DefaultSize; i++)
        {
            if (board.AllShipsSunk)
                break;

            var target = strategy.ChooseTarget(board);
            Assert.False(board.HasBeenShotAt(target));
            board.ReceiveShot(target);
        }

        Assert.True(board.AllShipsSunk);
    }

    [Fact]
    public void ChooseTarget_Reprend_La_Chasse_Une_Fois_Le_Navire_Touche_Coule()
    {
        var board = new Board();
        board.PlaceShip(new Ship("Torpilleur", [new Coordinate(0, 0), new Coordinate(0, 1)]));
        board.ReceiveShot(new Coordinate(0, 0));
        board.ReceiveShot(new Coordinate(0, 1));

        var strategy = new HuntTargetComputerStrategy(new Random(3));
        var target = strategy.ChooseTarget(board);

        Assert.True(target.IsWithin(Board.DefaultSize));
        Assert.False(board.HasBeenShotAt(target));
    }

    [Fact]
    public void ChooseTarget_Ignore_Les_Navires_Deja_Coules_Pour_Viser_Un_Autre_Navire_Touche()
    {
        var board = new Board();
        board.PlaceShip(new Ship("Torpilleur", [new Coordinate(0, 0), new Coordinate(0, 1)]));
        board.PlaceShip(new Ship("Sous-marin", [new Coordinate(9, 9), new Coordinate(8, 9), new Coordinate(7, 9)]));
        board.ReceiveShot(new Coordinate(0, 0));
        board.ReceiveShot(new Coordinate(0, 1));
        board.ReceiveShot(new Coordinate(9, 9));

        var strategy = new HuntTargetComputerStrategy(new Random(4));
        var target = strategy.ChooseTarget(board);

        Coordinate[] expectedNeighbors = [new Coordinate(8, 9), new Coordinate(9, 8)];
        Assert.Contains(target, expectedNeighbors);
    }

    [Fact]
    public void ChooseTarget_Quand_Toutes_Les_Cases_En_Damier_Sont_Jouees_Vise_Une_Case_Hors_Damier()
    {
        var board = new Board();
        for (var row = 0; row < Board.DefaultSize; row++)
        {
            for (var col = 0; col < Board.DefaultSize; col++)
            {
                if ((row + col) % 2 == 0)
                    board.ReceiveShot(new Coordinate(row, col));
            }
        }

        var strategy = new HuntTargetComputerStrategy(new Random(5));
        var target = strategy.ChooseTarget(board);

        Assert.Equal(1, (target.Row + target.Col) % 2);
        Assert.False(board.HasBeenShotAt(target));
    }
}
