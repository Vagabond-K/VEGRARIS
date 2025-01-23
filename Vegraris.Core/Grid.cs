using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Vegraris.Tracing;

namespace Vegraris
{
    class Grid
    {
        public Grid(in int width, in int height)
        {
            Columns = width;
            Rows = height;
            cells = new Cell[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    cells[x, y] = new Cell(x, y);

            Pieces = new ReadOnlyObservableCollection<LockedPiece>(pieces = new ObservableCollection<LockedPiece>());
            LockedTraces = lockedTraces = new TraceCollection<LockedPiece>();
        }

        private readonly Cell[,] cells;
        private readonly ObservableCollection<LockedPiece> pieces;
        private readonly TraceCollection<LockedPiece> lockedTraces;

        private IEnumerable<CellCornerPoint> FindStartPoints(HashSet<Piece> pieces)
        {
            foreach (var cell in cells)
                foreach (var point in cell.CornerPoints)
                    point.NextPoint = null;

            foreach (var cell in cells)
                if (pieces.Contains(cell.Piece))
                    foreach (var start in cell.CornerPoints)
                        if (start.NextPoint == null)
                        {
                            var current = start;
                            while (current.NextPoint == null)
                            {
                                var nextX = current.Cell.X;
                                var nextY = current.Cell.Y;

                                switch (current.NextCorner)
                                {
                                    case CellCorner.LeftTop: nextX -= 1; break;
                                    case CellCorner.TopRight: nextY -= 1; break;
                                    case CellCorner.RightBottom: nextX += 1; break;
                                    case CellCorner.BottomLeft: nextY += 1; break;
                                }

                                current.NextPoint =
                                    current.NextCorner == CellCorner.LeftTop && nextX < 0
                                    || current.NextCorner == CellCorner.TopRight && nextY < 0
                                    || current.NextCorner == CellCorner.RightBottom && nextX >= Columns
                                    || current.NextCorner == CellCorner.BottomLeft && nextY >= Rows
                                    || current.Cell.Piece != cells[nextX, nextY].Piece
                                    || current.Cell.Clearing != cells[nextX, nextY].Clearing
                                    ? current.Cell[current.NextCorner]
                                    : cells[nextX, nextY][current.PrevCorner];
                                current = current.NextPoint;
                            }
                            yield return start;
                        }
        }

        private static IEnumerable<PiecePoint> ToPiecePoints(CellCornerPoint start, HashSet<Cell> cells = null)
        {
            var prev = start;
            start = prev.NextPoint;
            var current = start;
            do
            {
                cells?.Add(current.Cell);
                var isConvex = (prev.CellCorner == current.PrevCorner && current.CellCorner == current.NextPoint.PrevCorner) ? true :
                    (prev.CellCorner == current.NextCorner && current.CellCorner == current.NextPoint.NextCorner) ? false : (bool?)null;

                yield return new PiecePoint(current.Cell.X, current.Cell.Y, current.CellCorner, isConvex);

                prev = current;
                current = current.NextPoint;
            } while (current != start);
        }

        public int Columns { get; }
        public int Rows { get; }
        public Tetromino this[int x, int y] => cells[x, y].Piece?.Tetromino ?? Tetromino.Unknown;
        public IEnumerable<LockedPiece> Pieces { get; }
        public ITraceCollection<LockedPiece> LockedTraces { get; }

        public void LockDown(TimeSpan timeStamp, Action<GridEditor> drawingAction)
        {
            foreach (var item in Draw(drawingAction))
            {
                item.BeginLockedTrace(timeStamp);
                lockedTraces.Add(item);
            }
        }

        public void VacuumLockedTraces(TimeSpan renderingTime) => lockedTraces.Vacuum(renderingTime);

        public IEnumerable<LockedPiece> Draw(Action<GridEditor> drawingAction)
        {
            using (var editor = new GridEditor(this, cells))
            {
                drawingAction(editor);

                var editedPieces = new HashSet<LockedPiece>();
                var addedPieces = new HashSet<LockedPiece>();
                var newPieces = new Dictionary<Tetromino, LockedPiece>();

                foreach (var cell in editor.EditedCells)
                {
                    if (cell.Piece != null)
                        editedPieces.Add(cell.Piece);

                    var tetromino = cell.Editing.Value;
                    if (tetromino == Tetromino.Unknown)
                    {
                        if (!cell.Clearing)
                        {
                            var piece = new LockedPiece(cell.Piece.Tetromino) { Clearing = true };
                            cell.Piece = piece;
                            editedPieces.Add(piece);
                            addedPieces.Add(piece);
                        }
                        else
                            cell.Piece = null;
                        cell.Clearing = !cell.Clearing;
                    }
                    else
                    {
                        if (!newPieces.TryGetValue(tetromino, out var piece))
                        {
                            piece = new LockedPiece(tetromino);
                            newPieces[tetromino] = piece;
                            editedPieces.Add(piece);
                        }
                        cell.Piece = piece;
                    }
                    cell.Editing = null;
                }

                var cellsList = new List<(HashSet<Cell>, List<IEnumerable<PiecePoint>>)>();

                foreach (var pointGroup in FindStartPoints(new HashSet<Piece>(editedPieces))
                    .GroupBy(start => start.Cell.Piece, start =>
                    {
                        var cells = new HashSet<Cell>();
                        return (cells, ToPiecePoints(start, cells).ToArray());
                    }))
                {
                    cellsList.Clear();
                    foreach (var start in pointGroup)
                    {
                        var exists = cellsList.FirstOrDefault(item => item.Item1.Intersect(start.cells).Any());
                        if (exists.Item1 != null)
                        {
                            exists.Item1.UnionWith(start.cells);
                            exists.Item2.Add(start.Item2);
                        }
                        else
                        {
                            cellsList.Add((start.cells, new List<IEnumerable<PiecePoint>> { start.Item2 }));
                        }
                    }

                    for (var i = 0; i < cellsList.Count; i++)
                    {
                        var path = cellsList[i].Item2.ToArray();

                        if (i == 0)
                            pointGroup.Key.UpdatePath(path);
                        else
                        {
                            var piece = new LockedPiece(pointGroup.Key.Tetromino);
                            piece.UpdatePath(path);
                            foreach (var cell in cellsList[i].Item1)
                                cell.Piece = piece;
                            addedPieces.Add(piece);
                        }
                    }
                    editedPieces.Remove(pointGroup.Key);
                }

                foreach (var piece in addedPieces)
                    pieces.Add(piece);
                foreach (var piece in newPieces.Values)
                    pieces.Add(piece);
                foreach (var piece in editedPieces)
                {
                    pieces.Remove(piece);
                    (piece as ITrace).Complete();
                    lockedTraces.Remove(piece);
                }
                return newPieces.Values;
            }
        }

        public bool IsFullLine(in int line)
        {
            if (line < 0 || line >= Rows) return false;

            var holes = 0;
            for (int x = 0; x < Columns; x++)
                if (this[x, line] == Tetromino.Unknown)
                    holes++;
            return holes == 0;
        }

        public void BreakLine(int line)
        {
            if (line < 0 || line >= Rows) return;
            Draw(editor =>
            {
                for (int x = 0; x < Columns; x++)
                    editor[x, line] = Tetromino.Unknown;
            });
        }

        public void ShiftDown(in int line)
        {
            if (line < 0 || line >= Rows) return;
            BreakLine(line);

            var pieces = new HashSet<LockedPiece>();
            for (var y = line; y > 0; y--)
                for (var x = 0; x < Columns; x++)
                {
                    var piece = cells[x, y - 1].Piece;
                    cells[x, y].Piece = piece;
                    if (piece != null)
                        pieces.Add(piece);
                }

            for (var x = 0; x < Columns; x++)
                cells[x, 0].Piece = null;

            foreach (var piece in pieces)
                piece.UpdatePath(piece.Path.Select(points => points.Select(point => point.Offset(0, 1)).ToArray()).ToArray());
        }

        public void Clear()
        {
            foreach (var cell in cells)
            {
                cell.Piece = null;
                cell.Clearing = false;
            }

            lockedTraces.Clear();
            pieces.Clear();
        }
    }
}