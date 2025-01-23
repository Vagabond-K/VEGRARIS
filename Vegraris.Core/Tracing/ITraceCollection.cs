using System;
using System.Collections.Generic;

namespace Vegraris.Tracing
{
    public interface ITraceCollection<TTrace> : IEnumerable<TTrace>
    {
        TimeSpan LifeTime { get; set; }
        bool VacuumAtIdle { get; set; }
    }
}
