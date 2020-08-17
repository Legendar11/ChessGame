using CoreGame.Enums;

namespace CoreGame.Extensions
{
    public static class GameTileExtensions
    {
        public static bool IsBarrier(this GameTile tile)
        {
            return tile != GameTile.Free;
        }
    }
}
