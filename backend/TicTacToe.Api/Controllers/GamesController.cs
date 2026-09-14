using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost]
    public ActionResult<GameStateResponse> CreateGame([FromBody] CreateGameRequest request)
    {
        var game = _gameService.CreateGame(request.Mode);
        return CreatedAtAction(nameof(GetGame), new { id = game.GameId }, game);
    }

    [HttpGet("{id}")]
    public ActionResult<GameStateResponse> GetGame(Guid id)
    {
        try
        {
            return Ok(_gameService.GetGame(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/moves")]
    public ActionResult<GameStateResponse> MakeMove(Guid id, [FromBody] MoveRequest move)
    {
        try
        {
            return Ok(_gameService.MakeMove(id, move));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/undo")]
    public ActionResult<GameStateResponse> UndoMove(Guid id)
    {
        try
        {
            return Ok(_gameService.UndoMove(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/reset")]
    public ActionResult<GameStateResponse> ResetGame(Guid id)
    {
        try
        {
            return Ok(_gameService.ResetGame(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}