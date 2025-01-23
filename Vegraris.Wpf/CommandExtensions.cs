using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Vegraris.Wpf
{
    static class CommandExtensions
    {
        class InstantCommand(Action action, Func<bool> canCommandFunc) : ICommand
        {
            private readonly Action action = action;
            private readonly Func<bool> canCommandFunc = canCommandFunc;

            public event EventHandler? CanExecuteChanged;

            public void Execute(object? parameter) => action?.Invoke();
            public bool CanExecute(object? parameter) => canCommandFunc?.Invoke() ?? true;
            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        static class CommandContainer<T>
        {
            private readonly static Dictionary<string, ICommand> commands = [];

            public static ICommand? GetCommand(Action action, Func<bool> canCommandFunc, string commandName)
                => commands.TryGetValue(commandName, out var command)
                ? command : commands[commandName] = new InstantCommand(action, canCommandFunc);
        }

        public static ICommand? GetCommand<T>(this T owner, Action action, Func<bool> canCommandFunc, [CallerMemberName] string? commandName = null)
            => commandName != null ? CommandContainer<T>.GetCommand(action, canCommandFunc, commandName) : null;

        public static void RaiseCanExecuteChanged<T>(this T command) where T : ICommand
            => (command as InstantCommand)?.RaiseCanExecuteChanged();
    }
}
