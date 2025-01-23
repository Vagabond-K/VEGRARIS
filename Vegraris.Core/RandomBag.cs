using System;
using System.Collections.Generic;

namespace Vegraris
{
    class RandomBag
    {
        public RandomBag()
        {
            random = new Random();
            bag = new List<Tetromino>();
        }

        private readonly Random random;
        private readonly List<Tetromino> bag;

        public Tetromino Next()
        {
            if (bag.Count == 0)
                bag.AddRange(TetrominoExtensions.All);

            int index = random.Next(bag.Count);
            var result = bag[index];
            bag.RemoveAt(index);
            return result;
        }
    }
}