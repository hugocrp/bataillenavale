using BattleShip.API;
using BattleShip.API.Grpc;
using BattleShip.API.Validation;
using BattleShip.Models;
using BattleShip.Models.Contracts;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<GameStore>();
builder.Services.AddScoped<IValidator<ShotRequest>, ShotRequestValidator>();
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

app.MapPost("/games", (GameStore store) =>
{
    var game = store.Create();
    return Results.Created($"/games/{game.Id}", GameStateMapper.ToDto(game));
});

app.MapGet("/games/{id:guid}", IResult (Guid id, GameStore store) =>
    store.TryGet(id, out var game)
        ? Results.Ok(GameStateMapper.ToDto(game))
        : Results.NotFound());

app.MapPost("/games/{id:guid}/shots", async Task<IResult> (
    Guid id,
    ShotRequest request,
    GameStore store,
    IValidator<ShotRequest> validator) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());

    if (!store.TryGet(id, out var game))
        return Results.NotFound();

    if (game.Phase != GamePhase.InProgress)
        return Results.Conflict("La partie est terminée.");

    var result = game.Shoot(new Coordinate(request.Row, request.Col));
    return Results.Ok(GameStateMapper.ToDto(result, game));
});

app.Run();

public partial class Program;
