namespace BattleShip.Models.Contracts;

public sealed record ShipPlacementDto(string Name, int Row, int Col, string Orientation);
