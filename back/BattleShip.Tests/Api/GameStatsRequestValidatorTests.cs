using BattleShip.API.Grpc;
using BattleShip.API.Validation;

namespace BattleShip.Tests.Api;

public class GameStatsRequestValidatorTests
{
    private readonly GameStatsRequestValidator _validator = new();

    [Fact]
    public void Valide_Un_Identifiant_Guid_Correct()
    {
        var result = _validator.Validate(new GameStatsRequest { GameId = Guid.NewGuid().ToString() });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rejette_Un_Identifiant_Qui_Nest_Pas_Un_Guid()
    {
        var result = _validator.Validate(new GameStatsRequest { GameId = "pas-un-guid" });

        Assert.False(result.IsValid);
    }
}
