using BattleShip.API;
using Grpc.Core;

namespace BattleShip.API.Grpc;

public sealed class GameStatsGrpcService(GameStore store) : GameStats.GameStatsBase
{
    public override Task<GameStatsReply> GetStats(GameStatsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.GameId, out var id) || !store.TryGet(id, out var game))
            throw new RpcException(new Status(StatusCode.NotFound, "Partie inconnue."));

        return Task.FromResult(new GameStatsReply
        {
            Phase = game.Phase.ToString(),
            PlayerShipsRemaining = game.PlayerBoard.Ships.Count(s => !s.IsSunk),
            ComputerShipsRemaining = game.ComputerBoard.Ships.Count(s => !s.IsSunk),
            ShotsFiredByPlayer = game.ComputerBoard.ShotsFired
        });
    }
}
