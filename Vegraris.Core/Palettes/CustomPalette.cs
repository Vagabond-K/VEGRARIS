namespace Vegraris.Palettes
{
    public class CustomPalette : IPalette
    {
        public IPalette BasePalette { get; set; }

        public uint? I { get; set; }
        public uint? J { get; set; }
        public uint? L { get; set; }
        public uint? O { get; set; }
        public uint? S { get; set; }
        public uint? Z { get; set; }
        public uint? T { get; set; }

        public uint ToColor(Tetromino tetromino)
        {
            switch (tetromino)
            {
                case Tetromino.I: return I ?? BasePalette?.ToColor(tetromino) ?? 0;
                case Tetromino.J: return J ?? BasePalette?.ToColor(tetromino) ?? 0;
                case Tetromino.L: return L ?? BasePalette?.ToColor(tetromino) ?? 0;
                case Tetromino.O: return O ?? BasePalette?.ToColor(tetromino) ?? 0;
                case Tetromino.S: return S ?? BasePalette?.ToColor(tetromino) ?? 0;
                case Tetromino.Z: return Z ?? BasePalette?.ToColor(tetromino) ?? 0;
                case Tetromino.T: return T ?? BasePalette?.ToColor(tetromino) ?? 0;
                default: return 0;
            }
        }
    }
}
