using BattleShip.Models;
using BattleShip.Models.Contracts;
using FluentValidation;

namespace BattleShip.API.Validation;

public sealed class CreateGameRequestValidator : AbstractValidator<CreateGameRequest>
{
    public CreateGameRequestValidator()
    {
        RuleFor(x => x.PlayerFleet)
            .Must(fleet => fleet!.Count == FleetFactory.StandardFleet.Count)
            .WithMessage($"La flotte doit compter exactement {FleetFactory.StandardFleet.Count} navires.")
            .When(x => x.PlayerFleet is not null);

        RuleForEach(x => x.PlayerFleet)
            .ChildRules(ship =>
            {
                ship.RuleFor(s => s.Row).InclusiveBetween(0, Board.DefaultSize - 1);
                ship.RuleFor(s => s.Col).InclusiveBetween(0, Board.DefaultSize - 1);
                ship.RuleFor(s => s.Orientation)
                    .Must(o => o is nameof(Orientation.Horizontal) or nameof(Orientation.Vertical))
                    .WithMessage("L'orientation doit être « Horizontal » ou « Vertical ».");
            })
            .When(x => x.PlayerFleet is not null);

        RuleFor(x => x.Difficulty)
            .Must(d => d is nameof(ComputerDifficulty.Easy) or nameof(ComputerDifficulty.Hard) or nameof(ComputerDifficulty.Expert))
            .WithMessage("La difficulté doit être « Easy », « Hard » ou « Expert ».")
            .When(x => x.Difficulty is not null);

        RuleFor(x => x)
            .Must(x => x.PlayerFleet is null)
            .WithMessage("Le placement manuel n'est pas disponible en Mode Tempête (grille et flotte différentes).")
            .When(x => x.StormMode);
    }
}
