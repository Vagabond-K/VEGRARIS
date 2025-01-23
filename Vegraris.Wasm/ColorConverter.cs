using Vegraris.Palettes;

namespace Vegraris.Wasm
{
    class ColorConverter : ColorConverter<string>
    {
        protected override string ToColor(uint color) => $"#{color & 0xffffff:X6}{color >> 24:X2}";
    }
}
