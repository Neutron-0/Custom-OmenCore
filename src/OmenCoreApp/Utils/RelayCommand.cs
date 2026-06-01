using System;
using System.Threading;
using System.Windows.Input;

namespace OmenCore.Utils
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;
        private int _raiseCanExecuteQueued;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged()
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                Interlocked.Exchange(ref _raiseCanExecuteQueued, 0);
            }
            else
            {
                if (Interlocked.CompareExchange(ref _raiseCanExecuteQueued, 1, 0) != 0)
                {
                    return;
                }

                dispatcher.BeginInvoke(() =>
                {
                    Interlocked.Exchange(ref _raiseCanExecuteQueued, 0);
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                });
            }
        }
    }
}
