using System;
using System.Collections.Generic;

namespace Vegraris
{
    class GridEditor : IDisposable
    {
        internal GridEditor(Grid grid, Cell[,] cells)
        {
            Grid = grid;
            this.cells = cells;
        }

        public Grid Grid { get; }
        private Cell[,] cells;

        internal HashSet<Cell> EditedCells { get; } = new HashSet<Cell>();

        public Tetromino? this[in int x, in int y]
        {
            get => cells == null ? throw new ObjectDisposedException(nameof(GridEditor)) : cells[x, y].Editing;
            set
            {
                if (cells == null)
                    throw new ObjectDisposedException(nameof(GridEditor));

                var cell = cells[x, y];
                if (cell.Editing != value)
                {
                    cell.Editing = value;
                    if (value == null) EditedCells.Remove(cell);
                    else if (value.Value != Tetromino.Unknown || cell.Piece != null) EditedCells.Add(cell);
                }
            }
        }

        public void Dispose() => cells = null;
    }
}