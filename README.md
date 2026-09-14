# Tic Tac Toe Full-Stack Application (.NET 8 + React TS)

Full-stack, browser-based Tic Tac Toe solution built with a **React.JS + TypeScript** frontend and a **.NET 8 Web API** backend.

## Tech Stack
- **Frontend**: React.JS, TypeScript, Vite, CSS3
- **Backend**: .NET 8 Web API (C#)
- **API Architecture**: RESTful API (JSON)
- **Testing Engine**: xUnit (Backend Unit Tests)

---

## Features Implemented
1. **Interactive 3x3 Grid**: Real-time board rendering, locked cells, and winning cell highlight formatting.
2. **Game Rules & Win Engine**: Row, column, diagonal win detection, draw logic, and alternating turn validation.
3. **Move History**: Complete table tracking move numbers, active players, and precise 1-based `Row X, Column Y` positions.
4. **Smart Undo (Option B Implemented)**:
   - **Two Player Mode**: Undoes 1 single move.
   - **Computer Mode**: Undoes pair of moves (Human + AI together).
   - **Scoreboard Adjustment**: Reversing a completed game decrements the win/draw score count appropriately.
5. **Computer Opponent AI**: Priority heuristic (Win $\rightarrow$ Block $\rightarrow$ Center $\rightarrow$ Corner $\rightarrow$ Open Cell).
6. **Session Scoreboard**: Server-backed dynamic scoreboard for X Wins, O Wins, and Draws.

---

## How to Run Locally

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js v18+](https://nodejs.org/)

### 1. Launch Backend API
```bash
cd backend/TicTacToe.Api
dotnet run
```
---

## Design Decisions

* **Backend State Ownership:** 
  - The backend acts as the single source of truth for all game states, move validation, win/draw evaluation, and scoreboard management.
  - **Scoreboard & Undo Behavior:**Undo is allowed even after a game has ended (Won/Draw). If a winning or draw-inducing move is undone, the session scoreboard dynamically decrements to reflect the reverted state correctly.

* **Computer AI Strategy:**
  * Implemented a rule-based deterministic AI priority algorithm rather than full Minimax to fulfill the *Basic Computer Mode* requirement cleanly and predictably:
    1. Check for immediate winning move ($O$).
    2. Check for immediate blocking move against human ($X$).
    3. Take center cell (`Row 1, Col 1` / index 4).
    4. Take available corner cells (`0, 2, 6, 8`).
    5. Take any remaining available cell.

* **RESTful State Management:**
  - Game sessions are decoupled into distinct resource endpoints (`/api/games`, `/api/scoreboard`). State updates are triggered via explicit POST actions to enforce clean REST principles.

---

## Clarifications and Assumptions

* **In-Memory Storage Scope:**
  - Game sessions and scoreboards are stored in memory using singleton services. Restarting the backend server resets active sessions and global scores.
* **Turn Sequence in Computer Mode:**
  - Human player is always assigned symbol **X** and moves first. The computer player is assigned symbol **O** and responds automatically after a valid human turn.
* **Atomic Computer Undo:**
  - In Computer Mode, triggering an Undo action reverts two moves simultaneously (the Computer's last move + the Human's preceding move) so the turn immediately resets back to the human player without desynchronizing state.

---

## Known Limitations

* **Session Persistence:**
  - Because state is maintained in-memory without persistent storage (e.g., SQLite or PostgreSQL), refreshing the backend server purges active games and scoreboard history.
* **Single Active Session Concurrent Access:**
  - The current in-memory store uses basic dictionary structures suitable for single-user local evaluation rather than thread-safe distributed caching (e.g., Redis).
* **AI Complexity:**
  - The computer opponent uses a rule-based priority algorithm instead of a full Minimax algorithm; while highly effective, specialized trap setups can still win against it.

---

## Future Improvements

* **Database Integration:**
  - Integrate SQLite or EF Core with PostgreSQL to persist game histories, user profiles, and global leaderboards permanently across server restarts.
* **Difficulty Modes:**
  - Add configurable AI difficulty levels (Easy/Random, Medium/Priority-based, Hard/Unbeatable Minimax).
* **Real-Time Multiplayer:**
  - Upgrade from polling REST APIs to SignalR / WebSockets to enable real-time two-player matches across different devices.
* **Expanded Test Coverage:**
  - Add end-to-end (E2E) UI testing using Cypress or Playwright for component interaction and edge-case visual regression testing.