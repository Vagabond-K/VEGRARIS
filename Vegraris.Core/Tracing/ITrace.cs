using System;

namespace Vegraris.Tracing
{
    interface ITrace
    {
        TimeSpan TimeStamp { get; }
        void Complete();
    }
}
