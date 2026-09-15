namespace BattleShip.Models;

public sealed class RandomComputerStrategy(Random random) : IComputerStrategy
{
    public Coordinate ChooseTarget(Board targetBoard)
    {
        Coordinate candidate;
        do
        {
            candidate = new Coordinate(random.Next(Board.Size), random.Next(Board.Size));
        } while (targetBoard.HasBeenShotAt(candidate));

        return candidate;
    }
}
