using BattleShip.API;
using FluentValidation;
using Grpc.Core;

namespace BattleShip.API.Grpc;

public sealed class GameStatsGrpcService(GameStore store, IValidator<GameStatsRequest> validator) : GameStats.GameStatsBase
{
    public override async Task<GameStatsReply> GetStats(GameStatsRequest request, ServerCallContext context)
    {
        var validation = await validator.ValidateAsync(request, context.CancellationToken);
        if (!validation.IsValid)
        {
            var message = string.Join(" ", validation.Errors.Select(e => e.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, message));
        }

        var id = Guid.Parse(request.GameId);
        if (!store.TryGet(id, out var game))
            throw new RpcException(new Status(StatusCode.NotFound, "Partie inconnue."));

        return new GameStatsReply
        {
            Phase = game.Phase.ToString(),
            PlayerShipsRemaining = game.PlayerBoard.Ships.Count(s => !s.IsSunk),
            ComputerShipsRemaining = game.ComputerBoard.Ships.Count(s => !s.IsSunk),
            ShotsFiredByPlayer = game.ComputerBoard.ShotsFired
        };
    }
}
