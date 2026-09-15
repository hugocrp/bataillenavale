namespace BattleShip.Models;

public sealed class Game
{
    private readonly IComputerStrategy _computerStrategy;
    private bool _playerSkipsNextTurn;
    private bool _computerSkipsNextTurn;

    public Game(IComputerStrategy? computerStrategy = null, Random? random = null)
    {
        var rng = random ?? Random.Shared;
        PlayerBoard = FleetFactory.CreateRandomBoard(rng);
        ComputerBoard = FleetFactory.CreateRandomBoard(rng);
        _computerStrategy = computerStrategy ?? new HuntTargetComputerStrategy(rng);
    }

    public Game(Board playerBoard, IComputerStrategy? computerStrategy = null, Random? random = null)
    {
        var rng = random ?? Random.Shared;
        PlayerBoard = playerBoard;
        ComputerBoard = FleetFactory.CreateRandomBoard(rng);
        _computerStrategy = computerStrategy ?? new HuntTargetComputerStrategy(rng);
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

        if (!target.IsWithin(ComputerBoard.Size))
            throw new ArgumentOutOfRangeException(nameof(target), "La case visée est hors de la grille.");

        ShotResult playerShot;
        if (_playerSkipsNextTurn)
        {
            _playerSkipsNextTurn = false;
            playerShot = new ShotResult(target, ShotOutcome.TurnSkipped);
        }
        else
        {
            var playerOutcome = ComputerBoard.ReceiveShot(target);
            playerShot = new ShotResult(target, playerOutcome);

            if (playerOutcome == ShotOutcome.AlreadyPlayed)
                return new TurnResult(playerShot, null, Phase);

            if (playerOutcome == ShotOutcome.MineHit)
                _playerSkipsNextTurn = true;

            if (ComputerBoard.AllShipsSunk)
            {
                Phase = GamePhase.PlayerWon;
                return new TurnResult(playerShot, null, Phase);
            }
        }

        if (_computerSkipsNextTurn)
        {
            _computerSkipsNextTurn = false;
            return new TurnResult(playerShot, null, Phase);
        }

        var computerTarget = _computerStrategy.ChooseTarget(PlayerBoard);
        var computerOutcome = PlayerBoard.ReceiveShot(computerTarget);
        var computerShot = new ShotResult(computerTarget, computerOutcome);

        if (computerOutcome == ShotOutcome.MineHit)
            _computerSkipsNextTurn = true;

        if (PlayerBoard.AllShipsSunk)
            Phase = GamePhase.ComputerWon;

        return new TurnResult(playerShot, computerShot, Phase);
    }
}
