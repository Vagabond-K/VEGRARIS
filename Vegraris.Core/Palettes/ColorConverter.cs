namespace Vegraris.Palettes
{
    public abstract class ColorConverter<TColor>
    {
        private readonly static IPalette defaultPalette = new DefaultPalette();

        public IPalette Palette { get; set; }

        protected abstract TColor ToColor(uint color);
        public TColor ToColor(Tetromino tetromino) => ToColor((Palette ?? defaultPalette).ToColor(tetromino));
    }
}
