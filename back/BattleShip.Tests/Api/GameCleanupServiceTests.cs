using BattleShip.API;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace BattleShip.Tests.Api;

public class GameCleanupServiceTests
{
    [Fact]
    public async Task ExecuteAsync_Supprime_Les_Parties_Expirees_A_Chaque_Balayage()
    {
        var time = new FakeTimeProvider();
        var store = new GameStore(time);
        store.CreateRandom();
        time.Advance(GameCleanupService.MaxIdleTime + TimeSpan.FromMinutes(1));

        var service = new GameCleanupService(store, time, NullLogger<GameCleanupService>.Instance);
        await service.StartAsync(CancellationToken.None);
        try
        {
            // Laisse ExecuteAsync atteindre son premier « await » sur le PeriodicTimer
            // avant d'avancer l'horloge simulée, sans quoi le tick peut être manqué.
            await Task.Delay(50);
            time.Advance(GameCleanupService.SweepInterval);

            var deadline = DateTime.UtcNow.AddSeconds(2);
            while (store.Count > 0 && DateTime.UtcNow < deadline)
                await Task.Delay(10);

            Assert.Equal(0, store.Count);
        }
        finally
        {
            await service.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task ExecuteAsync_Ne_Supprime_Pas_Une_Partie_Encore_Active_Au_Balayage()
    {
        var time = new FakeTimeProvider();
        var store = new GameStore(time);
        var game = store.CreateRandom();

        var service = new GameCleanupService(store, time, NullLogger<GameCleanupService>.Instance);
        await service.StartAsync(CancellationToken.None);
        try
        {
            time.Advance(GameCleanupService.SweepInterval);
            await Task.Delay(100);

            Assert.True(store.TryGet(game.Id, out _));
        }
        finally
        {
            await service.StopAsync(CancellationToken.None);
        }
    }
}
