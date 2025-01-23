using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Vegraris.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var mainViewModel = new MainViewModel();
            var game = mainViewModel.Game;

            PreviewKeyDown += (sender, e) => e.Handled = game.Control(ToPlayerInput(e.Key), true);
            PreviewKeyUp += (sender, e) => e.Handled = game.Control(ToPlayerInput(e.Key), false);

            CompositionTarget.Rendering += (sender, e)
                => game.Update((e as RenderingEventArgs)?.RenderingTime ?? TimeSpan.FromTicks(Environment.TickCount64));

            DataContext = mainViewModel;
        }

        private static PlayerInput ToPlayerInput(Key key)
            => key switch
            {
                Key.Left => PlayerInput.ShiftLeft,
                Key.Right => PlayerInput.ShiftRight,
                Key.Up => PlayerInput.RotateCW,
                Key.Z or Key.LeftCtrl => PlayerInput.RotateCCW,
                Key.Down => PlayerInput.SoftDrop,
                Key.Space => PlayerInput.HardDrop,
                Key.C or Key.LeftShift or Key.RightShift => PlayerInput.Hold,
                _ => PlayerInput.None
            };
    }
}