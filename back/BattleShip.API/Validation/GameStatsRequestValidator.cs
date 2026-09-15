using BattleShip.API.Grpc;
using FluentValidation;

namespace BattleShip.API.Validation;

public sealed class GameStatsRequestValidator : AbstractValidator<GameStatsRequest>
{
    public GameStatsRequestValidator()
    {
        RuleFor(x => x.GameId)
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("L'identifiant de partie doit être un GUID valide.");
    }
}
