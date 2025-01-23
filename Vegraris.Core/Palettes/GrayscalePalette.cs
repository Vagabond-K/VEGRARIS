namespace Vegraris.Palettes
{
    public class GrayscalePalette : IPalette
    {
        public uint ToColor(Tetromino tetromino)
        {
            switch (tetromino)
            {
                case Tetromino.S: return 0xFFBBBBBB;
                case Tetromino.L: return 0xFFAAAAAA;
                case Tetromino.I: return 0xFF999999;
                case Tetromino.T: return 0xFF888888;
                case Tetromino.O: return 0xFF777777;
                case Tetromino.Z: return 0xFF666666;
                case Tetromino.J: return 0xFF555555;
                default: return 0;
            }
        }
    }
}
