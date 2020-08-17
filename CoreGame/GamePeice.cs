using CoreGame.Enums;
using System;

namespace CoreGame
{
    /// <summary>
    /// Игровая точка.
    /// </summary>
    public struct GamePoint : IEquatable<GamePoint>
    {
        public readonly int X;

        public readonly int Y;

        public GamePoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(GamePoint other)
        {
            return X == other.X && Y == other.Y;
        }
    }

    /// <summary>
    /// Игровой ход.
    /// </summary>
    public struct GameMove : IEquatable<GameMove>
    {
        /// <summary>
        /// Клетка, откуда совершается ход
        /// </summary>
        public readonly GamePeice Peice;

        /// <summary>
        /// Тип хода
        /// </summary>
        public readonly GameMoveType MoveType;

        /// <summary>
        /// Точка, куда соврешается ход
        /// </summary>
        public readonly GamePoint DestinationPoint;

        /// <summary>
        /// Определен ли ход (для ИИ)
        /// </summary>
        public readonly bool IsDefined;

        public GameMove(GamePeice peice, GameMoveType move, GamePoint point)
        {
            Peice = peice;
            MoveType = move;
            DestinationPoint = point;
            IsDefined = true;
        }

        public bool Equals(GameMove other)
        {
            return Peice.Equals(other.Peice)
                && MoveType.Equals(other.MoveType)
                && DestinationPoint.Equals(other.DestinationPoint);
        }
    }

    /// <summary>
    /// Игровая клетка.
    /// </summary>
    public struct GamePeice : IEquatable<GamePeice>
    {
        /// <summary>
        /// Сторона
        /// </summary>
        public readonly GameSide Side;

        /// <summary>
        /// Фигура
        /// </summary>
        public readonly GameFigure Figure;

        /// <summary>
        /// Локация
        /// </summary>
        public readonly GamePoint Point;

        public bool HaveFigure => Figure != GameFigure.None;
        
        public GamePeice(GamePoint point, GameSide side = GameSide.Undefined, GameFigure figure = GameFigure.None)
        {
            Side = side;
            Point = point;
            Figure = figure;
        }

        public bool Equals(GamePeice other)
        {
            return Side == other.Side
                && Figure == other.Figure
                && Point.Equals(other.Point);
        }
    }
}
