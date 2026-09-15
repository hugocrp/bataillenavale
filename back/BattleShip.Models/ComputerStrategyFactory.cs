namespace BattleShip.Models;

public static class ComputerStrategyFactory
{
    public static IComputerStrategy Create(ComputerDifficulty difficulty, Random random) => difficulty switch
    {
        ComputerDifficulty.Easy => new RandomComputerStrategy(random),
        ComputerDifficulty.Hard => new HuntTargetComputerStrategy(random),
        ComputerDifficulty.Expert => new ExpertComputerStrategy(random),
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty))
    };
}
