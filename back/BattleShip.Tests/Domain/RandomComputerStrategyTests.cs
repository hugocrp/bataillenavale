using BattleShip.Models;

namespace BattleShip.Tests.Domain;

public class RandomComputerStrategyTests
{
    [Fact]
    public void ChooseTarget_Retourne_Une_Case_Dans_La_Grille()
    {
        var board = new Board();
        var strategy = new RandomComputerStrategy(new Random(1));

        var target = strategy.ChooseTarget(board);

        Assert.True(target.IsWithin(Board.DefaultSize));
    }

    [Fact]
    public void ChooseTarget_Ne_Vise_Jamais_Une_Case_Deja_Jouee()
    {
        var board = new Board();
        var strategy = new RandomComputerStrategy(new Random(2));

        for (var i = 0; i < Board.DefaultSize * Board.DefaultSize; i++)
        {
            var target = strategy.ChooseTarget(board);
            Assert.False(board.HasBeenShotAt(target));
            board.ReceiveShot(target);
        }
    }
}
