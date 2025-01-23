using System;
using System.Collections.Generic;

namespace Vegraris
{
    class TimerManager
    {
        private readonly HashSet<Timer> timers = new HashSet<Timer>();

        public TimeSpan TimeStamp { get; private set; }

        public Timer CreateTimer(Action callback, double dueTime, double period)
        {
            var result = new Timer(this, callback, dueTime, period);
            timers.Add(result);
            return result;
        }

        public bool RemoveTimer(Timer timer) => timers.Remove(timer);

        public void UpdateTimers(in TimeSpan renderingTime)
        {
            TimeStamp = renderingTime;
            foreach (var timer in timers)
                timer.Update(renderingTime);
        }
    }
}