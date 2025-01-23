using System;

namespace Vegraris
{
    public class DefaultScoring : IScoring
    {
        public virtual int LineClear(int level, int lines, TSpin tSpin, bool isBackToBack, int combo)
        {
            level = Math.Max(level, 1);
            combo = Math.Max(0, combo);

            var result = 0;
            switch (tSpin)
            {
                case TSpin.None:
                    switch (lines)
                    {
                        case 1: result = 100; break;
                        case 2: result = 300; break;
                        case 3: result = 500; break;
                        case 4: result = 800; break;
                    }
                    break;
                case TSpin.Mini:
                    switch (lines)
                    {
                        case 0: result = 100; break;
                        case 1: result = 200; break;
                        case 2: result = 400; break;
                    }
                    break;
                case TSpin.TSpin:
                    switch (lines)
                    {
                        case 0: result = 400; break;
                        case 1: result = 800; break;
                        case 2: result = 1200; break;
                        case 3: result = 1600; break;
                    }
                    break;
            }

            result *= level;

            if (isBackToBack)
                result = (int)(result * 1.5);

            return result + 50 * combo * level;
        }

        public virtual int SoftDrop(int level) => 1;

        public virtual int HardDrop(int level, int lines) => 2 * lines;
    }
}
