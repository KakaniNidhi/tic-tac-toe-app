export type GameMode = 0 | 1; // 0: TwoPlayer, 1: Computer
export type Player = 0 | 1 | 2; // 0: None, 1: X, 2: O
export type GameStatus = 0 | 1 | 2; // 0: InProgress, 1: Won, 2: Draw

export interface MoveRecord {
  moveNumber: number;
  player: Player;
  row: number;
  column: number;
}

export interface ScoreboardState {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface GameStateResponse {
  gameId: string;
  board: Player[];
  currentPlayer: Player;
  gameMode: GameMode;
  status: GameStatus;
  winner: Player;
  winningCells: number[];
  moveHistory: MoveRecord[];
  scoreboard: ScoreboardState;
}