namespace BattleShip.Models;

public sealed class Game
{
    private readonly IComputerStrategy _computerStrategy;

    public Game(IComputerStrategy? computerStrategy = null, Random? random = null)
    {
        var rng = random ?? Random.Shared;
        PlayerBoard = FleetFactory.CreateRandomBoard(rng);
        ComputerBoard = FleetFactory.CreateRandomBoard(rng);
        _computerStrategy = computerStrategy ?? new RandomComputerStrategy(rng);
    }

    public Game(Board playerBoard, Board computerBoard, IComputerStrategy computerStrategy)
    {
        PlayerBoard = playerBoard;
        ComputerBoard = computerBoard;
        _computerStrategy = computerStrategy;
    }

    public Guid Id { get; } = Guid.NewGuid();
    public Board PlayerBoard { get; }
    public Board ComputerBoard { get; }
    public GamePhase Phase { get; private set; } = GamePhase.InProgress;

    public TurnResult Shoot(Coordinate target)
    {
        if (Phase != GamePhase.InProgress)
            throw new InvalidOperationException("La partie est terminée : aucun nouveau coup n'est accepté.");

        var playerOutcome = ComputerBoard.ReceiveShot(target);
        var playerShot = new ShotResult(target, playerOutcome);

        if (playerOutcome == ShotOutcome.AlreadyPlayed)
            return new TurnResult(playerShot, null, Phase);

        if (ComputerBoard.AllShipsSunk)
        {
            Phase = GamePhase.PlayerWon;
            return new TurnResult(playerShot, null, Phase);
        }

        var computerTarget = _computerStrategy.ChooseTarget(PlayerBoard);
        var computerOutcome = PlayerBoard.ReceiveShot(computerTarget);
        var computerShot = new ShotResult(computerTarget, computerOutcome);

        if (PlayerBoard.AllShipsSunk)
            Phase = GamePhase.ComputerWon;

        return new TurnResult(playerShot, computerShot, Phase);
    }
}
