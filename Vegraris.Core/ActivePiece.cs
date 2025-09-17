using System;
using System.Collections.Generic;
using System.Linq;

namespace Vegraris
{
    public class ActivePiece : Piece
    {
        private readonly static Dictionary<Tetromino, ActivePiece> defaults;

        private readonly static (int, int)[][] tSpinFront = { CreateTSpinPositions(-1, -1).ToArray(), CreateTSpinPositions(1, -1).ToArray() };
        private readonly static (int, int)[][] tSpinBack = { CreateTSpinPositions(-1, 1).ToArray(), CreateTSpinPositions(1, 1).ToArray() };

        private static IEnumerable<(int, int)> CreateTSpinPositions(int offsetX, int offsetY)
        {
            yield return (offsetX, offsetY);
            for (int i = 0; i < 3; i++)
            {
                var temp = offsetX;
                offsetX = -offsetY;
                offsetY = temp;
                yield return (offsetX, offsetY);
            }
        }

        static ActivePiece()
        {
            var grid = new Grid(4, 4);
            defaults = TetrominoExtensions.All.Select(tetromino => new ActivePiece(tetromino, grid)).ToDictionary(item => item.Tetromino, item => item);
        }


        internal static ActivePiece Create(Tetromino tetromino, Grid grid)
        {
            var result = (ActivePiece)defaults[tetromino].MemberwiseClone();
            result.grid = grid;
            result.InitLocation();
            return result;
        }
        internal static StaticPiece GetPreview(Tetromino tetromino) => defaults[tetromino].preview;

        private ActivePiece(Tetromino tetromino, Grid grid) : base(tetromino)
        {
            width = tetromino.GetWidth();
            height = tetromino.GetHeight();
            wallKicks = tetromino.GetWallKicks();
            shapes = tetromino.ToShapeMatrices();

            this.grid = grid;
            paths = Enumerable.Range(0, shapes.Length).Select(i =>
            {
                rotation = i;
                grid.Clear();
                grid.Draw(OnDraw);
                return grid.Pieces.First().Path;
            }).ToArray();
            this.grid = null;

            rotation = 0;
            Path = paths[0];
            var offsetX = -Path.Min(points => points.Min(point => point.CellX));
            var offsetY = -Path.Min(points => points.Min(point => point.CellY));

            preview = new StaticPiece(paths[0].Select(points => points.Select(point => point.Offset(offsetX, offsetY)).ToArray()).ToArray(), tetromino);
        }

        private readonly int width;
        private readonly int height;
        private readonly bool[][,] shapes;
        private readonly (int, int)[,] wallKicks;
        private readonly IEnumerable<IEnumerable<PiecePoint>>[] paths;
        private readonly StaticPiece preview;

        private Grid grid;
        private int x;
        private int y;
        private int rotation;
        private IEnumerable<IEnumerable<PiecePoint>> ghostPath;
        private bool lockingDown;

        internal TSpin TSpin { get; private set; } = TSpin.None;
        public IEnumerable<IEnumerable<PiecePoint>> GhostPath { get => ghostPath; private set => SetProperty(ref ghostPath, value); }
        public bool LockingDown { get => lockingDown; internal set => SetProperty(ref lockingDown, value); }

        private void InitLocation() => Update(Math.Max((grid.Columns - 4) / 2, 0), 0, 0, true);

        private void Update(in int x, in int y, in int rotation, bool forceUpdateGhost = false)
        {
            var changedGhost = this.x != x || this.rotation != rotation;
            var changed = changedGhost || this.y != y;

            this.x = x;
            this.y = y;
            this.rotation = rotation;

            if (changed)
                UpdatePath();
            if (forceUpdateGhost || changedGhost)
                UpdateGhost();
        }

        private void UpdatePath()
        {
            var x = this.x;
            var y = this.y;
            Path = paths[rotation].Select(points => points.Select(point => point.Offset(x, y)));
        }

        private void UpdateGhost()
        {
            var offsetY = 1;
            while (ValidateLocation(0, offsetY, 0))
                offsetY++;
            offsetY--;
            GhostPath = Path.Select(points => points.Select(point => point.Offset(0, offsetY)));
        }

        private void OnDraw(GridEditor editor)
        {
            for (int shapeX = 0; shapeX < width; shapeX++)
                for (int shapeY = 0; shapeY < height; shapeY++)
                {
                    var targetX = shapeX + x;
                    var targetY = shapeY + y;
                    if (targetX >= 0 && targetY >= 0 && targetX < grid.Columns && targetY < grid.Rows && shapes[rotation][shapeX, shapeY])
                        editor[targetX, targetY] = Tetromino;
                }
        }

        internal bool ValidateLocation(in int offsetX, in int offsetY, in int rotation)
        {
            for (int shapeX = 0; shapeX < width; shapeX++)
                for (int shapeY = 0; shapeY < height; shapeY++)
                {
                    var targetX = shapeX + x + offsetX;
                    var targetY = shapeY + y + offsetY;
                    if (shapes[(this.rotation + rotation) % 4][shapeX, shapeY] &&
                        (targetX < 0 || targetY < 0 || targetX >= grid.Columns || targetY >= grid.Rows || grid[targetX, targetY] != Tetromino.Unknown))
                        return false;
                }

            return true;
        }

        internal void LockDown(TimeSpan timeStamp)
        {
            GhostPath = null;
            grid.LockDown(timeStamp, editor => OnDraw(editor));
        }

        internal bool Move(in int offsetX, in int offsetY)
        {
            if (ValidateLocation(offsetX, offsetY, 0))
            {
                Update(x + offsetX, y + offsetY, rotation);
                TSpin = TSpin.None;
                return true;
            }
            return false;
        }

        internal bool Rotate(in bool cw)
        {
            for (int i = 0; i < 5; i++)
            {
                var offset = cw ? wallKicks[rotation % shapes.Length * 2, i] : wallKicks[(rotation + 3) % shapes.Length * 2 + 1, i];
                if (ValidateLocation(offset.Item1, -offset.Item2, cw ? 1 : 3))
                {
                    Update(x + offset.Item1, y - offset.Item2, (rotation + (cw ? 1 : 3)) % 4);

                    if (Tetromino == Tetromino.T)
                    {
                        var centerX = x + 1;
                        var centerY = y + 1;

                        int CountTSpinBlocks((int, int)[][] tSpinBlocks)
                            => tSpinBlocks.Count(positions =>
                            {
                                var tOffset = positions[rotation];
                                var targetX = centerX + tOffset.Item1;
                                var targetY = centerY + tOffset.Item2;
                                return targetX < 0 || targetX >= grid.Columns || targetY < 0 || targetY >= grid.Rows || grid[targetX, targetY] != Tetromino.Unknown;
                            });

                        var front = CountTSpinBlocks(tSpinFront);
                        var back = CountTSpinBlocks(tSpinBack);

                        if (front + back >= 3)
                        {
                            if (front == 2)
                                TSpin = TSpin.TSpin;
                            else
                                TSpin = TSpin.Mini;
                        }
                        else
                            TSpin = TSpin.None;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}