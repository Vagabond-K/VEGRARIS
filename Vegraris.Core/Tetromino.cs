using System;
using System.Collections.Generic;
using System.Linq;

namespace Vegraris
{
    public enum Tetromino : ushort
    {
        Unknown = 0,
        //shapeBits_wallKicks_width_height
        I = 0b00001111_01_100_100,
        J = 0b10001110_00_011_011,
        L = 0b00101110_00_011_011,
        O = 0b01100110_10_100_011,
        S = 0b01101100_00_011_011,
        Z = 0b11000110_00_011_011,
        T = 0b01001110_00_011_011
    }

    static class TetrominoExtensions
    {
        //https://tetris.wiki/Super_Rotation_System
        private readonly static (int, int)[][,] defaultWallKicks =
        {
            new (int, int)[,]
            {
                { ( 0, 0), (-1, 0), (-1,+1), ( 0,-2), (-1,-2) }, //0 to 90
                { ( 0, 0), (+1, 0), (+1,-1), ( 0,+2), (+1,+2) }, //90 to 0
                { ( 0, 0), (+1, 0), (+1,-1), ( 0,+2), (+1,+2) }, //90 to 180
                { ( 0, 0), (-1, 0), (-1,+1), ( 0,-2), (-1,-2) }, //180 to 90
                { ( 0, 0), (+1, 0), (+1,+1), ( 0,-2), (+1,-2) }, //180 to 270
                { ( 0, 0), (-1, 0), (-1,-1), ( 0,+2), (-1,+2) }, //270 to 180
                { ( 0, 0), (-1, 0), (-1,-1), ( 0,+2), (-1,+2) }, //270 to 0
                { ( 0, 0), (+1, 0), (+1,+1), ( 0,-2), (+1,-2) }  //0 to 270
            },
            new (int, int)[,]
            {
                { ( 0, 0), (-2, 0), (+1, 0), (-2,-1), (+1,+2) }, //0 to 90
                { ( 0, 0), (+2, 0), (-1, 0), (+2,+1), (-1,-2) }, //90 to 0
                { ( 0, 0), (-1, 0), (+2, 0), (-1,+2), (+2,-1) }, //90 to 180
                { ( 0, 0), (+1, 0), (-2, 0), (+1,-2), (-2,+1) }, //180 to 90
                { ( 0, 0), (+2, 0), (-1, 0), (+2,+1), (-1,-2) }, //180 to 270
                { ( 0, 0), (-2, 0), (+1, 0), (-2,-1), (+1,+2) }, //270 to 180
                { ( 0, 0), (+1, 0), (-2, 0), (+1,-2), (-2,+1) }, //270 to 0
                { ( 0, 0), (-1, 0), (+2, 0), (-1,+2), (+2,-1) }  //0 to 270
            },
            new (int, int)[8, 5]
        };

        public static IEnumerable<Tetromino> All { get; } = Enum.GetValues(typeof(Tetromino)).Cast<Tetromino>().Where(tetromino => tetromino != Tetromino.Unknown).ToArray();

        public static int GetWidth(this Tetromino tetromino) => ((ushort)tetromino >> 3) & 0b111;
        public static int GetHeight(this Tetromino tetromino) => (ushort)tetromino & 0b111;
        public static bool[][,] ToShapeMatrices(this Tetromino tetromino)
        {
            var width = tetromino.GetWidth();
            var height = tetromino.GetHeight();
            var shapeBits = (ushort)tetromino & 0xff00;

            var shapes = new bool[4][,];
            var shape = shapes[0] = new bool[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    shape[x, y] = ((shapeBits >> ((3 - y) * 4 + (3 - x))) & 1) != 0;

            for (int i = 1; i < shapes.Length; i++)
            {
                var rotated = new bool[width, height];

                if (width == height)
                    for (int x = 0; x < width; x++)
                        for (int y = 0; y < height; y++)
                            rotated[width - 1 - y, x] = shape[x, y];
                else
                    Array.Copy(shape, rotated, shape.Length);

                shape = shapes[i] = rotated;
            }
            return shapes;
        }

        public static (int, int)[,] GetWallKicks(this Tetromino tetromino) => defaultWallKicks[((ushort)tetromino >> 6) & 0b11];
    }
}
