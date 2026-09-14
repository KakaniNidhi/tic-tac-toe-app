namespace TicTacToe.Api.Models;

public enum GameMode
{
    TwoPlayer = 0,
    Computer = 1
}

public enum GameStatus
{
    InProgress = 0,
    Won = 1,
    Draw = 2
}

public enum Player
{
    None = 0,
    X = 1,
    O = 2
}

public class MoveRecord
{
    public int MoveNumber { get; set; }
    public Player Player { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
}

public class MoveRequest
{
    public int Row { get; set; }
    public int Column { get; set; }
    public Player Player { get; set; }
}

public class CreateGameRequest
{
    public GameMode Mode { get; set; } = GameMode.TwoPlayer;
}

public class ScoreboardState
{
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }
}

public class GameStateResponse
{
    public Guid GameId { get; set; }
    public Player[] Board { get; set; } = new Player[9];
    public Player CurrentPlayer { get; set; }
    public GameMode GameMode { get; set; }
    public GameStatus Status { get; set; }
    public Player Winner { get; set; }
    public int[] WinningCells { get; set; } = Array.Empty<int>();
    public List<MoveRecord> MoveHistory { get; set; } = new();
    public ScoreboardState Scoreboard { get; set; } = new();
}