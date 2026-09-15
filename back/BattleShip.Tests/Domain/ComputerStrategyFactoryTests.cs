using BattleShip.Models;

namespace BattleShip.Tests.Domain;

public class ComputerStrategyFactoryTests
{
    [Fact]
    public void Easy_Retourne_Une_Strategie_Aleatoire()
    {
        var strategy = ComputerStrategyFactory.Create(ComputerDifficulty.Easy, new Random(1));

        Assert.IsType<RandomComputerStrategy>(strategy);
    }

    [Fact]
    public void Hard_Retourne_La_Strategie_Chasse_Puis_Cible()
    {
        var strategy = ComputerStrategyFactory.Create(ComputerDifficulty.Hard, new Random(1));

        Assert.IsType<HuntTargetComputerStrategy>(strategy);
    }

    [Fact]
    public void Expert_Retourne_La_Strategie_Probabiliste()
    {
        var strategy = ComputerStrategyFactory.Create(ComputerDifficulty.Expert, new Random(1));

        Assert.IsType<ExpertComputerStrategy>(strategy);
    }

    [Fact]
    public void Difficulte_Inconnue_Leve_Une_Exception()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ComputerStrategyFactory.Create((ComputerDifficulty)99, new Random(1)));
    }
}
