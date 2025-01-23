namespace Vegraris.Palettes
{
    public class DefaultPalette : IPalette
    {
        public uint ToColor(Tetromino tetromino)
        {
            switch (tetromino)
            {
                case Tetromino.I: return 0xFF00FFFF;
                case Tetromino.J: return 0xFF0040FF;
                case Tetromino.L: return 0xFFFF8000;
                case Tetromino.O: return 0xFFD0D000;
                case Tetromino.S: return 0xFF40D000;
                case Tetromino.Z: return 0xFFFF0000;
                case Tetromino.T: return 0xFFC000FF;
                default: return 0;
            }
        }
    }
}
