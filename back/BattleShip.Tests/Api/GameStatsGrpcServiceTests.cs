using BattleShip.API;
using BattleShip.API.Grpc;
using BattleShip.API.Validation;
using BattleShip.Models;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.Time.Testing;

namespace BattleShip.Tests.Api;

public class GameStatsGrpcServiceTests
{
    private static ServerCallContext CreateContext() =>
        TestServerCallContext.Create(
            method: "GetStats",
            host: null,
            deadline: DateTime.UtcNow.AddMinutes(1),
            requestHeaders: [],
            cancellationToken: CancellationToken.None,
            peer: "test",
            authContext: null,
            contextPropagationToken: null,
            writeHeadersFunc: _ => Task.CompletedTask,
            writeOptionsGetter: () => null,
            writeOptionsSetter: _ => { });

    [Fact]
    public async Task GetStats_Sur_Une_Partie_Existante_Retourne_Les_Compteurs_Attendus()
    {
        var store = new GameStore(new FakeTimeProvider());
        var game = store.CreateRandom();
        var service = new GameStatsGrpcService(store, new GameStatsRequestValidator());

        var reply = await service.GetStats(new GameStatsRequest { GameId = game.Id.ToString() }, CreateContext());

        Assert.Equal(nameof(GamePhase.InProgress), reply.Phase);
        Assert.Equal(FleetFactory.StandardFleet.Count, reply.PlayerShipsRemaining);
        Assert.Equal(FleetFactory.StandardFleet.Count, reply.ComputerShipsRemaining);
        Assert.Equal(0, reply.ShotsFiredByPlayer);
    }

    [Fact]
    public async Task GetStats_Avec_Un_Identifiant_Invalide_Leve_Une_RpcException_InvalidArgument()
    {
        var store = new GameStore(new FakeTimeProvider());
        var service = new GameStatsGrpcService(store, new GameStatsRequestValidator());

        var exception = await Assert.ThrowsAsync<RpcException>(
            () => service.GetStats(new GameStatsRequest { GameId = "pas-un-guid" }, CreateContext()));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task GetStats_Sur_Une_Partie_Inconnue_Leve_Une_RpcException_NotFound()
    {
        var store = new GameStore(new FakeTimeProvider());
        var service = new GameStatsGrpcService(store, new GameStatsRequestValidator());

        var exception = await Assert.ThrowsAsync<RpcException>(
            () => service.GetStats(new GameStatsRequest { GameId = Guid.NewGuid().ToString() }, CreateContext()));

        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }
}
