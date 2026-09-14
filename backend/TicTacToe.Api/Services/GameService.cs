using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IGameService
{
    GameStateResponse CreateGame(GameMode mode);
    GameStateResponse GetGame(Guid gameId);
    GameStateResponse MakeMove(Guid gameId, MoveRequest move);
    GameStateResponse UndoMove(Guid gameId);
    GameStateResponse ResetGame(Guid gameId);
    ScoreboardState GetScoreboard();
    ScoreboardState ResetScoreboard();
}

public class GameService : IGameService
{
    private static readonly Dictionary<Guid, GameSession> _games = new();
    private static readonly ScoreboardState _scoreboard = new();
    private static readonly object _lock = new();

    private static readonly int[][] WinningLines = new int[][]
    {
        new[] {0, 1, 2}, new[] {3, 4, 5}, new[] {6, 7, 8}, // Rows
        new[] {0, 3, 6}, new[] {1, 4, 7}, new[] {2, 5, 8}, // Columns
        new[] {0, 4, 8}, new[] {2, 4, 6}                  // Diagonals
    };

    private class GameSession
    {
        public Guid GameId { get; set; }
        public Player[] Board { get; set; } = new Player[9];
        public Player CurrentPlayer { get; set; } = Player.X;
        public GameMode Mode { get; set; }
        public GameStatus Status { get; set; } = GameStatus.InProgress;
        public Player Winner { get; set; } = Player.None;
        public int[] WinningCells { get; set; } = Array.Empty<int>();
        public List<MoveRecord> MoveHistory { get; set; } = new();
        public bool ScoreRecorded { get; set; } = false;
    }

    public GameStateResponse CreateGame(GameMode mode)
    {
        lock (_lock)
        {
            var session = new GameSession
            {
                GameId = Guid.NewGuid(),
                Mode = mode,
                CurrentPlayer = Player.X,
                Status = GameStatus.InProgress
            };
            _games[session.GameId] = session;
            return MapToResponse(session);
        }
    }

    public GameStateResponse GetGame(Guid gameId)
    {
        lock (_lock)
        {
            if (!_games.TryGetValue(gameId, out var session))
                throw new KeyNotFoundException("Game session not found.");
            return MapToResponse(session);
        }
    }

    public GameStateResponse MakeMove(Guid gameId, MoveRequest move)
    {
        lock (_lock)
        {
            if (!_games.TryGetValue(gameId, out var session))
                throw new KeyNotFoundException("Game session not found.");

            ExecutePlayerMove(session, move.Row, move.Column, move.Player);

            // Trigger AI Move if in Computer Mode and game is still running
            if (session.Mode == GameMode.Computer && session.Status == GameStatus.InProgress && session.CurrentPlayer == Player.O)
            {
                var compIndex = GetBestComputerMove(session.Board);
                if (compIndex != -1)
                {
                    int compRow = compIndex / 3;
                    int compCol = compIndex % 3;
                    ExecutePlayerMove(session, compRow, compCol, Player.O);
                }
            }

            return MapToResponse(session);
        }
    }

    private void ExecutePlayerMove(GameSession session, int row, int col, Player player)
    {
        if (session.Status != GameStatus.InProgress)
            throw new InvalidOperationException("Game is already completed.");

        if (row < 0 || row > 2 || col < 0 || col > 2)
            throw new ArgumentException("Move outside board boundaries.");

        int cellIndex = row * 3 + col;
        if (session.Board[cellIndex] != Player.None)
            throw new InvalidOperationException("Cell is already occupied.");

        if (player != session.CurrentPlayer)
            throw new InvalidOperationException($"Not player {player}'s turn.");

        session.Board[cellIndex] = player;
        session.MoveHistory.Add(new MoveRecord
        {
            MoveNumber = session.MoveHistory.Count + 1,
            Player = player,
            Row = row + 1,
            Column = col + 1
        });

        EvaluateGameStatus(session);

        if (session.Status == GameStatus.InProgress)
        {
            session.CurrentPlayer = player == Player.X ? Player.O : Player.X;
        }
    }

    private void EvaluateGameStatus(GameSession session)
    {
        foreach (var line in WinningLines)
        {
            if (session.Board[line[0]] != Player.None &&
                session.Board[line[0]] == session.Board[line[1]] &&
                session.Board[line[1]] == session.Board[line[2]])
            {
                session.Status = GameStatus.Won;
                session.Winner = session.Board[line[0]];
                session.WinningCells = line;

                if (!session.ScoreRecorded)
                {
                    if (session.Winner == Player.X) _scoreboard.XWins++;
                    else if (session.Winner == Player.O) _scoreboard.OWins++;
                    session.ScoreRecorded = true;
                }
                return;
            }
        }

        if (session.Board.All(cell => cell != Player.None))
        {
            session.Status = GameStatus.Draw;
            session.Winner = Player.None;
            if (!session.ScoreRecorded)
            {
                _scoreboard.Draws++;
                session.ScoreRecorded = true;
            }
        }
    }

    public GameStateResponse UndoMove(Guid gameId)
    {
        lock (_lock)
        {
            if (!_games.TryGetValue(gameId, out var session))
                throw new KeyNotFoundException("Game session not found.");

            if (session.MoveHistory.Count == 0)
                throw new InvalidOperationException("No moves to undo.");

            // Clarification 2 (Option B): Adjust Scoreboard if reversing completed game
            if (session.ScoreRecorded)
            {
                if (session.Winner == Player.X) _scoreboard.XWins--;
                else if (session.Winner == Player.O) _scoreboard.OWins--;
                else if (session.Status == GameStatus.Draw) _scoreboard.Draws--;
                session.ScoreRecorded = false;
            }

            int movesToRemove = (session.Mode == GameMode.Computer && session.MoveHistory.Count >= 2) ? 2 : 1;

            for (int i = 0; i < movesToRemove; i++)
            {
                if (session.MoveHistory.Count > 0)
                {
                    var lastMove = session.MoveHistory.Last();
                    int cellIndex = (lastMove.Row - 1) * 3 + (lastMove.Column - 1);
                    session.Board[cellIndex] = Player.None;
                    session.MoveHistory.RemoveAt(session.MoveHistory.Count - 1);
                }
            }

            session.Status = GameStatus.InProgress;
            session.Winner = Player.None;
            session.WinningCells = Array.Empty<int>();

            if (session.MoveHistory.Count == 0)
            {
                session.CurrentPlayer = Player.X;
            }
            else
            {
                session.CurrentPlayer = session.MoveHistory.Last().Player == Player.X ? Player.O : Player.X;
            }

            return MapToResponse(session);
        }
    }

    public GameStateResponse ResetGame(Guid gameId)
    {
        lock (_lock)
        {
            if (!_games.TryGetValue(gameId, out var session))
                throw new KeyNotFoundException("Game session not found.");

            session.Board = new Player[9];
            session.CurrentPlayer = Player.X;
            session.Status = GameStatus.InProgress;
            session.Winner = Player.None;
            session.WinningCells = Array.Empty<int>();
            session.MoveHistory.Clear();
            session.ScoreRecorded = false;

            return MapToResponse(session);
        }
    }

    public ScoreboardState GetScoreboard()
    {
        lock (_lock)
        {
            return new ScoreboardState
            {
                XWins = _scoreboard.XWins,
                OWins = _scoreboard.OWins,
                Draws = _scoreboard.Draws
            };
        }
    }

    public ScoreboardState ResetScoreboard()
    {
        lock (_lock)
        {
            _scoreboard.XWins = 0;
            _scoreboard.OWins = 0;
            _scoreboard.Draws = 0;
            return GetScoreboard();
        }
    }

    // AI Heuristic Engine
    public static int GetBestComputerMove(Player[] board)
    {
        // Priority 1: Winning move
        int winMove = FindWinningIndex(board, Player.O);
        if (winMove != -1) return winMove;

        // Priority 2: Block player X
        int blockMove = FindWinningIndex(board, Player.X);
        if (blockMove != -1) return blockMove;

        // Priority 3: Center cell
        if (board[4] == Player.None) return 4;

        // Priority 4: Corner cell
        int[] corners = { 0, 2, 6, 8 };
        foreach (var c in corners)
            if (board[c] == Player.None) return c;

        // Priority 5: Any open cell
        for (int i = 0; i < 9; i++)
            if (board[i] == Player.None) return i;

        return -1;
    }

    private static int FindWinningIndex(Player[] board, Player p)
    {
        foreach (var line in WinningLines)
        {
            int count = 0, emptyIdx = -1;
            foreach (var idx in line)
            {
                if (board[idx] == p) count++;
                else if (board[idx] == Player.None) emptyIdx = idx;
            }
            if (count == 2 && emptyIdx != -1) return emptyIdx;
        }
        return -1;
    }

    private GameStateResponse MapToResponse(GameSession session) => new()
    {
        GameId = session.GameId,
        Board = session.Board,
        CurrentPlayer = session.CurrentPlayer,
        GameMode = session.Mode,
        Status = session.Status,
        Winner = session.Winner,
        WinningCells = session.WinningCells,
        MoveHistory = session.MoveHistory,
        Scoreboard = GetScoreboard()
    };
}