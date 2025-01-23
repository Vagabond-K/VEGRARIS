namespace Vegraris
{
    public interface IScoring
    {
        int LineClear(int level, int lines, TSpin tSpin, bool isBackToBack, int combo);
        int SoftDrop(int level);
        int HardDrop(int level, int lines);
    }
}
