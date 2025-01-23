namespace Vegraris.Palettes
{
    public class SingleColorPalette : IPalette
    {
        public uint Color { get; set; } = 0xFF808080;
        public uint ToColor(Tetromino tetromino) => Color;
    }
}
