using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Tests;

public class GameServiceTests
{
    private readonly GameService _service = new();

    [Fact]
    public void MakeMove_ValidMove_UpdatesBoardAndPlayerTurn()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        var state = _service.MakeMove(game.GameId, new MoveRequest { Row = 0, Column = 0, Player = Player.X });

        Assert.Equal(Player.X, state.Board[0]);
        Assert.Equal(Player.O, state.CurrentPlayer);
        Assert.Single(state.MoveHistory);
    }

    [Fact]
    public void MakeMove_OccupiedCell_ThrowsException()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        _service.MakeMove(game.GameId, new MoveRequest { Row = 0, Column = 0, Player = Player.X });

        Assert.Throws<InvalidOperationException>(() =>
            _service.MakeMove(game.GameId, new MoveRequest { Row = 0, Column = 0, Player = Player.O }));
    }

    [Fact]
    public void MakeMove_RowWin_DetectsWinnerAndUpdatesScoreboard()
    {
        _service.ResetScoreboard();
        var game = _service.CreateGame(GameMode.TwoPlayer);
        Guid id = game.GameId;

        _service.MakeMove(id, new MoveRequest { Row = 0, Column = 0, Player = Player.X });
        _service.MakeMove(id, new MoveRequest { Row = 1, Column = 0, Player = Player.O });
        _service.MakeMove(id, new MoveRequest { Row = 0, Column = 1, Player = Player.X });
        _service.MakeMove(id, new MoveRequest { Row = 1, Column = 1, Player = Player.O });
        var finalState = _service.MakeMove(id, new MoveRequest { Row = 0, Column = 2, Player = Player.X });

        Assert.Equal(GameStatus.Won, finalState.Status);
        Assert.Equal(Player.X, finalState.Winner);
        Assert.Equal(new[] { 0, 1, 2 }, finalState.WinningCells);
        Assert.Equal(1, finalState.Scoreboard.XWins);
    }

    [Fact]
    public void UndoMove_TwoPlayerMode_RemovesLastSingleMove()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        _service.MakeMove(game.GameId, new MoveRequest { Row = 0, Column = 0, Player = Player.X });
        var stateAfterUndo = _service.UndoMove(game.GameId);

        Assert.Equal(Player.None, stateAfterUndo.Board[0]);
        Assert.Equal(Player.X, stateAfterUndo.CurrentPlayer);
        Assert.Empty(stateAfterUndo.MoveHistory);
    }

    [Fact]
    public void ComputerMode_ExecutesComputerMoveAutomatically()
    {
        var game = _service.CreateGame(GameMode.Computer);
        var state = _service.MakeMove(game.GameId, new MoveRequest { Row = 0, Column = 0, Player = Player.X });

        Assert.Equal(2, state.MoveHistory.Count);
        Assert.Equal(Player.X, state.CurrentPlayer);
    }

    [Fact]
    public void ComputerMoveSelection_BlocksPlayerWin()
    {
        Player[] board = new Player[9]
        {
            Player.X, Player.X, Player.None,
            Player.None, Player.None, Player.None,
            Player.None, Player.None, Player.None
        };

        int move = GameService.GetBestComputerMove(board);
        Assert.Equal(2, move);
    }
}