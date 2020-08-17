using CoreGame.Enums;
using CoreGame.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreGame
{
    public struct ChessGame
    {
        /// <summary>
        /// Игровая доска.
        /// </summary>
        public List<GamePeice> Board { get; private set; }

        /// <summary>
        /// Игровые ходы.
        /// </summary>
        public readonly Stack<(GameMove move, GamePeice attacked)> GameMoves;

        #region Границы игрового поля
        public int LeftXBorder => 0;
        public int RightXBorder => 7;
        public int DownYBorder => 0;
        public int UpperYBorder => 7;
        #endregion

        /// <summary>
        /// Текущий ход
        /// </summary>
        public readonly GameSide Turn;

        /// <summary>
        /// окончена ли игра.
        /// </summary>
        public bool IsEndGame => UglyMoves().Count == 0;

        public ChessGame(GameSide curSide, List<GamePeice> board = null, Stack<(GameMove, GamePeice)> moves = null)
        {
            Board = board;
            GameMoves = moves;
            Turn = curSide;

            if (moves is null)
            {
                GameMoves = new Stack<(GameMove, GamePeice)>();
            }
            if (board is null)
            {
                Board = new List<GamePeice>();
                Reset();
            }
        }

        /// <summary>
        /// Все фигуры на поле.
        /// </summary>
        public IEnumerable<GamePeice> GamePeices => Board.Where(x => x.HaveFigure);

        #region Искуственный интеллект
        /// <summary>
        /// Получить значение клетки для расчета доски.
        /// </summary>
        private int GetPieceValue(GamePeice peice)
        {
            double[,] evalPawn =
            {
                { 0.0,  0.0,    0.0,    0.0,    0.0,    0.0,    0.0,    0.0 },
                { 5.0,  5.0,    5.0,    5.0,    5.0,    5.0,    5.0,    5.0 },
                { 1.0,  1.0,    2.0,    3.0,    3.0,    2.0,    1.0,    1.0 },
                { 0.5,  0.5,    1.0,    2.5,    2.5,    1.0,    0.5,    0.5 },
                { 0.0,  0.0,    0.0,    2.0,    2.0,    0.0,    0.0,    0.0 },
                { 0.5,  -0.5,   -1.0,   0.0,    0.0,    -1.0,   -0.5,   0.5 },
                { 0.5,  1.0,    1.0,    -2.0,   -2.0,   1.0,    1.0,    0.5 },
                { 0.0,  0.0,    0.0,    0.0,    0.0,    0.0,    0.0,    0.0 }
            };

            double[,] evalKnight =
            {
                { -5.0, -4.0,   -3.0,   -3.0,   -3.0,   -3.0,   -4.0,   -5.0 },
                { -4.0, -2.0,   0.0,    0.0,    0.0,    0.0,    -2.0,   -4.0 },
                { -3.0, 0.0,    1.0,    1.5,    1.5,    1.0,    0.0,    -3.0 },
                { -3.0, 0.5,    1.5,    2.0,    2.0,    1.5,    0.5,    -3.0 },
                { -3.0, 0.0,    1.5,    2.0,    2.0,    1.5,    0.0,    -3.0 },
                { -3.0, 0.5,    1.0,    1.5,    1.5,    1.0,    0.5,    -3.0 },
                { -4.0, -2.0,   0.0,    0.5,    0.5,    0.0,    -2.0,   -4.0 },
                { -5.0, -4.0,   -3.0,   -3.0,   -3.0,   -3.0,   -4.0,   -5.0 }
            };

            double[,] evalBishop =
            {
                { -2.0, -1.0,   -1.0,   -1.0,   -1.0,   -1.0,   -1.0,   -2.0 },
                { -1.0, 0.0,    0.0,    0.0,    0.0,    0.0,    0.0,    -1.0 },
                { -1.0, 0.0,    0.5,    1.0,    1.0,    0.5,    0.0,    -1.0 },
                { -1.0, 0.5,    0.5,    1.0,    1.0,    0.5,    0.5,    -1.0 },
                { -1.0, 0.0,    1.0,    1.0,    1.0,    1.0,    0.0,    -1.0 },
                { -1.0, 1.0,    1.0,    1.0,    1.0,    1.0,    1.0,    -1.0 },
                { -1.0, 0.5,    0.0,    0.0,    0.0,    0.0,    0.5,    -1.0 },
                { -2.0, -1.0,   -1.0,   -1.0,   -1.0,   -1.0,   -1.0,   -2.0 }
            };

            double[,] evalRook =
            {
                { 0.0,  0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 },
                { 0.5,  1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.5 },
                { -0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5 },
                { -0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5 },
                { -0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5 },
                { -0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5 },
                { -0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5 },
                { 0.0,  0.0, 0.0, 0.5, 0.5, 0.0, 0.0, 0.0 }
            };

            double[,] evalQueen =
            {
                { -2.0, -1.0,   -1.0,   -0.5,   -0.5,   -1.0,   -1.0,   -2.0 },
                { -1.0, 0.0,    0.0,    0.0,    0.0,    0.0,    0.0,    -1.0 },
                { -1.0, 0.0,    0.5,    0.5,    0.5,    0.5,    0.0,    -1.0 },
                { -0.5, 0.0,    0.5,    0.5,    0.5,    0.5,    0.0,    -0.5 },
                { 0.0,  0.0,    0.5,    0.5,    0.5,    0.5,    0.0,    -0.5 },
                { -1.0, 0.5,    0.5,    0.5,    0.5,    0.5,    0.0,    -1.0 },
                { -1.0, 0.0,    0.5,    0.0,    0.0,    0.0,    0.0,    -1.0 },
                { -2.0, -1.0,   -1.0,   -0.5,   -0.5,   -1.0,   -1.0,   -2.0 }
            };

            double[,] evalKing =
            {
                { -3.0, -4.0,   -4.0,   -5.0,   -5.0,   -4.0,   -4.0,   -3.0 },
                { -3.0, -4.0,   -4.0,   -5.0,   -5.0,   -4.0,   -4.0,   -3.0 },
                { -3.0, -4.0,   -4.0,   -5.0,   -5.0,   -4.0,   -4.0,   -3.0 },
                { -3.0, -4.0,   -4.0,   -5.0,   -5.0,   -4.0,   -4.0,   -3.0 },
                { -2.0, -3.0,   -3.0,   -4.0,   -4.0,   -3.0,   -3.0,   -2.0 },
                { -1.0, -2.0,   -2.0,   -2.0,   -2.0,   -2.0,   -2.0,   -1.0 },
                { 2.0,  2.0,    0.0,    0.0,    0.0,    0.0,    2.0,    2.0 },
                { 2.0,  3.0,    1.0,    0.0,    0.0,    1.0,    3.0,    2.0 }
            };

            var coef = peice.Side == GameSide.White ? 1 : -1;
            var resultX = peice.Side == GameSide.White
                ? LeftXBorder + peice.Point.X
                : RightXBorder - peice.Point.X;
            var resultY = peice.Side == GameSide.White
                ? DownYBorder + peice.Point.Y
                : UpperYBorder - peice.Point.Y;

            switch (peice.Figure)
            {
                case GameFigure.None:
                    return 0;
                case GameFigure.Pawn:
                    return coef * 10 + (int)evalPawn[resultY, resultX];
                case GameFigure.Knight:
                    return coef * 30 + (int)evalKnight[resultY, resultX];
                case GameFigure.Bishop:
                    return coef * 30 + (int)evalBishop[resultY, resultX];
                case GameFigure.Rook:
                    return coef * 50 + (int)evalRook[resultY, resultX];
                case GameFigure.Queen:
                    return coef * 90 + (int)evalQueen[resultY, resultX];
                case GameFigure.King:
                    return coef * 900 + (int)evalKing[resultY, resultX];
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Рассчитать ценность игровой доски.
        /// </summary>
        /// <returns></returns>
        private int EvaluateBoard()
        {
            int result = 0;
            foreach (var peice in GamePeices)
            {
                var value = GetPieceValue(peice);
                result += value;
            }
            return result;
        }

        /// <summary>
        /// Алгоритм Минимаакс.
        /// </summary>
        /// <param name="depth">Глубина</param>
        /// <param name="alpha">Альфа-отсечение</param>
        /// <param name="beta">Бета-отсечение</param>
        /// <param name="isMaximisingPlayer">Ход игрока или компьютера</param>
        /// <returns></returns>
        private int Minimax(int depth, int alpha, int beta, bool isMaximisingPlayer)
        {
            // если глубина равна нулю - рассчиытваем ценность доски для этой комбинации ходов
            if (depth == 0)
            {
                return -EvaluateBoard();
            }

            // получаем все возможные ходы
            // Далее идет просто реализация алгоритма как есть
            var moves = UglyMoves();
            if (isMaximisingPlayer)
            {
                var bestMove = -9999;
                foreach (var move in moves)
                {
                    MakeMove(move);

                    if (KingIsUnderAttck())
                    {
                        Undo();
                        continue;
                    }

                    bestMove = Math.Max(bestMove, Minimax(depth - 1, alpha, beta, !isMaximisingPlayer));
                    Undo();

                    alpha = Math.Max(alpha, bestMove);
                    if (beta <= alpha)
                    {
                        return bestMove;
                    }
                }
                return bestMove;
            }
            else
            {
                var bestMove = 9999;
                foreach (var move in moves)
                {
                    MakeMove(move);

                    if (KingIsUnderAttck())
                    {
                        Undo();
                        continue;
                    }

                    bestMove = Math.Min(bestMove, Minimax(depth - 1, alpha, beta, !isMaximisingPlayer));
                    Undo();

                    beta = Math.Min(beta, bestMove);
                    if (beta <= alpha)
                    {
                        return bestMove;
                    }
                }
                return bestMove;
            }
        }
        
        /// <summary>
        /// Ход компьютерного игрока
        /// </summary>
        /// <param name="coolAI"></param>
        /// <returns></returns>
        public GameMove AIMove(bool coolAI)
        {
            GameMove bestMove = default(GameMove);

            // если нет доступных ходов - выходим
            var newGameMoves = UglyMoves();
            if (newGameMoves.Count == 0)
            {
                return bestMove;
            }

            var bestValue = -9999;

            // для всех возможных ходов
            var moves = UglyMoves();
            foreach (var move in newGameMoves)
            {
                // совершаем ход
                MakeMove(move);

                // если этот ход открываем короля - то отменяем этот ход и идем дальше
                if (KingIsUnderAttck())
                {
                    Undo();
                    continue;
                }

                // узнаем ценность хода
                var boardValue = Minimax(coolAI ? 3 : 1, -10000, 10000, true);

                // отменяем игровой ход
                Undo();

                // узнаем максимально полезный ход
                if (boardValue > bestValue)
                {
                    bestValue = boardValue;
                    bestMove = move;
                }
            }

            return bestMove;
        }
        #endregion

        /// <summary>
        /// Сбросить игровое поле и все ходы
        /// </summary>
        public void Reset()
        {
            GameMoves.Clear();
            Board.Clear();

            for (var y = 1; y <= 6; y += 5)
            {
                var curSide = y == 1 ? GameSide.White : GameSide.Black;
                for (var x = LeftXBorder; x <= RightXBorder; x++)
                {
                    Board.Add(new GamePeice(new GamePoint(x, y), curSide, GameFigure.Pawn));
                }
            }

            Board.Add(new GamePeice(new GamePoint(LeftXBorder + 1, DownYBorder), GameSide.White, GameFigure.Knight));
            Board.Add(new GamePeice(new GamePoint(RightXBorder - 1, DownYBorder), GameSide.White, GameFigure.Knight));
            Board.Add(new GamePeice(new GamePoint(LeftXBorder + 1, UpperYBorder), GameSide.Black, GameFigure.Knight));
            Board.Add(new GamePeice(new GamePoint(RightXBorder - 1, UpperYBorder), GameSide.Black, GameFigure.Knight));

            Board.Add(new GamePeice(new GamePoint(LeftXBorder, DownYBorder), GameSide.White, GameFigure.Rook));
            Board.Add(new GamePeice(new GamePoint(RightXBorder, DownYBorder), GameSide.White, GameFigure.Rook));
            Board.Add(new GamePeice(new GamePoint(LeftXBorder, UpperYBorder), GameSide.Black, GameFigure.Rook));
            Board.Add(new GamePeice(new GamePoint(RightXBorder, UpperYBorder), GameSide.Black, GameFigure.Rook));

            Board.Add(new GamePeice(new GamePoint(LeftXBorder + 2, DownYBorder), GameSide.White, GameFigure.Bishop));
            Board.Add(new GamePeice(new GamePoint(RightXBorder - 2, DownYBorder), GameSide.White, GameFigure.Bishop));
            Board.Add(new GamePeice(new GamePoint(LeftXBorder + 2, UpperYBorder), GameSide.Black, GameFigure.Bishop));
            Board.Add(new GamePeice(new GamePoint(RightXBorder - 2, UpperYBorder), GameSide.Black, GameFigure.Bishop));

            Board.Add(new GamePeice(new GamePoint(LeftXBorder + 3, DownYBorder), GameSide.White, GameFigure.Queen));
            Board.Add(new GamePeice(new GamePoint(LeftXBorder + 3, UpperYBorder), GameSide.Black, GameFigure.Queen));

            Board.Add(new GamePeice(new GamePoint(RightXBorder - 3, DownYBorder), GameSide.White, GameFigure.King));
            Board.Add(new GamePeice(new GamePoint(RightXBorder - 3, UpperYBorder), GameSide.Black, GameFigure.King));
        }

        /// <summary>
        /// Получить тип игровой клетки.
        /// </summary>
        public GameTile GetGameTile(GamePoint location, GameSide side)
        {
            if (location.X < LeftXBorder || location.X > RightXBorder
                || location.Y < DownYBorder || location.Y > UpperYBorder)
            {
                return GameTile.Blocked;
            }
            
            var boardPiece = Board.SingleOrDefault(x => x.Point.Equals(location));
            if (boardPiece.Figure == GameFigure.None)
            {
                return GameTile.Free;
            }
            if (boardPiece.Side == side)
            {
                return GameTile.Another;
            }
            if (boardPiece.Side != side)
            {
                return GameTile.Enemy;
            }   
            return GameTile.Blocked;
        }

        /// <summary>
        /// Отменить последний ход.
        /// </summary>
        public void Undo()
        {
            if (GameMoves.Count == 0)
            {
                return;
            }

            // достаем последний ход
            var lastMove = GameMoves.Pop();
            var move = lastMove.move;
            var originalPeice = lastMove.move.Peice;

            // возвращаем оригинальную фигуру на ход назад
            var forFound = new GamePeice(move.DestinationPoint, originalPeice.Side, originalPeice.Figure);
            var index = forFound.GetIndexOnBoard(this);
            Board.RemoveAt(index);
            Board.Add(new GamePeice(originalPeice.Point, originalPeice.Side, originalPeice.Figure));

            // если фигура срубила кого-то - восстанавливаем срубленного
            if (move.MoveType == GameMoveType.Attack)
            {
                Board.Add(lastMove.attacked);
            }
        }

        /// <summary>
        /// Проверка, под нападением ли король
        /// </summary>
        /// <returns></returns>
        public bool KingIsUnderAttck()
        {
            var curTurn = Turn;

            var anotherTurn = curTurn == GameSide.White ? GameSide.Black : GameSide.White;
            var anotherPeices = GamePeices.Where(x => x.Side == anotherTurn);

            var curKing = GamePeices.Single(x => x.Side == curTurn && x.Figure == GameFigure.King);

            // для всех вражеских фигур
            foreach (var peice in anotherPeices)
            {
                var move = peice.GetMoves(this);
                // если кто-то может атаковать короля
                if (move.Any(x => x.DestinationPoint.Equals(curKing.Point)))
                {
                    return true; // возвращаем, что да, король под атакой
                }
            }
            return false;
        }

        /// <summary>
        /// Список ходов, которые могут защитить короля.
        /// </summary>
        /// <returns></returns>
        public List<GameMove> WhoCanDestroyAttacker()
        {
            var result = new List<GameMove>();
            var curTurn = Turn;

            var anotherTurn = curTurn == GameSide.White ? GameSide.Black : GameSide.White;
            var anotherPeices = GamePeices.Where(x => x.Side == anotherTurn);

            var curKing = GamePeices.Single(x => x.Side == curTurn && x.Figure == GameFigure.King);

            // для всех вражеских фигур
            foreach (var peice in anotherPeices)
            {
                var move = peice.GetMoves(this);
                // если кто-то из них атакует нашего короля
                if (move.Any(x => x.DestinationPoint.Equals(curKing.Point)))
                {
                    var curPeices = GamePeices.Where(x => x.Side == curTurn).ToList();
                    // для всех дружественных фигур
                    foreach (var peiceCur in curPeices)
                    {
                        var movesCur = peiceCur.GetMoves(this);

                        // для всех ходов всех дружественных фигур
                        // если при совершении хода наш король не будет под атакой - 
                        // то добавляем такой ход в результат
                        foreach (var moveCur in movesCur)
                        {
                            MakeMove(moveCur);

                            if (!KingIsUnderAttck())
                            {
                                result.Add(moveCur);
                            }

                            Undo();
                        }
                    }
                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// Все возможные ходы текущей стороны
        /// </summary>
        public List<GameMove> UglyMoves()
        {
            var curTurn = Turn;

            // если король под атакой - то получаем только те ходы, которые защитят короля
            if (KingIsUnderAttck())
            {
                var movesForDefend = WhoCanDestroyAttacker();
                return movesForDefend;
            }

            // добавляем все ходы всех невражеских фигур фигур
            var result = new List<GameMove>();
            foreach (var peice in GamePeices.Where(x => x.Side == curTurn))
            {
                result.AddRange(peice.GetMoves(this));
            }

            return result;
        }

        /// <summary>
        /// Совершить ход
        /// </summary>
        public void MakeMove(GameMove move)
        {
            var curPeice = move.Peice;
            var peiceTo = Board.SingleOrDefault(x => x.Point.Equals(move.DestinationPoint));

            // если кого-то срубаем - убираем срубленную фигуру с доски
            var attacked = new GamePeice();
            if (peiceTo.HaveFigure)
            {
                if (move.MoveType == GameMoveType.Attack)
                {
                    var indexTo = peiceTo.GetIndexOnBoard(this);
                    attacked = Board[indexTo];
                    Board.RemoveAt(indexTo);
                }
            }

            // кладем в стек игровых ходов
            GameMoves.Push((move, attacked));

            // перемещаем фигуру
            var index = move.Peice.GetIndexOnBoard(this);
            Board[index] = new GamePeice(move.DestinationPoint, curPeice.Side, curPeice.Figure);

        }
    }
}