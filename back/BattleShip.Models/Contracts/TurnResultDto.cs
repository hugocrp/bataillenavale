namespace BattleShip.Models.Contracts;

public sealed record TurnResultDto(ShotResultDto PlayerShot, ShotResultDto? ComputerShot, GameStateDto State);
