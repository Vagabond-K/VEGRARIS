using Vegraris.Drawing;

namespace Vegraris.Wasm
{
    class PieceDrawingFactory : PieceDrawingFactory<string, DrawingContext>
    {
        public PieceDrawingFactory()
        {
            CellSize = 40;
            StrokeThickness = 6;
            ContainsHole = false;
        }
    }
}