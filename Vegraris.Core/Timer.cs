using System;

namespace Vegraris
{
    class Timer
    {
        internal Timer(TimerManager timerManager, Action callback, double dueTime, double period)
        {
            this.timerManager = timerManager;
            this.callback = callback;
            DueTime = TimeSpan.FromMilliseconds(dueTime);
            Period = TimeSpan.FromMilliseconds(period);
        }

        private readonly TimerManager timerManager;
        private readonly Action callback;
        private bool updated;
        private TimeSpan lastTime;

        public bool IsRunning { get; private set; }

        public TimeSpan DueTime { get; set; }
        public TimeSpan Period { get; set; }

        public void Start()
        {
            IsRunning = true;
            updated = false;
            lastTime = timerManager.TimeStamp;
        }

        public void Stop()
        {
            IsRunning = false;
            updated = false;
        }

        internal void Update(in TimeSpan elapsed)
        {
            if (!IsRunning)
            {
                lastTime = elapsed;
                return;
            }

            var period = Period;
            var timeSpan = !updated ? DueTime : period;
            while (elapsed - lastTime >= timeSpan)
            {
                lastTime = lastTime.Add(timeSpan);
                updated = true;
                if (period.TotalMilliseconds == 0)
                {
                    IsRunning = false;
                    updated = false;
                }
                callback?.Invoke();
                if (timeSpan.TotalMilliseconds == 0)
                    return;
            }
        }
    }
}