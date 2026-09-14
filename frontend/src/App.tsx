import React, { useState, useEffect } from "react";
import { GameStateResponse, GameMode, Player } from "./types/game";
import "./styles.css";

const API_BASE_URL = "http://localhost:5000/api";

export default function App() {
  const [gameState, setGameState] = useState<GameStateResponse | null>(null);
  const [selectedMode, setSelectedMode] = useState<GameMode>(0);
  const [error, setError] = useState<string | null>(null);

  const startNewGame = async (mode: GameMode) => {
    setError(null);
    try {
      const res = await fetch(`${API_BASE_URL}/games`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ mode }),
      });
      const data = await res.json();
      setGameState(data);
    } catch {
      setError("Failed to connect to backend server.");
    }
  };

  useEffect(() => {
    startNewGame(selectedMode);
  }, []);

  const handleClick = async (index: number) => {
    if (!gameState || gameState.status !== 0 || gameState.board[index] !== 0) return;

    const row = Math.floor(index / 3);
    const col = index % 3;

    try {
      const res = await fetch(`${API_BASE_URL}/games/${gameState.gameId}/moves`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ row, column: col, player: gameState.currentPlayer }),
      });

      if (!res.ok) {
        const err = await res.json();
        setError(err.message || "Invalid move");
        return;
      }
      const updatedState = await res.json();
      setGameState(updatedState);
    } catch {
      setError("Error submitting move.");
    }
  };

  const handleUndo = async () => {
    if (!gameState || gameState.moveHistory.length === 0) return;
    try {
      const res = await fetch(`${API_BASE_URL}/games/${gameState.gameId}/undo`, {
        method: "POST",
      });
      const updatedState = await res.json();
      setGameState(updatedState);
    } catch {
      setError("Failed to undo move.");
    }
  };

  const handleResetGame = async () => {
    if (!gameState) return;
    try {
      const res = await fetch(`${API_BASE_URL}/games/${gameState.gameId}/reset`, {
        method: "POST",
      });
      const updatedState = await res.json();
      setGameState(updatedState);
    } catch {
      setError("Failed to reset game.");
    }
  };

  const handleResetScoreboard = async () => {
    try {
      const res = await fetch(`${API_BASE_URL}/scoreboard/reset`, { method: "POST" });
      const scoreboard = await res.json();
      if (gameState) setGameState({ ...gameState, scoreboard });
    } catch {
      setError("Failed to reset scoreboard.");
    }
  };

  const getPlayerLabel = (p: Player) => (p === 1 ? "X" : p === 2 ? "O" : "");

  if (!gameState) return <div className="loading">Loading game...</div>;

  return (
    <div className="App">
      {error && <div className="error-banner">{error}</div>}

      <div>
        <h1>Tic Tac Toe (3x3)</h1>

        <div className="mode-selection">
          <label>Game Mode: </label>
          <select
            value={selectedMode}
            onChange={(e) => {
              const mode = Number(e.target.value) as GameMode;
              setSelectedMode(mode);
              startNewGame(mode);
            }}
          >
            <option value={0}>Two Player</option>
            <option value={1}>Play Against Computer</option>
          </select>
        </div>

        <div className="Board">
          {gameState.board.map((cell, index) => {
            const isWinning = gameState.winningCells.includes(index);
            return (
              <div
                key={index}
                className={`cell ${isWinning ? "winning" : ""}`}
                onClick={() => handleClick(index)}
              >
                {getPlayerLabel(cell)}
              </div>
            );
          })}
        </div>
      </div>

      <div>
        {gameState.status === 0 && (
          <h2>Current Turn: Player {getPlayerLabel(gameState.currentPlayer)}</h2>
        )}
        
        {gameState.status === 1 && (
          <h1>Result: Player {getPlayerLabel(gameState.winner)} Wins! 🎉</h1>
        )}

        {gameState.status === 2 && <h1>Result: Game Ended in a Draw! 🤝</h1>}

        <h2>Scoreboard</h2>
        <p>Player 1 (X): {gameState.scoreboard.xWins}</p>
        <p>Player 2 (O): {gameState.scoreboard.oWins}</p>
        <p>Draws: {gameState.scoreboard.draws}</p>

        <div className="button-group">
          <button onClick={handleUndo} disabled={gameState.moveHistory.length === 0}>
            Undo last move
          </button>
          <button onClick={handleResetGame}>Reset Game</button>
          <button onClick={handleResetScoreboard}>Reset Scoreboard</button>
        </div>

        <h3>Move History</h3>
        <table className="history-table">
          <thead>
            <tr>
              <th>Move</th>
              <th>Player</th>
              <th>Position</th>
            </tr>
          </thead>
          <tbody>
            {gameState.moveHistory.map((m) => (
              <tr key={m.moveNumber}>
                <td>{m.moveNumber}</td>
                <td>{getPlayerLabel(m.player)}</td>
                <td>Row {m.row}, Column {m.column}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}