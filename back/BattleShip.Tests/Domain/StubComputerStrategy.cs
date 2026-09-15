using BattleShip.Models;

namespace BattleShip.Tests.Domain;

internal sealed class StubComputerStrategy(Coordinate target) : IComputerStrategy
{
    public Coordinate ChooseTarget(Board targetBoard) => target;
}
