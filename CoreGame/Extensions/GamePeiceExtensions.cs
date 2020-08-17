using CoreGame.Enums;
using System.Collections.Generic;

namespace CoreGame.Extensions
{
    public static class GamePeiceExtensions
    {
        private static void AddPossibleValidMove(List<GameMove> moves, GamePoint location, GamePeice peice, GameTile tile)
        {
            if (tile == Enums.GameTile.Free)
            {
                moves.Add(new GameMove(peice, Enums.GameMoveType.Standart, location));
            }
            else if (tile == Enums.GameTile.Enemy)
            {
                moves.Add(new GameMove(peice, Enums.GameMoveType.Attack, location));
            }
        }

        private static List<GameMove> GetMovesForRook(GamePeice peice, ChessGame game)
        {
            var result = new List<GameMove>();
            for (int i = -1; true; i = 1) //Axis direction
            {
                // X-axis movement
                for (int xoffset = i; true; xoffset += i)
                {
                    var newLoc = new GamePoint(peice.Point.X + xoffset, peice.Point.Y);
                    var tile = game.GetGameTile(newLoc, peice.Side);

                    AddPossibleValidMove(result, newLoc, peice, tile);

                    if (tile.IsBarrier())
                    {
                        break;
                    }
                }

                // Y-axis movement
                for (int yoffset = i; true; yoffset += i)
                {
                    var newLoc = new GamePoint(peice.Point.X, peice.Point.Y + yoffset);

                    var tile = game.GetGameTile(newLoc, peice.Side);
                    AddPossibleValidMove(result, newLoc, peice, tile);

                    if (tile.IsBarrier())
                    {
                        break;
                    }
                }

                if (i == 1) { break; }
            }

            return result;
        }

        private static List<GameMove> GetMovesForKing(GamePeice peice, ChessGame game)
        {
            var result = new List<GameMove>();

            var king_offsets = new GamePoint[] 
            {
                new GamePoint(0, 1),
                new GamePoint(1, 0),
                new GamePoint(0, -1),
                new GamePoint(-1, 0),
                new GamePoint(1, 1),
                new GamePoint(1, -1),
                new GamePoint(-1, -1),
                new GamePoint(-1, 1)
            };
            foreach (var offset in king_offsets)
            {
                var newLoc = new GamePoint(peice.Point.X + offset.X, peice.Point.Y + offset.Y);
                var tile = game.GetGameTile(newLoc, peice.Side);

                AddPossibleValidMove(result, newLoc, peice, tile);
            }

            //Castling  TO-DO
            
            return result;
        }

        private static List<GameMove> GetMovesForBishop(GamePeice peice, ChessGame game)
        {
            var result = new List<GameMove>();

            var quadOffsets = new GamePoint[] 
            {
                new GamePoint(1, 1),
                new GamePoint(1, -1),
                new GamePoint(-1, -1),
                new GamePoint(-1, 1)
            };
            foreach (var quad in quadOffsets)
            {
                var offset = quad;
                while (true)
                {
                    var newLoc = new GamePoint(peice.Point.X + offset.X, peice.Point.Y + offset.Y);
                    var tile = game.GetGameTile(newLoc, peice.Side);

                    AddPossibleValidMove(result, newLoc, peice, tile);

                    if (tile.IsBarrier())
                    {
                        break;
                    }

                    offset = new GamePoint(offset.X + quad.X, offset.Y + quad.Y);
                }
            }

            return result;
        }

        private static List<GameMove> GetMovesForQueen(GamePeice peice, ChessGame game)
        {
            var result = new List<GameMove>();
            result.AddRange(GetMovesForRook(peice, game));
            result.AddRange(GetMovesForBishop(peice, game));
            return result;
        }

        private static List<GameMove> GetMovesForKnight(GamePeice peice, ChessGame game)
        {
            var result = new List<GameMove>();
            
            var offsets = new GamePoint[]
            {
                new GamePoint(1, 2),
                new GamePoint(2, 1),
                new GamePoint(2, -1),
                new GamePoint(1, -2),
                new GamePoint(-1, -2),
                new GamePoint(-2, -1),
                new GamePoint(-2, 1),
                new GamePoint(-1, 2)
            };
            foreach (var offset in offsets)
            {
                var newLoc = new GamePoint(peice.Point.X + offset.X, peice.Point.Y + offset.Y);
                var tile = game.GetGameTile(newLoc, peice.Side);

                AddPossibleValidMove(result, newLoc, peice, tile);
            }

            return result;
        }

        private static List<GameMove> GetMovesForPawn(GamePeice peice, ChessGame game)
        {
            var result = new List<GameMove>();

            int yOfsset = peice.Side == Enums.GameSide.White ? 1 : -1;

            for (int i = 1; true; i = -1) //X-Axis direction
            {
                var newLoc = new GamePoint(peice.Point.X + i, peice.Point.Y + yOfsset);
                var tile = game.GetGameTile(newLoc, peice.Side);

                if (tile == GameTile.Enemy)
                {
                    result.Add(new GameMove(peice, Enums.GameMoveType.Attack, newLoc));
                }

                if (i == -1) { break; }
            }

            //Forward directional movement
            for (int i = 1; i <= 2; i++)
            {
                var newLoc = new GamePoint(peice.Point.X, peice.Point.Y + (yOfsset * i));
                var tile = game.GetGameTile(newLoc, peice.Side);

                if (tile == GameTile.Free)
                {
                    result.Add(new GameMove(peice, Enums.GameMoveType.Standart, newLoc));
                }

                if (tile.IsBarrier())
                {
                    break;
                }

                var firstMovePositionY = peice.Side == Enums.GameSide.White ? 1 : 6;
                if (peice.Point.Y != firstMovePositionY)
                {
                    break;
                }
            }

            return result;
        }

        public static List<GameMove> GetMoves(this GamePeice peice, ChessGame game)
        {
            switch (peice.Figure)
            {
                case Enums.GameFigure.Rook:
                    return GetMovesForRook(peice, game);
                case Enums.GameFigure.King:
                    return GetMovesForKing(peice, game);
                case Enums.GameFigure.Bishop:
                    return GetMovesForBishop(peice, game);
                case Enums.GameFigure.Queen:
                    return GetMovesForQueen(peice, game);
                case Enums.GameFigure.Knight:
                    return GetMovesForKnight(peice, game);
                case Enums.GameFigure.Pawn:
                    return GetMovesForPawn(peice, game);
            }

            //return 
            return null;
        }

        public static int GetIndexOnBoard(this GamePeice peice, ChessGame game)
        {
            for (var i = 0; i < game.Board.Count; i++)
            {
                if (game.Board[i].Equals(peice))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
