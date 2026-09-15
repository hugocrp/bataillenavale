namespace BattleShip.Models;

public sealed record TurnResult(ShotResult PlayerShot, ShotResult? ComputerShot, GamePhase Phase);
