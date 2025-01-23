namespace Vegraris.Palettes
{
    public class ArcadePalette : IPalette
    {
        public uint ToColor(Tetromino tetromino)
        {
            switch (tetromino)
            {
                case Tetromino.I: return 0xFFFF0000;
                case Tetromino.J: return 0xFFD0D000;
                case Tetromino.L: return 0xFFC000FF;
                case Tetromino.O: return 0xFF0040FF;
                case Tetromino.S: return 0xFF00D0D0;
                case Tetromino.Z: return 0xFFFF8000;
                case Tetromino.T: return 0xFF40D000;
                default: return 0;
            }
        }
    }
}
