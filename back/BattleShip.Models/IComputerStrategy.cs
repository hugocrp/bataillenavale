namespace BattleShip.Models;

public interface IComputerStrategy
{
    Coordinate ChooseTarget(Board targetBoard);
}
