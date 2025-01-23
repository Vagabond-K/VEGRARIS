using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Vegraris.Tracing
{
    class TraceCollection<TTrace> : ReadOnlyObservableCollection<TTrace>, ITraceCollection<TTrace> where TTrace : class, ITrace
    {
        public TraceCollection() : base(new ObservableCollection<TTrace>()) { }

        private TimeSpan lifeTime = TimeSpan.FromSeconds(1);
        private bool vacuumAtIdle;
        private readonly List<TTrace> traces = new List<TTrace>();

        public TimeSpan LifeTime { get => lifeTime; set => SetProperty(ref lifeTime, value); }
        public bool VacuumAtIdle { get => vacuumAtIdle; set => SetProperty(ref vacuumAtIdle, value); }

        private bool SetProperty<T>(ref T target, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(target, value))
            {
                target = value;
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }

        internal void Add(TTrace item)
        {
            Items.Add(item);
            traces.Add(item);
        }
        internal bool Remove(TTrace item)
        {
            traces.Remove(item);
            return Items.Remove(item);
        }
        internal void Clear()
        {
            traces.Clear();
            Items.Clear();
        }

        public void Vacuum(TimeSpan renderingTime)
        {
            int count = traces.Count;
            for (int i = 0; i < count; i++)
            {
                var item = traces[0];
                if (renderingTime - item.TimeStamp > lifeTime)
                {
                    traces.Remove(item);
                    if (!vacuumAtIdle)
                        Items.Remove(item);
                    item.Complete();
                }
                else
                    break;
            }
            if (count > 0 && traces.Count == 0)
                Items.Clear();
        }
    }
}
