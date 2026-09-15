namespace BattleShip.API;

public sealed class GameCleanupService(GameStore store, TimeProvider timeProvider, ILogger<GameCleanupService> logger) : BackgroundService
{
    public static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan MaxIdleTime = TimeSpan.FromMinutes(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(SweepInterval, timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var removed = store.RemoveExpired(MaxIdleTime);
            if (removed > 0)
                logger.LogInformation("Nettoyage : {Count} partie(s) inactive(s) supprimée(s).", removed);
        }
    }
}
