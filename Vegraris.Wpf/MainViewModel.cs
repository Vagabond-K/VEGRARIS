using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Vegraris.Wpf
{
    class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            dispatcher = SynchronizationContext.Current;
            Game.PropertyChanged += OnGamePropertyChanged;
            Game.LineCleared += (sender, e) => soundEffectPlayer.PlayLineClearEffect();
            ((INotifyCollectionChanged)Game.LockedTraces).CollectionChanged += (sender, e) =>
            {
                if (e.NewItems != null)
                    soundEffectPlayer.PlayLockDownEffect();
            };
        }

        private readonly SynchronizationContext? dispatcher;
        private readonly SoundEffectPlayer soundEffectPlayer = new();
        private bool isReady;

        private bool SetProperty<T>(ref T target, T value, [CallerMemberName] string? propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(target, value))
            {
                target = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public Game Game { get; } = new Game();
        public bool IsReady { get => isReady; set => SetProperty(ref isReady, value); }

        public ICommand? PlayCommand => this.GetCommand(() =>
        {
            if (IsReady) return;
            IsReady = true;
            PlayCommand?.RaiseCanExecuteChanged();
            Game.Initialize();
            soundEffectPlayer.PlayReadyMessage();
            Task.Run(async () =>
            {
                await Task.Delay(2000);
                dispatcher?.Send(state => Game.Play(), null);
                IsReady = false;
            });
        }, () => !Game.Playing && !IsReady);

        public ICommand? PauseCommand => this.GetCommand(Game.Pause, () => Game.Playing && !Game.Paused && !IsReady);
        public ICommand? ResumeCommand => this.GetCommand(Game.Resume, () => Game.Playing && Game.Paused && !IsReady);
        public ICommand? QuitCommand => this.GetCommand(Game.Quit, () => Game.Playing && !IsReady);

        private void OnGamePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Game.Playing):
                    PlayCommand?.RaiseCanExecuteChanged();
                    PauseCommand?.RaiseCanExecuteChanged();
                    ResumeCommand?.RaiseCanExecuteChanged();
                    QuitCommand?.RaiseCanExecuteChanged();
                    break;
                case nameof(Game.Paused):
                    PauseCommand?.RaiseCanExecuteChanged();
                    ResumeCommand?.RaiseCanExecuteChanged();
                    break;
            }
        }
    }
}
