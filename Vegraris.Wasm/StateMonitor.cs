using System.Collections.Specialized;
using System.ComponentModel;

namespace Vegraris.Wasm
{
    class StateMonitor
    {
        private readonly List<WeakReference<PropertyChangedMonitor>> propertyChangedMonitors = [];
        private readonly List<WeakReference<CollectionChangedMonitor>> collectionChangedMonitors = [];

        public bool StateHasChanged { get; set; }

        private static void Vacuum<T>(List<WeakReference<T>> list) where T : class
        {
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (!item.TryGetTarget(out _))
                {
                    list.Remove(item);
                    i--;
                }
            }
        }

        public void Add(object? target)
        {
            if (target is INotifyPropertyChanged notifyPropertyChanged)
                propertyChangedMonitors.Add(new WeakReference<PropertyChangedMonitor>(new PropertyChangedMonitor(this, notifyPropertyChanged)));
            if (target is INotifyCollectionChanged notifyCollectionChanged)
                collectionChangedMonitors.Add(new WeakReference<CollectionChangedMonitor>(new CollectionChangedMonitor(this, notifyCollectionChanged)));
        }

        public void Vacuum()
        {
            Vacuum(propertyChangedMonitors);
            Vacuum(collectionChangedMonitors);
        }

        class PropertyChangedMonitor
        {
            public PropertyChangedMonitor(StateMonitor owner, INotifyPropertyChanged target)
            {
                this.owner = owner;
                target.PropertyChanged += OnPropertyChanged;
            }

            private readonly StateMonitor owner;

            private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                owner.StateHasChanged = true;
                if (e.PropertyName == null) return;

                var value = sender?.GetType()?.GetProperty(e.PropertyName)?.GetValue(sender);
                if (value is INotifyPropertyChanged notifyPropertyChanged)
                    owner.Add(notifyPropertyChanged);
            }
        }

        class CollectionChangedMonitor
        {
            public CollectionChangedMonitor(StateMonitor owner, INotifyCollectionChanged target)
            {
                this.owner = owner;
                target.CollectionChanged += OnCollectionChanged;
            }

            private readonly StateMonitor owner;

            private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                owner.StateHasChanged = true;
                if (e.NewItems != null)
                    foreach (var value in e.NewItems)
                        owner.Add(value);
            }
        }
    }
}
