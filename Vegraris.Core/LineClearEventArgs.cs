using System;

namespace Vegraris
{
    public class LineClearEventArgs : EventArgs
    {
        public LineClearEventArgs(int lineNumber)
        {
            LineNumber = lineNumber;
        }

        public int LineNumber { get; }
    }
}
