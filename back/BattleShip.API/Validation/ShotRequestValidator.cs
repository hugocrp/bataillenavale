using BattleShip.Models;
using BattleShip.Models.Contracts;
using FluentValidation;

namespace BattleShip.API.Validation;

public sealed class ShotRequestValidator : AbstractValidator<ShotRequest>
{
    public ShotRequestValidator()
    {
        // Borne large : la taille exacte dépend du mode de la partie (10 en standard, 12 en Tempête).
        // La borne précise par partie est appliquée par Board.ReceiveShot, capturée dans l'endpoint.
        RuleFor(x => x.Row).InclusiveBetween(0, FleetFactory.StormBoardSize - 1);
        RuleFor(x => x.Col).InclusiveBetween(0, FleetFactory.StormBoardSize - 1);
    }
}
