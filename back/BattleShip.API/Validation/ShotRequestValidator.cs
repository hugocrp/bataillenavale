using BattleShip.Models;
using BattleShip.Models.Contracts;
using FluentValidation;

namespace BattleShip.API.Validation;

public sealed class ShotRequestValidator : AbstractValidator<ShotRequest>
{
    public ShotRequestValidator()
    {
        RuleFor(x => x.Row).InclusiveBetween(0, Board.Size - 1);
        RuleFor(x => x.Col).InclusiveBetween(0, Board.Size - 1);
    }
}
