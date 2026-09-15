namespace BattleShip.Models;

public sealed class RandomComputerStrategy(Random random) : IComputerStrategy
{
    public Coordinate ChooseTarget(Board targetBoard)
    {
        Coordinate candidate;
        do
        {
            candidate = new Coordinate(random.Next(targetBoard.Size), random.Next(targetBoard.Size));
        } while (targetBoard.HasBeenShotAt(candidate));

        return candidate;
    }
}
