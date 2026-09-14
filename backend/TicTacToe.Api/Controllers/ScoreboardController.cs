using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoreboardController : ControllerBase
{
    private readonly IGameService _gameService;

    public ScoreboardController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet]
    public ActionResult<ScoreboardState> GetScoreboard()
    {
        return Ok(_gameService.GetScoreboard());
    }

    [HttpPost("reset")]
    public ActionResult<ScoreboardState> ResetScoreboard()
    {
        return Ok(_gameService.ResetScoreboard());
    }
}