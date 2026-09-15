using BattleShip.API;
using BattleShip.API.Grpc;
using BattleShip.API.Validation;
using BattleShip.Models;
using BattleShip.Models.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context => context.ProblemDetails.Extensions.Remove("exception"));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<GameStore>();
builder.Services.AddHostedService<GameCleanupService>();
builder.Services.AddScoped<IValidator<ShotRequest>, ShotRequestValidator>();
builder.Services.AddScoped<IValidator<CreateGameRequest>, CreateGameRequestValidator>();
builder.Services.AddScoped<IValidator<GameStatsRequest>, GameStatsRequestValidator>();
builder.Services.AddGrpc();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["https://localhost:7068", "http://localhost:5109"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Front", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Front");
app.UseGrpcWeb();

app.MapGrpcService<GameStatsGrpcService>().EnableGrpcWeb().RequireCors("Front");

app.MapGet("/fleet", Ok<FleetSlotDto[]> () =>
    TypedResults.Ok(FleetFactory.StandardFleet.Select(s => new FleetSlotDto(s.Name, s.Size)).ToArray()));

app.MapPost("/games", async Task<Results<Created<GameStateDto>, ValidationProblem>> (
    CreateGameRequest request,
    GameStore store,
    IValidator<CreateGameRequest> validator) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid)
        return TypedResults.ValidationProblem(validation.ToDictionary());

    var difficulty = request.Difficulty is null
        ? ComputerDifficulty.Hard
        : Enum.Parse<ComputerDifficulty>(request.Difficulty);

    if (request.StormMode)
    {
        var stormGame = store.CreateStorm(difficulty);
        return TypedResults.Created($"/games/{stormGame.Id}", GameStateMapper.ToDto(stormGame));
    }

    if (request.PlayerFleet is null)
    {
        var randomGame = store.CreateRandom(difficulty);
        return TypedResults.Created($"/games/{randomGame.Id}", GameStateMapper.ToDto(randomGame));
    }

    Board playerBoard;
    try
    {
        var placements = request.PlayerFleet
            .Select(s => new ShipPlacement(s.Name, new Coordinate(s.Row, s.Col), Enum.Parse<Orientation>(s.Orientation)))
            .ToList();
        playerBoard = FleetFactory.CreateManualBoard(placements);
    }
    catch (InvalidOperationException ex)
    {
        return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["playerFleet"] = [ex.Message] });
    }

    var game = store.CreateWithPlayerFleet(playerBoard, difficulty);
    return TypedResults.Created($"/games/{game.Id}", GameStateMapper.ToDto(game));
});

app.MapGet("/games", Ok<GameSummaryDto[]> (GameStore store) =>
    TypedResults.Ok(store.ListAll()
        .Select(s => new GameSummaryDto(s.Id, s.Phase.ToString(), s.Difficulty.ToString(), s.StormMode, s.LastActivityUtc))
        .ToArray()));

app.MapGet("/stats", Ok<GlobalStatsDto> (GameStore store) =>
{
    var stats = store.GetGlobalStats();
    return TypedResults.Ok(new GlobalStatsDto(
        stats.TotalGames, stats.PlayerWins, stats.ComputerWins, stats.InProgressCount, stats.AverageShotsToFinish));
});

app.MapGet("/games/{id:guid}", Results<Ok<GameStateDto>, NotFound> (Guid id, GameStore store) =>
    store.TryGet(id, out var game)
        ? TypedResults.Ok(GameStateMapper.ToDto(game))
        : TypedResults.NotFound());

app.MapPost("/games/{id:guid}/shots", async Task<Results<Ok<TurnResultDto>, ValidationProblem, NotFound, ProblemHttpResult>> (
    Guid id,
    ShotRequest request,
    GameStore store,
    IValidator<ShotRequest> validator) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid)
        return TypedResults.ValidationProblem(validation.ToDictionary());

    if (!store.TryGet(id, out var game))
        return TypedResults.NotFound();

    if (game.Phase != GamePhase.InProgress)
        return TypedResults.Problem("La partie est terminée.", statusCode: StatusCodes.Status409Conflict);

    try
    {
        var result = game.Shoot(new Coordinate(request.Row, request.Col));
        return TypedResults.Ok(GameStateMapper.ToDto(result, game));
    }
    catch (ArgumentOutOfRangeException)
    {
        return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            ["row"] = [$"La case visée est hors de la grille de cette partie ({game.PlayerBoard.Size}x{game.PlayerBoard.Size})."]
        });
    }
}).Produces<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json");

app.Run();

public partial class Program;
