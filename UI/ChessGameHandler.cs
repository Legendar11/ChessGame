using CoreGame;
using CoreGame.Enums;
using CoreGame.Extensions;
using System.Drawing;
using System.Linq;

namespace UI
{
    internal class ChessGameHandler
    {
        /// <summary>
        /// Игровая логика.
        /// </summary>
        public ChessGame InnerGame { get; set; }

        /// <summary>
        /// Выбранная клетка.
        /// </summary>
        public GamePeice? SelectedPeice { get; set; }

        /// <summary>
        /// Изображения игровых фигур.
        /// </summary>
        private Image[,] PeiceImages { get; }

        /// <summary>
        /// Изображения игровых объектов.
        /// </summary>
        private Image[] GameImages { get; }

        /// <summary>
        /// Победитель игры.
        /// </summary>
        public GameSide Winner;

        public ChessGameHandler()
        {
            InnerGame = new ChessGame(GameSide.White);
            Winner = GameSide.Undefined;

            // заполняем изображения из файлов
            PeiceImages = new Image[2, 6];

            PeiceImages[0, 0] = Image.FromFile("Images/king_w.png");
            PeiceImages[0, 1] = Image.FromFile("Images/queen_w.png");
            PeiceImages[0, 2] = Image.FromFile("Images/rook_w.png");
            PeiceImages[0, 3] = Image.FromFile("Images/bishop_w.png");
            PeiceImages[0, 4] = Image.FromFile("Images/knight_w.png");
            PeiceImages[0, 5] = Image.FromFile("Images/pawn_w.png");
            //Black images
            PeiceImages[1, 0] = Image.FromFile("Images/king_b.png");
            PeiceImages[1, 1] = Image.FromFile("Images/queen_b.png");
            PeiceImages[1, 2] = Image.FromFile("Images/rook_b.png");
            PeiceImages[1, 3] = Image.FromFile("Images/bishop_b.png");
            PeiceImages[1, 4] = Image.FromFile("Images/knight_b.png");
            PeiceImages[1, 5] = Image.FromFile("Images/pawn_b.png");

            GameImages = new Image[3];
            //Misc images
            GameImages[0] = Image.FromFile("images/selected.png");
            GameImages[1] = Image.FromFile("images/checkglow.png");
            GameImages[2] = Image.FromFile("images/selectglow.png");
        }

        /// <summary>
        /// Маппер фигр и их изображений.
        /// </summary>
        public Image GetPieceImage(GameFigure figure, GameSide side)
        {
            if (side == GameSide.White)
            {
                switch (figure)
                {
                    case GameFigure.Rook:
                        return PeiceImages[0, 2];
                    case GameFigure.King:
                        return PeiceImages[0, 0];
                    case GameFigure.Bishop:
                        return PeiceImages[0, 3];
                    case GameFigure.Knight:
                        return PeiceImages[0, 4];
                    case GameFigure.Queen:
                        return PeiceImages[0, 1];
                    case GameFigure.Pawn:
                        return PeiceImages[0, 5];
                }
            }
            if (side == GameSide.Black)
            {
                switch (figure)
                {
                    case GameFigure.Rook:
                        return PeiceImages[1, 2];
                    case GameFigure.King:
                        return PeiceImages[1, 0];
                    case GameFigure.Bishop:
                        return PeiceImages[1, 3];
                    case GameFigure.Knight:
                        return PeiceImages[1, 4];
                    case GameFigure.Queen:
                        return PeiceImages[1, 1];
                    case GameFigure.Pawn:
                        return PeiceImages[1, 5];
                }
            }

            return null;
        }

        public void Undo()
        {
            InnerGame.Undo();
            SelectedPeice = null;
        }

        public enum GameImageType
        {
            Selected = 1
        }
        /// <summary>
        /// Маппер объектов и их изображений.
        /// </summary>
        public Image GetGameImage(GameImageType type)
        {
            switch (type)
            {
                case GameImageType.Selected:
                    return GameImages[0];
            }
            return null;
        }

        /// <summary>
        /// Получить игровую клетку.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public GamePeice GetPeiceOnLocation(Point location)
        {
            var point = new GamePoint(location.X, location.Y);
            var peice = InnerGame.Board.SingleOrDefault(x => x.Point.Equals(point));
            return peice;
        }

        /// <summary>
        /// Выбрать игровую клетку.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool SelectPeice(Point location)
        {
            var point = new GamePoint(location.X, location.Y);
            var peice = InnerGame.Board.SingleOrDefault(x => x.Point.Equals(point));

            if (!peice.HaveFigure // если у клетки нет фигуры или это не наш ход
                || InnerGame.Turn != peice.Side)
            {
                return false; // выбор клетки неудачен
            }

            SelectedPeice = peice;
            return true;
        }

        /// <summary>
        /// Совершить ход.
        /// </summary>
        public void MakeMove(Point location, bool coolAI)
        {
            if (SelectedPeice is null) // нужно, чтобы пользователь выбрал клетку
            {
                return;
            }

            // определяем, что ход корректен
            var point = new GamePoint(location.X, location.Y);
            var moves = SelectedPeice.Value.GetMoves(InnerGame);
            if (!moves.Any(x => x.DestinationPoint.Equals(point)))
            {
                return;
            }

            // определяем тип хода
            var moveType = GameMoveType.Attack;
            var tile = InnerGame.GetGameTile(point, InnerGame.Turn);
            if (tile == GameTile.Another)
            {
                moveType = GameMoveType.Special;
            }
            if (tile == GameTile.Enemy)
            {
                moveType = GameMoveType.Attack;
            }
            if (tile == GameTile.Free)
            {
                moveType = GameMoveType.Standart;
            }

            // совершаем ход
            InnerGame.MakeMove(new GameMove(SelectedPeice.Value, moveType, point));
            SelectedPeice = null;

            var turn = InnerGame.Turn == GameSide.White ? GameSide.Black : GameSide.White;
            InnerGame = new ChessGame(turn, InnerGame.Board, InnerGame.GameMoves);

            // если конец игры
            if (InnerGame.IsEndGame)
            {
                Winner = GameSide.White;
                return;
            }

            // ход ИИ
            var aiMove = InnerGame.AIMove(coolAI);
            if (aiMove.IsDefined) // если ИИ смог совершить ход
            {
                InnerGame.MakeMove(aiMove);

                turn = InnerGame.Turn == GameSide.White ? GameSide.Black : GameSide.White;
                InnerGame = new ChessGame(turn, InnerGame.Board, InnerGame.GameMoves);

                if (InnerGame.IsEndGame)
                {
                    Winner = GameSide.Black;
                    return;
                }
            }
            else // иначе продолжаем ход игрока (внештатная ситуация, не должен сюда переходить)
            {
                turn = InnerGame.Turn == GameSide.White ? GameSide.Black : GameSide.White;
                InnerGame = new ChessGame(turn, InnerGame.Board, InnerGame.GameMoves);
            }
        }
    }
}
