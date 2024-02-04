using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BankAccount.Command
{
    public class RelayCommand : ICommand
    {
        private readonly Func<object, Task> _ExecuteAsync;
        private readonly Func<object, bool> _CanExecute;

        public RelayCommand(Func<object, Task> ExecuteAsync, Func<object, bool> CanExecute = null)
        {
            _ExecuteAsync = ExecuteAsync ?? throw new ArgumentNullException(nameof(ExecuteAsync));
            _CanExecute = CanExecute;
        }

        public bool CanExecute(object parameter) => _CanExecute?.Invoke(parameter) ?? true;

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        private async Task ExecuteAsync(object parameter)
        {
            await _ExecuteAsync(parameter);
        }

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }  
}
