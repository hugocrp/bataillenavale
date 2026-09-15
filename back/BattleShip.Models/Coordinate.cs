namespace BattleShip.Models;

public sealed record Coordinate(int Row, int Col)
{
    public bool IsWithin(int size) => Row >= 0 && Row < size && Col >= 0 && Col < size;
}
